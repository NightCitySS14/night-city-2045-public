using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Shared._NC.LobbyRewards;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;

namespace Content.Server._NC.LobbyRewards;

public sealed class LobbyRewardsEui : BaseEui
{
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public event Action? OnClose;

    /// <summary>Default maximum budget points per round for loadout selection.</summary>
    private const int DefaultMaxBudget = 150;

    private int _balance;
    private List<LobbyRewardItem> _inventory = new();

    public LobbyRewardsEui()
    {
        IoCManager.InjectDependencies(this);
    }

    public override EuiStateBase GetNewState()
    {
        // Calculate current budget from selected items
        var currentBudget = _inventory.Where(x => x.Selected).Sum(x => x.PointsCost);

        return new LobbyRewardsEuiState(
            _balance,
            DefaultMaxBudget,
            currentBudget,
            _inventory,
            GetMarketItems());
    }

    /// <summary>
    ///     Scans all entity prototypes with NcRewardMetadataComponent that have a MarketPrice set,
    ///     and builds the market listing sorted by price.
    /// </summary>
    private List<LobbyMarketItem> GetMarketItems()
    {
        var marketItems = new List<LobbyMarketItem>();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
        {
            if (proto.TryGetComponent<NcRewardMetadataComponent>(out var metadata))
            {
                if (metadata.MarketPrice.HasValue)
                {
                    marketItems.Add(new LobbyMarketItem(
                        proto.ID,
                        metadata.MarketPrice.Value,
                        metadata.PointsCost,
                        metadata.Category,
                        metadata.MarketTab));
                }
            }
        }

        return marketItems.OrderBy(x => x.Price).ToList();
    }

    public override async void Opened()
    {
        base.Opened();
        await RefreshData();
    }

    public override void Closed()
    {
        base.Closed();
        OnClose?.Invoke();
    }

    /// <summary>
    ///     Refreshes balance and inventory from the database, then pushes state to client.
    /// </summary>
    private async Task RefreshData()
    {
        _balance = await _dbManager.GetNightCoinsBalanceAsync(Player.UserId);

        var inventoryRecords = await _dbManager.GetMetaInventoryAsync(Player.UserId);
        _inventory = inventoryRecords.Select(x =>
        {
            // Look up PointsCost and Category from the entity prototype
            var pointsCost = 0;
            var category = "Misc";

            if (_prototypeManager.TryIndex<EntityPrototype>(x.ItemPrototype, out var proto) &&
                proto.TryGetComponent<NcRewardMetadataComponent>(out var metadata))
            {
                pointsCost = metadata.PointsCost;
                category = metadata.Category;
            }

            return new LobbyRewardItem(x.Id, x.ItemPrototype, x.Quantity, x.Selected, pointsCost, category);
        }).ToList();

        StateDirty();
    }

    public override async void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            // Toggle deployment selection for an inventory item, with budget validation
            case LobbyRewardSelectMessage selectMsg:
            {
                // If selecting (not deselecting), check budget
                if (selectMsg.Selected)
                {
                    var item = _inventory.FirstOrDefault(x => x.Id == selectMsg.ItemId);
                    if (item != null)
                    {
                        var currentBudget = _inventory.Where(x => x.Selected).Sum(x => x.PointsCost);
                        if (currentBudget + item.PointsCost > DefaultMaxBudget)
                            return; // Reject: would exceed budget
                    }
                }

                await _dbManager.SetMetaInventoryItemSelectedAsync(selectMsg.ItemId, selectMsg.Selected);
                await RefreshData();
                break;
            }

            // Purchase an item from the Night-Market: deduct NC and add to meta-inventory
            case LobbyRewardPurchaseMessage purchaseMsg:
            {
                if (!_prototypeManager.TryIndex<EntityPrototype>(purchaseMsg.PrototypeId, out var proto) ||
                    !proto.TryGetComponent<NcRewardMetadataComponent>(out var metadata) ||
                    !metadata.MarketPrice.HasValue)
                {
                    return;
                }

                // Reject if the player already owns this item
                if (_inventory.Any(x => x.PrototypeId == purchaseMsg.PrototypeId))
                    return;

                // Atomic deduction: TryDeduct returns false if insufficient funds
                var deducted = await _dbManager.TryDeductNightCoinsAsync(Player.UserId, metadata.MarketPrice.Value);
                if (!deducted)
                    return;

                // Add the purchased item to the player's meta-inventory
                await _dbManager.AddToMetaInventoryAsync(Player.UserId, purchaseMsg.PrototypeId);
                await RefreshData();
                break;
            }

            // Reset all selections (clear loadout)
            case LobbyRewardResetLoadoutMessage:
            {
                foreach (var item in _inventory.Where(x => x.Selected))
                {
                    await _dbManager.SetMetaInventoryItemSelectedAsync(item.Id, false);
                }
                await RefreshData();
                break;
            }

            // Manual refresh
            case LobbyRewardRefreshMessage:
            {
                await RefreshData();
                break;
            }
        }
    }
}
