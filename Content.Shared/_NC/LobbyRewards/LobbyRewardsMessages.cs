using Robust.Shared.Serialization;
using Robust.Shared.Network;

namespace Content.Shared._NC.LobbyRewards;

[Serializable, NetSerializable]
public sealed class RequestLobbyRewardsMessage : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class LobbyRewardsDataMessage : EntityEventArgs
{
    public int NightCoinsBalance;
    public List<LobbyRewardItem> Items;

    public LobbyRewardsDataMessage(int balance, List<LobbyRewardItem> items)
    {
        NightCoinsBalance = balance;
        Items = items;
    }
}

[Serializable, NetSerializable]
public sealed class LobbyRewardItem
{
    public int Id;
    public string Prototype;
    public int Quantity;
    public bool Selected;

    public LobbyRewardItem(int id, string prototype, int quantity, bool selected)
    {
        Id = id;
        Prototype = prototype;
        Quantity = quantity;
        Selected = selected;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleLobbyRewardMessage : EntityEventArgs
{
    public int ItemId;
    public bool Selected;

    public ToggleLobbyRewardMessage(int itemId, bool selected)
    {
        ItemId = itemId;
        Selected = selected;
    }
}
