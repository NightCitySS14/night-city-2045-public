using Content.Server.Hands.Systems;
using Content.Server.NPC.Components;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._NC.RTS.Components;
using Content.Shared._NC.RTS.Events;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Map;

namespace Content.Server._NC.RTS.Systems;

/// <summary>
/// Keeps retaliation for RTS-controlled NPCs aligned with the weapon they are
/// actually holding instead of letting the generic hostile HTN fall into melee.
/// </summary>
public sealed class RTSRetaliationCombatSystem : EntitySystem
{
    private const string TargetKey = "Target";
    private const string TargetCoordinatesKey = "TargetCoordinates";

    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly NPCSteeringSystem _steering = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RTSControllableComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<RTSControllableComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased ||
            args.Origin is not { } attacker ||
            !HasComp<NPCRetaliationComponent>(ent.Owner) ||
            !HasComp<MobStateComponent>(attacker))
        {
            return;
        }

        // A direct move order explicitly ignores enemies and aggression.
        if (ent.Comp.ActiveCommand == RTSCommandType.Move)
            return;

        if (TryComp<NPCRetaliationComponent>(ent.Owner, out var retaliation) &&
            !retaliation.RetaliateFriendlies &&
            _faction.IsEntityFriendly(ent.Owner, attacker))
        {
            return;
        }

        if (!_hands.TryGetActiveItem(ent.Owner, out var heldItem) ||
            !HasComp<GunComponent>(heldItem))
        {
            return;
        }

        _steering.Unregister(ent.Owner);
        RemComp<NPCMeleeCombatComponent>(ent.Owner);

        var ranged = EnsureComp<NPCRangedCombatComponent>(ent.Owner);
        ranged.Target = attacker;

        _combatMode.SetInCombatMode(ent.Owner, true);

        if (!TryComp<HTNComponent>(ent.Owner, out var htn))
            return;

        htn.Blackboard.SetValue(TargetKey, attacker);
        htn.Blackboard.SetValue(TargetCoordinatesKey, Transform(attacker).Coordinates);

        if (htn.Plan != null)
            _htn.ShutdownPlan(htn);

        _htn.Replan(htn);
    }
}
