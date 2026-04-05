using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared._NC.Vehicle.Components;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using Robust.Shared.Network;
using Robust.Shared.EntitySerialization;

namespace Content.Shared._NC.Vehicle;

public sealed partial class VehicleSystem
{
    private void InitializeInterior()
    {
        SubscribeLocalEvent<VehicleEnterComponent, ActivateInWorldEvent>(OnVehicleEnterActivate);
        SubscribeLocalEvent<VehicleExitComponent, ActivateInWorldEvent>(OnVehicleExitActivate);
        SubscribeLocalEvent<VehicleEnterComponent, VehicleEnterDoAfterEvent>(OnVehicleEnterDoAfter);
        SubscribeLocalEvent<VehicleExitComponent, VehicleExitDoAfterEvent>(OnVehicleExitDoAfter);
    }

    private void OnOccupantStartup(Entity<RMCVehicleInteriorOccupantComponent> ent, ref ComponentStartup args)
    {
        _meta.AddFlag(ent, MetaDataFlags.ExtraTransformEvents);
    }

    private void OnOccupantRemove(Entity<RMCVehicleInteriorOccupantComponent> ent, ref ComponentRemove args)
    {
        _meta.RemoveFlag(ent, MetaDataFlags.ExtraTransformEvents);

        if (ent.Comp.Vehicle.IsValid())
            UnregisterTrackedOccupant(ent.Comp.Vehicle, ent.Owner);
    }

    private void OnOccupantMapChanged(Entity<RMCVehicleInteriorOccupantComponent> ent, ref MapUidChangedEvent args)
    {
        if (ent.Comp.Vehicle == EntityUid.Invalid)
            return;

        if (TryComp(ent.Comp.Vehicle, out RMCVehicleInteriorComponent? interior) &&
            args.NewMapId == interior.MapId)
        {
            return;
        }

        RemCompDeferred<RMCVehicleInteriorOccupantComponent>(ent.Owner);
    }

    private void OnVehicleEnterActivate(Entity<VehicleEnterComponent> ent, ref ActivateInWorldEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.Handled)
            return;

        // ТОЧНЫЙ ПЕРЕНОС: Поиск входа
        if (!TryFindEntry(ent, args.User, out var entryIndex))
        {
            _popup.PopupEntity("Используйте дверной проем!", args.User, args.User);
            return;
        }

        var interior = EnsureComp<RMCVehicleInteriorComponent>(ent.Owner);

        if (!interior.EntryLocks.Add(entryIndex))
        {
            _popup.PopupEntity("Вход занят!", args.User, args.User);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, args.User, ent.Comp.EnterDoAfter, new VehicleEnterDoAfterEvent { EntryIndex = entryIndex }, ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            interior.EntryLocks.Remove(entryIndex);
            return;
        }

