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

    private int _balance;
    private List<LobbyRewardItem> _inventory = new();

    public LobbyRewardsEui()
    {
        IoCManager.InjectDependencies(this);
    }

    public override EuiStateBase GetNewState()
    {
        return new LobbyRewardsEuiState(_balance, _inventory, GetMarketItems());
    }

    private List<LobbyMarketItem> GetMarketItems()
    {
        var marketItems = new List<LobbyMarketItem>();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
        {
            if (proto.TryGetComponent<NcRewardMetadataComponent>(out var metadata))
            {
                if (metadata.MarketPrice.HasValue)
                {
                    marketItems.Add(new LobbyMarketItem(proto.ID, metadata.MarketPrice.Value));
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

    private async Task RefreshData()
    {
        _balance = await _dbManager.GetNightCoinsBalanceAsync(Player.UserId);
        
        var inventoryRecords = await _dbManager.GetMetaInventoryAsync(Player.UserId);
        _inventory = inventoryRecords.Select(x =>
            new LobbyRewardItem(x.Id, x.ItemPrototype, x.Quantity, x.Selected)).ToList();

        StateDirty();
    }

    public override async void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is LobbyRewardSelectMessage selectMsg)
        {
            await _dbManager.SetMetaInventoryItemSelectedAsync(selectMsg.ItemId, selectMsg.Selected);
            await RefreshData();
        }
        else if (msg is LobbyRewardPurchaseMessage purchaseMsg)
        {
            if (!_prototypeManager.TryIndex<EntityPrototype>(purchaseMsg.PrototypeId, out var proto) ||
                !proto.TryGetComponent<NcRewardMetadataComponent>(out var metadata) ||
                !metadata.MarketPrice.HasValue)
            {
                return;
            }

            if (_balance >= metadata.MarketPrice.Value)
            {
                // In real scenario: deduct balance and add to DB
                // For now, refresh to simulate
                await RefreshData();
            }
        }
        else if (msg is LobbyRewardRefreshMessage)
        {
            await RefreshData();
        }
    }
}
