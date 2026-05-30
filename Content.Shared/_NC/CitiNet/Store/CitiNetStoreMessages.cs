using Content.Shared._NC.CitiNet.Delivery;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._NC.CitiNet.Store;

[Serializable, NetSerializable]
public sealed class CitiNetStoreUpdateState : BoundUserInterfaceState
{
    public int Balance { get; }
    public List<CitiNetStoreCategoryData> Categories { get; }

    public CitiNetStoreUpdateState(int balance, List<CitiNetStoreCategoryData> categories)
    {
        Balance = balance;
        Categories = categories;
    }
}

[Serializable, NetSerializable]
public sealed class CitiNetStoreCategoryData
{
    public string Name { get; }
    public List<CitiNetStoreEntryData> Entries { get; }

    public CitiNetStoreCategoryData(string name, List<CitiNetStoreEntryData> entries)
    {
        Name = name;
        Entries = entries;
    }
}

[Serializable, NetSerializable]
public sealed class CitiNetStoreEntryData
{
    public string Id { get; }
    public string ProtoId { get; }
    public string Name { get; }
    public string Description { get; }
    public int Price { get; }
    public int? RemainingCount { get; }

    public CitiNetStoreEntryData(string id, string protoId, string name, string description, int price, int? remainingCount)
    {
        Id = id;
        ProtoId = protoId;
        Name = name;
        Description = description;
        Price = price;
        RemainingCount = remainingCount;
    }
}

[Serializable, NetSerializable]
public sealed class CitiNetStoreBuyRequestMessage : BoundUserInterfaceMessage
{
    public string CategoryId { get; }
    public string EntryProtoId { get; }
    public DropType DeliveryType { get; }

    public CitiNetStoreBuyRequestMessage(string categoryId, string entryProtoId, DropType deliveryType)
    {
        CategoryId = categoryId;
        EntryProtoId = entryProtoId;
        DeliveryType = deliveryType;
    }
}

[Serializable, NetSerializable]
public sealed class CitiNetStoreRequestDataMessage : BoundUserInterfaceMessage
{
}
