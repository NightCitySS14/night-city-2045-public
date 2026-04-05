using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._NC.Vehicle.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RMCVehicleInteriorComponent : Component
{
    public EntityUid Map = EntityUid.Invalid;
    public MapId MapId = MapId.Nullspace;
    public EntityCoordinates Entry;
    public EntityUid EntryParent = EntityUid.Invalid;
    public EntityUid Grid = EntityUid.Invalid;
    public HashSet<int> EntryLocks = new();
    public HashSet<EntityUid> Passengers = new();
}

[RegisterComponent, NetworkedComponent]
public sealed partial class RMCVehicleInteriorLinkComponent : Component
{
    public EntityUid Vehicle = EntityUid.Invalid;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class RMCVehicleInteriorOccupantComponent : Component
{
    [ViewVariables]
    public EntityUid Vehicle = EntityUid.Invalid;
}
