using Content.Client._NC.LobbyRewards.UI;
using Content.Client.Eui;
using Content.Shared._NC.LobbyRewards;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client._NC.LobbyRewards;

[UsedImplicitly]
public sealed class LobbyRewardsEui : BaseEui
{
    private readonly LobbyRewardsWindow _window;

    public LobbyRewardsEui()
    {
        _window = new LobbyRewardsWindow();
        
        _window.OnItemSelected += (itemId, selected) =>
        {
            SendMessage(new LobbyRewardSelectMessage(itemId, selected));
        };

        _window.OnItemPurchase += (protoId) =>
        {
            SendMessage(new LobbyRewardPurchaseMessage(protoId));
        };

        _window.OnRefreshRequested += () =>
        {
            SendMessage(new LobbyRewardRefreshMessage());
        };

        _window.OnClose += () =>
        {
            SendMessage(new CloseEuiMessage());
        };
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        base.HandleState(state);

        if (state is not LobbyRewardsEuiState rewardsState)
            return;

        _window.UpdateState(rewardsState);
    }
}
