// D:\projects\night-station\Content.Server\_NC\NPC\HTN\PrimitiveTasks\Operators\Interactions\NPCUseInHandOperator.cs
using System.Threading;
using System.Threading.Tasks;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Interactions;

/// <summary>
/// NPC operator that triggers UseInHand (mimics pressing 'Z') on whatever is held in the active hand.
/// </summary>
public sealed partial class NPCUseInHandOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_entManager.TryGetComponent<HandsComponent>(owner, out var hands) || hands.ActiveHand?.HeldEntity == null)
            return (false, null);

        return (true, null);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var interactionSystem = _entManager.System<SharedInteractionSystem>();

        if (!_entManager.TryGetComponent<HandsComponent>(owner, out var hands) || hands.ActiveHand?.HeldEntity == null)
            return HTNOperatorStatus.Failed;

        var used = hands.ActiveHand.HeldEntity.Value;

        // Trigger UseInHand (Z press)
        interactionSystem.UseInHandInteraction(owner, used, checkCanUse: false, checkCanInteract: false);

        return HTNOperatorStatus.Finished;
    }
}
