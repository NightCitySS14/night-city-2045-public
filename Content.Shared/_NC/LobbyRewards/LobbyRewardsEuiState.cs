using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._NC.LobbyRewards;

[Serializable, NetSerializable]
public sealed class LobbyRewardsEuiState : EuiStateBase
{
    public int NightCoinsBalance;
    public List<LobbyRewardItem> Inventory;

    public LobbyRewardsEuiState(int nightCoinsBalance, List<LobbyRewardItem> inventory)
    {
        NightCoinsBalance = nightCoinsBalance;
        Inventory = inventory;
    }
}

[Serializable, NetSerializable]
public sealed class LobbyRewardItem
{
    public int Id;
    public string PrototypeId;
    public int Quantity;
    public bool Selected;

    public LobbyRewardItem(int id, string prototypeId, int quantity, bool selected)
    {
        Id = id;
        PrototypeId = prototypeId;
        Quantity = quantity;
        Selected = selected;
    }
}

[Serializable, NetSerializable]
public sealed class LobbyRewardSelectMessage : EuiMessageBase
{
    public int ItemId;
    public bool Selected;

    public LobbyRewardSelectMessage(int itemId, bool selected)
    {
        ItemId = itemId;
        Selected = selected;
    }
}
