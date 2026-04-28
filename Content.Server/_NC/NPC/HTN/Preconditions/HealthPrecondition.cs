using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;

namespace Content.Server.NPC.HTN.Preconditions;

public sealed partial class HealthPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField("minThreshold")]
    public float MinThreshold = 0f;

    [DataField("maxThreshold")]
    public float MaxThreshold = 1f;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_entManager.TryGetComponent<DamageableComponent>(owner, out var damageable))
            return false;

        var thresholdSystem = _entManager.System<MobThresholdSystem>();
        if (!thresholdSystem.TryGetIncapPercentage(owner, damageable.TotalDamage, out var percentage))
            return false;

        var health = 1.0f - (float) percentage.Value;

        return health >= MinThreshold && health <= MaxThreshold;
    }
}
