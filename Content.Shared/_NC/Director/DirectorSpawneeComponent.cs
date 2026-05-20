using Robust.Shared.GameStates;

namespace Content.Shared._NC.Director;

/// <summary>
/// Component attached to entities spawned by a Director event.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DirectorSpawneeComponent : Component
{
    /// <summary>
    /// The entity representing the Director event that spawned this.
    /// </summary>
    [DataField]
    public EntityUid EventEntity;
}
