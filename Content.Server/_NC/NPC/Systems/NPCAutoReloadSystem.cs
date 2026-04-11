// Content.Server/_NC/NPC/Systems/NPCAutoReloadSystem.cs
// System that automatically reloads ballistic weapons for NPCs
// when their magazine runs empty, searching pockets for spare mags.

using Content.Shared._NC.NPC;
using Content.Shared._NC.Weapons.Ranged.NCWeapon;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Server._NC.NPC.Systems;

/// <summary>
///     Periodically checks if an NPC's held gun is empty and attempts
///     to reload it from inventory (pockets). Works with magazine-fed weapons
///     (MagazineAmmoProvider / ChamberMagazineAmmoProvider).
/// </summary>
public sealed class NPCAutoReloadSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;

    // Slot IDs used by ballistic weapons in SS14.
    private const string MagazineSlot = "gun_magazine";
    private const string ChamberSlot = "gun_chamber";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NPCAutoReloadComponent, HandsComponent>();

        while (query.MoveNext(out var uid, out var reload, out var hands))
        {
            // 1. Get whatever the NPC is holding in its active hand.
            if (hands.ActiveHandEntity is not { } heldEntity)
                continue;

            // 1.5 Handle Jamming, Racking and Empty Chambers
            if (TryComp<ChamberMagazineAmmoProviderComponent>(heldEntity, out var chamber))
            {
                // A. Check for NC Jamming first. If jammed, NPC must start unjamming.
                if (TryComp<NCWeaponComponent>(heldEntity, out var ncWeapon) && ncWeapon.IsJammed)
                {
                    var useEv = new UseInHandEvent(uid);
                    RaiseLocalEvent(heldEntity, useEv);
                }
                // B. Check if bolt is open OR null (effectively open/unracked).
                else if (chamber.BoltClosed != true && chamber.CanRack)
                {
                    _gunSystem.SetBoltClosed(heldEntity, chamber, true, uid);
                }
                // C. Check if bolt is closed but chamber is empty (Dry Fire condition).
                // If closed but no round in chamber, we need to rack it to cycle a new round from the mag.
                else if (chamber.BoltClosed == true && IsChamberEmpty(heldEntity))
                {
                    // Only rack if there's actually a magazine to cycle from.
                    if (_container.TryGetContainer(heldEntity, MagazineSlot, out var magContainer) &&
                        magContainer is ContainerSlot { ContainedEntity: not null })
                    {
                        // Open the bolt. The 'BoltClosed != true' check above will close it in the next tick,
                        // triggering CycleCartridge() and properly loading a round.
                        _gunSystem.SetBoltClosed(heldEntity, chamber, false, uid);
                    }
                }
            }

            // 2. Periodic Reload Check (Inventory search is expensive)
            reload.Accumulator += frameTime;
            if (reload.Accumulator < reload.CheckInterval)
                continue;

            reload.Accumulator = 0f;

            // 3. Check if the held item is a gun with a magazine slot.
            if (!HasComp<MagazineAmmoProviderComponent>(heldEntity) &&
                !HasComp<ChamberMagazineAmmoProviderComponent>(heldEntity))
                continue;

            // 4. Check current ammo count — if not empty, skip.
            var ammoEv = new GetAmmoCountEvent();
            RaiseLocalEvent(heldEntity, ref ammoEv);

            if (ammoEv.Count > 0)
                continue;

            // 5. Gun is empty. Try to eject the current (empty) magazine first.
            if (_container.TryGetContainer(heldEntity, MagazineSlot, out var magSlotContainer) &&
                magSlotContainer is ContainerSlot { ContainedEntity: not null })
            {
                _itemSlots.TryEject(heldEntity, MagazineSlot, uid, out _, excludeUserAudio: true);
            }

            // 6. Search the NPC's inventory for a spare magazine.
            var newMag = FindMagazineInInventory(uid, heldEntity);
            if (newMag == null)
                continue;

            // 7. Insert the new magazine.
            if (_itemSlots.TryInsert(heldEntity, MagazineSlot, newMag.Value, uid, excludeUserAudio: true))
            {
                // No need to rack here anymore, the every-frame check above handles it.
                break; 
            }
        }
    }

    /// <summary>
    ///     Checks if the weapon's chamber slot is empty.
    /// </summary>
    private bool IsChamberEmpty(EntityUid gun)
    {
        return _container.TryGetContainer(gun, ChamberSlot, out var container) &&
               container is ContainerSlot slot &&
               slot.ContainedEntity == null;
    }

    private EntityUid? FindMagazineInInventory(EntityUid npc, EntityUid gun)
    {
        if (!_itemSlots.TryGetSlot(gun, MagazineSlot, out var itemSlot))
            return null;

        var enumerator = _inventory.GetHandOrInventoryEntities(npc);

        foreach (var item in enumerator)
        {
            if (item == gun)
                continue;

            if (_itemSlots.CanInsert(gun, item, null, itemSlot))
                return item;
        }

        return null;
    }
}
