using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Shared._NC.LobbyRewards;
using Content.Shared.Eui;

namespace Content.Server._NC.LobbyRewards;

public sealed class LobbyRewardsEui : BaseEui
{
    [Dependency] private readonly IServerDbManager _dbManager = default!;

    private int _balance;
    private List<LobbyRewardItem> _inventory = new();

    public LobbyRewardsEui()
    {
        IoCManager.InjectDependencies(this);
    }

    public override EuiStateBase GetNewState()
    {
        return new LobbyRewardsEuiState(_balance, _inventory);
    }

    public override async void Opened()
    {
        base.Opened();
        await RefreshData();
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
    }
}
