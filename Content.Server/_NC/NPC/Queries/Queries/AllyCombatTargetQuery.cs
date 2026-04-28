using Content.Server.NPC.Components;
using Content.Shared.NPC;
using Content.Server.NPC.Queries.Queries;

namespace Content.Server._NC.NPC.Queries.Queries;

public sealed partial class AllyCombatTargetQuery : UtilityQuery
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override void CanExecute(EntityUid uid, Blackboard blackboard, List<EntityUid> entities)
    {
        // This query finds targets of nearby allies
        var factionSystem = _entManager.System<NpcFactionSystem>();
        var lookup = _entManager.System<EntityLookupSystem>();
        var transform = _entManager.System<SharedTransformSystem>();

        var xform = _entManager.GetComponent<TransformComponent>(uid);
        var worldPos = transform.GetWorldPosition(xform);
        
        // Find nearby allies
        foreach (var ally in lookup.GetEntitiesInRange(uid, 15f))
        {
            if (ally == uid) continue;
            if (!factionSystem.IsFriendly(uid, ally)) continue;

            // Check if ally has a target
            EntityUid? target = null;
            if (_entManager.TryGetComponent<NPCRangedCombatComponent>(ally, out var ranged))
            {
                target = ranged.Target;
            }
            else if (_entManager.TryGetComponent<NPCMeleeCombatComponent>(ally, out var melee))
            {
                target = melee.Target;
            }

            if (target != null && !entities.Contains(target.Value))
            {
                entities.Add(target.Value);
            }
        }
    }
}
