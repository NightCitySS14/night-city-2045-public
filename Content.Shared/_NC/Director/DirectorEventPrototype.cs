using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._NC.Director;

/// <summary>
/// A prototype for an event managed by the Global Director.
/// </summary>
[Prototype("directorEvent")]
public sealed partial class DirectorEventPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// ID of the announcer to use for phase announcements.
    /// If null, the director's default is used.
    /// </summary>
    [DataField]
    public string? AnnouncerId;

    /// <summary>
    /// Color for phase announcements.
    /// If null, the director's default is used.
    /// </summary>
    [DataField]
    public Color? AnnouncementColor;

    /// <summary>
    /// Weight of this event for the random picker.
    /// </summary>
    [DataField]
    public float Weight { get; private set; } = 10f;

    [DataField]
    public List<DirectorPhase> Phases { get; private set; } = new();
}

[DataDefinition]
public sealed partial class DirectorPhase
{
    [DataField]
    public string Name { get; private set; } = "Unnamed Phase";

    /// <summary>
    /// How long this phase lasts before automatically progressing.
    /// If null, the phase will not progress automatically by time.
    /// </summary>
    [DataField]
    public TimeSpan? Duration;

    /// <summary>
    /// Locale string for the announcement when this phase starts.
    /// </summary>
    [DataField]
    public string? Announcement;

    /// <summary>
    /// Entities to spawn at the start of this phase.
    /// </summary>
    [DataField]
    public List<string> Spawns { get; private set; } = new();

    /// <summary>
    /// List of triggers that can advance this phase.
    /// </summary>
    [DataField]
    public List<DirectorTrigger> Triggers { get; private set; } = new();
}

[DataDefinition]
public sealed partial class DirectorTrigger
{
    [DataField]
    public DirectorTriggerType Type { get; private set; } = DirectorTriggerType.None;

    /// <summary>
    /// Optional target ID for the trigger (e.g., prototype ID or a tag).
    /// </summary>
    [DataField]
    public string? Target { get; private set; }
}

public enum DirectorTriggerType : byte
{
    None = 0,
    
    /// <summary>
    /// Advances when any spawned entity from this event is killed.
    /// If Target is specified, only that prototype advances it.
    /// </summary>
    MobKilled,

    /// <summary>
    /// Advances when any spawned entity is destroyed.
    /// </summary>
    EntityDestroyed,
}
