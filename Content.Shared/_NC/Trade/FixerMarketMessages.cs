using Robust.Shared.Serialization;

namespace Content.Shared._NC.Trade;

[Serializable, NetSerializable]
public sealed class FixerMarketStateMessage : BoundUserInterfaceState
{
    public int Balance { get; }
    public List<FixerMarketListingData> Listings { get; }
    public List<string> Categories { get; }

    public FixerMarketStateMessage(int balance, List<FixerMarketListingData> listings, List<string> categories)
    {
        Balance = balance;
        Listings = listings;
        Categories = categories;
    }
}

[Serializable, NetSerializable]
public sealed class FixerMarketListingData
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Category { get; }
    public int Price { get; }
    public string? Icon { get; }
    public int AvailableCount { get; }

    public FixerMarketListingData(string id, string name, string description, string category, int price, string? icon, int availableCount)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        Icon = icon;
        AvailableCount = availableCount;
    }
}

[Serializable, NetSerializable]
public sealed class FixerMarketBuyMessage : BoundUserInterfaceMessage
{
    public string ListingId { get; }
    public int Count { get; }

    public FixerMarketBuyMessage(string listingId, int count)
    {
        ListingId = listingId;
        Count = count;
    }
}

[Serializable, NetSerializable]
public sealed class FixerMarketRequestRefreshMessage : BoundUserInterfaceMessage { }
