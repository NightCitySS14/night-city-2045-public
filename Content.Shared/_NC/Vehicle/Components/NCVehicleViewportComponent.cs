using System;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._NC.Vehicle.Components;

// Возвращаем оригинальное имя для совместимости с картами RMC-14
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RMCVehicleViewportComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Range = 15f;
}
