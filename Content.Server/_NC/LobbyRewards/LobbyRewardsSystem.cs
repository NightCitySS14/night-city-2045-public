using System.Linq;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Server.Hands.Systems;
using Content.Shared._NC.LobbyRewards;
using Content.Shared.GameTicking;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._NC.LobbyRewards;

public sealed class LobbyRewardsSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
    }

    private async void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        var inventory = await _dbManager.GetMetaInventoryAsync(ev.Player.UserId);
        var selectedItems = inventory.Where(x => x.Selected).ToList();

        foreach (var item in selectedItems)
        {
            if (!_prototypeManager.HasIndex<EntityPrototype>(item.ItemPrototype))
                continue;

            for (var i = 0; i < item.Quantity; i++)
            {
                var entity = Spawn(item.ItemPrototype, Transform(ev.Mob).Coordinates);
                _handsSystem.TryPickupAnyHand(ev.Mob, entity);
            }
        }
    }

    public void OpenEui(ICommonSession session)
    {
        _euiManager.OpenEui(new LobbyRewardsEui(), session);
    }
}
