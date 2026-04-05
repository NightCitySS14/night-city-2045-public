using System;
using System.Collections.Generic;
using Content.Shared.Containers.ItemSlots;
using Content.Shared._NC.Vehicle.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.Shared._NC.Vehicle;

public readonly record struct NCVehicleMountedSlot(
    EntityUid Vehicle,
    EntityUid SlotOwner,
    string SlotId,
    string CompositeId,
    string HardpointType,
    EntityUid? Item,
    EntityUid? ParentItem,
    string? ParentSlotId)
{
    public bool HasItem => Item != null;
    public bool IsNested => ParentItem != null;
}

public sealed class NCVehicleTopologySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public bool TryGetVehicle(EntityUid uid, out EntityUid vehicle, bool includeSelf = true)
    {
        return TryGetContainerAncestor<VehicleComponent>(uid, out vehicle, includeSelf);
    }

    public bool TryGetParentTurret(EntityUid uid, out EntityUid turret, bool includeSelf = false)
    {
        return TryGetContainerAncestor<VehicleTurretComponent>(uid, out turret, includeSelf);
    }

    private bool TryGetContainerAncestor<TComponent>(EntityUid uid, out EntityUid ancestor, bool includeSelf = false)
        where TComponent : IComponent
    {
        ancestor = default;
        var query = GetEntityQuery<TComponent>();

        if (includeSelf && query.HasComp(uid))
        {
            ancestor = uid;
            return true;
        }

        var current = uid;
        while (_containers.TryGetContainingContainer((current, null), out var container))
        {
            var owner = container.Owner;
            if (query.HasComp(owner))
            {
                ancestor = owner;
                return true;
            }

            current = owner;
        }

        return false;
    }
}
