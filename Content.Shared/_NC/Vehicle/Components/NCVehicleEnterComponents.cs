using System.Numerics;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._NC.Vehicle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleEnterComponent : Component
{
    [DataField, AutoNetworkedField]
    public string InteriorPath = string.Empty;

    [DataField, AutoNetworkedField]
    public float EnterDoAfter = 1.0f;

    [DataField, AutoNetworkedField]
    public float ExitDoAfter = 1.0f;

    [DataField, AutoNetworkedField]
    public List<VehicleEntryPoint> EntryPoints = new();

    [DataField, AutoNetworkedField]
    public int MaxPassengers = 4;

    [DataField, AutoNetworkedField]
    public Vector2 ExitOffset = new(0, -2);
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class VehicleEntryPoint
{
    [DataField]
    public Vector2 Offset;

    [DataField]
    public float Radius = 0.5f;

    [DataField]
    public Vector2? InteriorCoords;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class VehicleExitComponent : Component
{
    public bool PendingExit = false;
    public int EntryIndex = -1;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class VehicleDriverSeatComponent : Component { }

[Serializable, NetSerializable]
public sealed partial class VehicleEnterDoAfterEvent : SimpleDoAfterEvent
{
    public int EntryIndex;
}

[Serializable, NetSerializable]
public sealed partial class VehicleExitDoAfterEvent : SimpleDoAfterEvent { }