        args.Handled = true;
    }

    private void OnVehicleEnterDoAfter(Entity<VehicleEnterComponent> ent, ref VehicleEnterDoAfterEvent args)
    {
        if (TryComp(ent.Owner, out RMCVehicleInteriorComponent? interior))
            interior.EntryLocks.Remove(args.EntryIndex);

        if (args.Cancelled || args.Handled)
            return;

        args.Handled = TryEnter(ent, args.User, args.EntryIndex);
    }

    private bool TryEnter(Entity<VehicleEnterComponent> ent, EntityUid user, int entryIndex = -1)
    {
        if (!EnsureInterior(ent, out var interior))
            return false;

        if (ent.Comp.MaxPassengers > 0 && interior.Passengers.Count >= ent.Comp.MaxPassengers)
        {
            _popup.PopupEntity("Машина полна!", user, user);
            return false;
        }

        // ТОЧНЫЙ ПЕРЕНОС ЛОГИКИ ТЕЛЕПОРТАЦИИ RMC
        var parent = interior.Grid.IsValid() ? interior.Grid : interior.EntryParent;
        Vector2 interiorPos;

        if (entryIndex >= 0 && entryIndex < ent.Comp.EntryPoints.Count)
        {
            var entryPoint = ent.Comp.EntryPoints[entryIndex];
            interiorPos = entryPoint.InteriorCoords ?? Vector2.Zero;
        }
        else
        {
            interiorPos = interior.Entry.Position;
            parent = interior.Entry.EntityId.IsValid() ? interior.Entry.EntityId : parent;
        }

        // Вычисляем мировые координаты на целевой карте (как делает RMC)
        var entityCoords = new EntityCoordinates(parent, interiorPos);
        var targetMapCoords = _transform.ToMapCoordinates(entityCoords);

        // Используем SetMapCoordinates (аналог HandlePulling в RMC)
        _transform.SetMapCoordinates(user, targetMapCoords);
        
        TrackOccupant(user, ent.Owner);
        return true;
    }

    private bool EnsureInterior(Entity<VehicleEnterComponent> ent, [NotNullWhen(true)] out RMCVehicleInteriorComponent? interior)
    {
        if (TryComp(ent.Owner, out interior) &&
            interior.MapId != MapId.Nullspace &&
            _mapManager.MapExists(interior.MapId))
        {
            return true;
        }

        if (_net.IsClient)
        {
            interior = null;
            return false;
        }

        interior = EnsureComp<RMCVehicleInteriorComponent>(ent.Owner);

        var deserializeOptions = new DeserializationOptions { InitializeMaps = true };

        if (!_mapLoader.TryLoadMap(new ResPath(ent.Comp.InteriorPath), out var loadedMap, out _, deserializeOptions))
        {
            Log.Error($"[Vehicle] Failed to load interior {ent.Comp.InteriorPath}");
            return false;
        }

        if (loadedMap is not { } map)
            return false;

        var mapId = map.Comp.MapId;
        var mapUid = map.Owner;

        EntityUid entryParent = mapUid;
        EntityUid interiorGrid = EntityUid.Invalid;
        
        var gridEnum = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (gridEnum.MoveNext(out var gridUid, out _, out var gridXform))
        {
            if (gridXform.MapID != mapId) continue;
            entryParent = gridUid;
            interiorGrid = gridUid;
            break;
        }

        var entryCoords = new EntityCoordinates(entryParent, Vector2.Zero);
        var exitQuery = EntityQueryEnumerator<VehicleExitComponent, TransformComponent>();
        while (exitQuery.MoveNext(out var exitUid, out _, out var xform))
        {
            if (xform.MapID != mapId) continue;
            entryCoords = xform.Coordinates;
            entryParent = xform.ParentUid.IsValid() ? xform.ParentUid : entryParent;
            break;
        }

        interior.Map = mapUid;
        interior.MapId = mapId;
        interior.Entry = entryCoords;
        interior.EntryParent = entryParent;
        interior.Grid = interiorGrid;

        var link = EnsureComp<RMCVehicleInteriorLinkComponent>(mapUid);
        link.Vehicle = ent.Owner;

        return true;
    }

    private void TrackOccupant(EntityUid user, EntityUid vehicle)
    {
        var occupant = EnsureComp<RMCVehicleInteriorOccupantComponent>(user);
        occupant.Vehicle = vehicle;
        
        if (TryComp(vehicle, out RMCVehicleInteriorComponent? interior))
            interior.Passengers.Add(user);
    }

    private void UnregisterTrackedOccupant(EntityUid vehicle, EntityUid user)
    {
        if (TryComp(vehicle, out RMCVehicleInteriorComponent? interior))
            interior.Passengers.Remove(user);
    }

    private void CleanupInterior(EntityUid vehicle)
    {
        if (!TryComp(vehicle, out RMCVehicleInteriorComponent? interior))
            return;

        foreach (var passenger in new List<EntityUid>(interior.Passengers))
        {
            RemCompDeferred<RMCVehicleInteriorOccupantComponent>(passenger);
        }

        if (interior.MapId != MapId.Nullspace && _mapManager.MapExists(interior.MapId))
            _mapManager.DeleteMap(interior.MapId);

        RemComp<RMCVehicleInteriorComponent>(vehicle);
    }

    private bool TryFindEntry(Entity<VehicleEnterComponent> ent, EntityUid user, out int entryIndex)
    {
        entryIndex = -1;

        if (ent.Comp.EntryPoints.Count == 0)
            return true;

        var vehicleXform = Transform(ent.Owner);
        var userXform = Transform(user);

        if (vehicleXform.MapID != userXform.MapID) return false;

        // ТОЧНАЯ МАТЕМАТИКА RMC-14
        var vehiclePos = _transform.GetWorldPosition(vehicleXform);
        var userPos = _transform.GetWorldPosition(userXform);
        var delta = userPos - vehiclePos;
        
        // Инвертируем вращение машины, чтобы найти локальную дельту
        var localDelta = (-vehicleXform.LocalRotation).RotateVec(delta);

        for (var i = 0; i < ent.Comp.EntryPoints.Count; i++)
        {
            var entry = ent.Comp.EntryPoints[i];
            if ((localDelta - entry.Offset).Length() <= entry.Radius)
            {
                entryIndex = i;
                return true;
            }
        }
        return false;
    }

    private void OnVehicleExitActivate(Entity<VehicleExitComponent> ent, ref ActivateInWorldEvent args)
    {
        if (_net.IsClient) return;
        if (!TryGetVehicleFromInterior(ent.Owner, out var vehicleUid) || vehicleUid == null) return;
        if (!TryComp(vehicleUid.Value, out VehicleEnterComponent? enter)) return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, enter.ExitDoAfter, new VehicleExitDoAfterEvent(), ent.Owner));
    }

    private void OnVehicleExitDoAfter(Entity<VehicleExitComponent> ent, ref VehicleExitDoAfterEvent args)
    {
        if (args.Cancelled) return;
        if (!TryGetVehicleFromInterior(ent.Owner, out var vehicleUid) || vehicleUid == null) return;
        
        var vehicleXform = Transform(vehicleUid.Value);
        var exitPos = vehicleXform.LocalPosition + vehicleXform.LocalRotation.RotateVec(new Vector2(0, -2)); 
        
        // ВЫХОД: Аналогичный RMC-метод
        var exitMapCoords = new MapCoordinates(exitPos, vehicleXform.MapID);
        _transform.SetMapCoordinates(args.User, exitMapCoords);
        _transform.SetParent(args.User, vehicleXform.ParentUid);
        
        RemCompDeferred<RMCVehicleInteriorOccupantComponent>(args.User);
    }

    public bool TryGetVehicleFromInterior(EntityUid interiorEntity, out EntityUid? vehicle)
    {
        vehicle = null;
        var mapId = _transform.GetMapId(interiorEntity);
        if (mapId == MapId.Nullspace || !_mapManager.MapExists(mapId))
            return false;

        var mapUid = _mapManager.GetMapEntityId(mapId);
        if (TryComp<RMCVehicleInteriorLinkComponent>(mapUid, out var link))
        {
            vehicle = link.Vehicle;
            return true;
        }
        return false;
    }
}
