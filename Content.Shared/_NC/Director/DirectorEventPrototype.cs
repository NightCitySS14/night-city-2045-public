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
    /// The ID of the phase to start with.
    /// </summary>
    [DataField]
    public string StartPhase { get; private set; } = "Start";

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
    public Dictionary<string, DirectorPhase> Phases { get; private set; } = new();
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
    /// Tag for spawn points where entities should be spawned.
    /// </summary>
    [DataField]
    public string? LocationTag;

    /// <summary>
    /// Optional tag for where entities should spawn initially.
    /// If null, LocationTag is used with a random offset.
    /// </summary>
    [DataField]
    public string? SpawnTag;

    /// <summary>
    /// HTN Domain to apply to all spawned entities at the start of this phase.
    /// </summary>
    [DataField]
    public string? AiDomain;

    /// <summary>
    /// If true, all spawned entities will be deleted at the END of this phase.
    /// </summary>
    [DataField]
    public bool Cleanup;

    /// <summary>
    /// Entities to spawn at the start of this phase.
    /// </summary>
    [DataField]
    public List<DirectorSpawnGroup> Spawns { get; private set; } = new();

    /// <summary>
    /// List of triggers that can advance this phase.
    /// </summary>
    [DataField]
    public List<DirectorTrigger> Triggers { get; private set; } = new();

    /// <summary>
    /// Possible next phases with their respective weights.
    /// Key is the phase ID from the prototype's Phases dictionary.
    /// </summary>
    [DataField]
    public Dictionary<string, float> NextPhases { get; private set; } = new();

    /// <summary>
    /// Faction IDs to apply to specific groups at the start of this phase.
    /// Key is the GroupTag defined in DirectorSpawnGroup.
    /// </summary>
    [DataField]
    public Dictionary<string, string> FactionOverrides { get; private set; } = new();
}

[DataDefinition]
public sealed partial class DirectorSpawnGroup
{
    /// <summary>
    /// Prototype ID of the entity to spawn.
    /// </summary>
    [DataField("prototype", required: true)]
    public string Prototype { get; private set; } = string.Empty;

    /// <summary>
    /// Optional tag to identify this group for faction overrides or AI changes.
    /// </summary>
    [DataField]
    public string? GroupTag;

    /// <summary>
    /// Faction ID to assign to the spawned entity (e.g., "Maelstrom", "Valentino").
    /// </summary>
    [DataField]
    public string? Faction;

    /// <summary>
    /// Number of entities of this prototype to spawn.
    /// </summary>
    [DataField]
    public int Amount = 1;
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

    /// <summary>
    /// Number of occurrences required to activate the trigger.
    /// </summary>
    [DataField]
    public int Count { get; private set; } = 1;
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
