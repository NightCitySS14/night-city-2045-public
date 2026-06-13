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

    /// <summary>
    /// Tag to identify which group this entity belongs to (e.g., "GroupA").
    /// </summary>
    [DataField]
    public string? GroupTag;

    /// <summary>
    /// Phase id that originally spawned this entity.
    /// Stored so trigger processing cannot leak across later phases.
    /// </summary>
    [DataField]
    public string? PhaseId;

    /// <summary>
    /// Monotonic phase sequence of the parent event when this entity was spawned.
    /// </summary>
    [DataField]
    public int PhaseSequence;
}
