using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._NC.Vehicle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RMCBuckleOffsetComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class RMCStrapNoDrawDepthChangeComponent : Component { }
