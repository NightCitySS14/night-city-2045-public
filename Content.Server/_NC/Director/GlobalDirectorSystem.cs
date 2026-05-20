using System.Linq;
using Content.Server.Announcements.Systems;
using Content.Shared._NC.Director;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._NC.Director;

/// <summary>
/// Manages the Global Director System (Living World).
/// Handles scheduling of events and their phase transitions.
/// </summary>
public sealed class GlobalDirectorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("director");

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<EntityTerminatingEvent>(OnEntityTerminating);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // 1. Manage Global Director Scheduler
        var directorQuery = EntityQueryEnumerator<GlobalDirectorComponent>();
        while (directorQuery.MoveNext(out var uid, out var director))
        {
            if (!director.Enabled)
                continue;

            if (_timing.CurTime < director.NextCheckTime)
                continue;

            // Time to try starting a new event
            if (TryStartRandomEvent())
            {
                _sawmill.Info("Director started a new random event.");
            }
            
            ResetDirectorTimer(director);
        }

        // 2. Advance Active Events based on timer
        var eventQuery = EntityQueryEnumerator<DirectorEventComponent>();
        while (eventQuery.MoveNext(out var uid, out var directorEvent))
        {
            if (directorEvent.PhaseEndTime != null && _timing.CurTime >= directorEvent.PhaseEndTime)
            {
                AdvancePhase(uid, directorEvent);
            }
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (ev.NewMobState != MobState.Dead)
            return;

        if (!TryComp<DirectorSpawneeComponent>(ev.Target, out var spawnee))
            return;

        if (!TryComp<DirectorEventComponent>(spawnee.EventEntity, out var directorEvent))
            return;

        if (CheckTriggers(spawnee.EventEntity, directorEvent, DirectorTriggerType.MobKilled, ev.Target))
        {
            AdvancePhase(spawnee.EventEntity, directorEvent);
        }
    }

    private void OnEntityTerminating(ref EntityTerminatingEvent ev)
    {
        if (!TryComp<DirectorSpawneeComponent>(ev.Entity, out var spawnee))
            return;

        if (!TryComp<DirectorEventComponent>(spawnee.EventEntity, out var directorEvent))
            return;

        // Cleanup the list in the event component
        directorEvent.SpawnedEntities.Remove(ev.Entity);

        if (CheckTriggers(spawnee.EventEntity, directorEvent, DirectorTriggerType.EntityDestroyed, ev.Entity))
        {
            AdvancePhase(spawnee.EventEntity, directorEvent);
        }
    }

    private bool CheckTriggers(EntityUid eventUid, DirectorEventComponent component, DirectorTriggerType type, EntityUid targetUid)
    {
        if (!_prototype.TryIndex<DirectorEventPrototype>(component.PrototypeId, out var proto))
            return false;

        if (component.CurrentPhase < 0 || component.CurrentPhase >= proto.Phases.Count)
            return false;

        var phase = proto.Phases[component.CurrentPhase];
        foreach (var trigger in phase.Triggers)
        {
            if (trigger.Type != type)
                continue;

            if (trigger.Target != null)
            {
                var targetProto = MetaData(targetUid).EntityPrototype?.ID;
                if (targetProto != trigger.Target)
                    continue;
            }

            return true;
        }

        return false;
    }

    private void ResetDirectorTimer(GlobalDirectorComponent director)
    {
        var delay = _random.Next(director.MinDelay, director.MaxDelay);
        director.NextCheckTime = _timing.CurTime + delay;
    }

    /// <summary>
    /// Attempts to start a random director event.
    /// </summary>
    public bool TryStartRandomEvent()
    {
        var prototypes = _prototype.EnumeratePrototypes<DirectorEventPrototype>().ToList();
        if (prototypes.Count == 0)
            return false;

        var totalWeight = prototypes.Sum(p => p.Weight);
        if (totalWeight <= 0)
            return false;

        var pick = _random.NextFloat(totalWeight);
        foreach (var proto in prototypes)
        {
            pick -= proto.Weight;
            if (pick <= 0)
            {
                StartEvent(proto);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Starts a specific director event.
    /// </summary>
    public EntityUid StartEvent(DirectorEventPrototype proto)
    {
        var uid = EntityManager.SpawnEntity(null, MapCoordinates.Nullspace);
        var directorEvent = EnsureComp<DirectorEventComponent>(uid);
        directorEvent.PrototypeId = proto.ID;
        directorEvent.CurrentPhase = -1; // So AdvancePhase moves to 0
        
        AdvancePhase(uid, directorEvent);
        return uid;
    }

    /// <summary>
    /// Force advances an event to the next phase.
    /// </summary>
    public void AdvancePhase(EntityUid uid, DirectorEventComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_prototype.TryIndex<DirectorEventPrototype>(component.PrototypeId, out var proto))
        {
            _sawmill.Error($"Failed to find prototype {component.PrototypeId} for event {ToPrettyString(uid)}");
            EntityManager.DeleteEntity(uid);
            return;
        }

        component.CurrentPhase++;

        if (component.CurrentPhase >= proto.Phases.Count)
        {
            _sawmill.Info($"Event {proto.Name} ({uid}) finished.");
            EntityManager.DeleteEntity(uid);
            return;
        }

        var phase = proto.Phases[component.CurrentPhase];

        // Get director settings for defaults
        var directorQuery = EntityQueryEnumerator<GlobalDirectorComponent>();
        var (announcerId, announcementColor) = ("Director", Color.Cyan);
        if (directorQuery.MoveNext(out _, out var director))
        {
            announcerId = proto.AnnouncerId ?? director.DefaultAnnouncerId;
            announcementColor = proto.AnnouncementColor ?? director.AnnouncementColor;
        }

        // Handle Spawns
        if (phase.Spawns.Count > 0)
        {
            var coords = GetSpawnLocation();
            if (coords.IsValid(EntityManager))
            {
                foreach (var spawnProto in phase.Spawns)
                {
                    var spawned = EntityManager.SpawnEntity(spawnProto, coords);
                    var spawnee = EnsureComp<DirectorSpawneeComponent>(spawned);
                    spawnee.EventEntity = uid;
                    component.SpawnedEntities.Add(spawned);
                }
            }
            else
            {
                _sawmill.Warning($"Could not find a valid spawn location for event {proto.Name} ({uid}) phase {phase.Name}");
            }
        }

        // Handle Announcement
        if (phase.Announcement != null)
        {
            _announcer.SendAnnouncement(
                announcerId,
                Loc.GetString(phase.Announcement),
                colorOverride: announcementColor
            );
        }

        // Set timer for next phase
        if (phase.Duration != null)
        {
            component.PhaseEndTime = _timing.CurTime + phase.Duration.Value;
        }
        else
        {
            component.PhaseEndTime = null;
        }
        
        Dirty(uid, component);
        _sawmill.Debug($"Event {proto.Name} ({uid}) advanced to phase {component.CurrentPhase}: {phase.Name}");
    }

    private EntityCoordinates GetSpawnLocation()
    {
        var query = EntityQueryEnumerator<DirectorSpawnPointComponent, TransformComponent>();
        var points = new List<EntityCoordinates>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            points.Add(xform.Coordinates);
        }

        if (points.Count > 0)
        {
            return _random.Pick(points);
        }

        return EntityCoordinates.Invalid;
    }

    /// <summary>
    /// Cancels an active event.
    /// </summary>
    public void CancelEvent(EntityUid uid)
    {
        if (HasComp<DirectorEventComponent>(uid))
        {
            _sawmill.Info($"Event {uid} cancelled.");
            EntityManager.DeleteEntity(uid);
        }
    }
}
