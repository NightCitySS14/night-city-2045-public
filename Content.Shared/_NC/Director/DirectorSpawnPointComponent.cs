namespace Content.Shared._NC.Director;

/// <summary>
/// Marker component for locations where Director events can spawn entities.
/// </summary>
[RegisterComponent]
public sealed partial class DirectorSpawnPointComponent : Component
{
    /// <summary>
    /// Tag used to categorize this spawn point (e.g., "Maintenance", "Alley", "Hidden").
    /// </summary>
    [DataField]
    public string? LocationTag;
}
