using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._NC.LobbyRewards;

// ── EUI State ────────────────────────────────────────────────────────────────

[Serializable, NetSerializable]
public sealed class LobbyRewardsEuiState : EuiStateBase
{
    /// <summary>Night Coins balance for market purchases.</summary>
    public int NightCoinsBalance;

    /// <summary>Maximum loadout budget points per round.</summary>
    public int MaxBudget;

    /// <summary>Currently spent budget points based on selected items.</summary>
    public int CurrentBudget;

    /// <summary>Player's owned items (Storage).</summary>
    public List<LobbyRewardItem> Inventory;

    /// <summary>Items available for purchase in the Night-Market.</summary>
    public List<LobbyMarketItem> MarketItems;

    public LobbyRewardsEuiState(
        int nightCoinsBalance,
        int maxBudget,
        int currentBudget,
        List<LobbyRewardItem> inventory,
        List<LobbyMarketItem> marketItems)
    {
        NightCoinsBalance = nightCoinsBalance;
        MaxBudget = maxBudget;
        CurrentBudget = currentBudget;
        Inventory = inventory;
        MarketItems = marketItems;
    }
}

// ── Data Records ─────────────────────────────────────────────────────────────

[Serializable, NetSerializable]
public sealed class LobbyRewardItem
{
    public int Id;
    public string PrototypeId;
    public int Quantity;
    public bool Selected;
    public int PointsCost;
    public string Category;

    public LobbyRewardItem(int id, string prototypeId, int quantity, bool selected, int pointsCost, string category)
    {
        Id = id;
        PrototypeId = prototypeId;
        Quantity = quantity;
        Selected = selected;
        PointsCost = pointsCost;
        Category = category;
    }
}

[Serializable, NetSerializable]
public sealed class LobbyMarketItem
{
    public string PrototypeId;
    public int Price;
    public int PointsCost;
    public string Category;
    public string MarketTab;

    public LobbyMarketItem(string prototypeId, int price, int pointsCost, string category, string marketTab)
    {
        PrototypeId = prototypeId;
        Price = price;
        PointsCost = pointsCost;
        Category = category;
        MarketTab = marketTab;
    }
}

// ── EUI Messages (Client → Server) ──────────────────────────────────────────

/// <summary>Toggle the "selected for deployment" flag on an inventory item.</summary>
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

/// <summary>Purchase a new item from the Night-Market.</summary>
[Serializable, NetSerializable]
public sealed class LobbyRewardPurchaseMessage : EuiMessageBase
{
    public string PrototypeId;

    public LobbyRewardPurchaseMessage(string prototypeId)
    {
        PrototypeId = prototypeId;
    }
}

/// <summary>Force-refresh the UI data from the database.</summary>
[Serializable, NetSerializable]
public sealed class LobbyRewardRefreshMessage : EuiMessageBase
{
}

/// <summary>Deselect all items in the loadout (reset budget).</summary>
[Serializable, NetSerializable]
public sealed class LobbyRewardResetLoadoutMessage : EuiMessageBase
{
}
