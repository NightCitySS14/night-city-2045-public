using Content.Server.Database;
using Content.Shared._NC.LobbyRewards;
using Content.Shared.GameTicking;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._NC.LobbyRewards;

public sealed class LobbyRewardsSystem : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestLobbyRewardsMessage>(OnRequestRewards);
        SubscribeNetworkEvent<ToggleLobbyRewardMessage>(OnToggleReward);
        
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnRequestRewards(RequestLobbyRewardsMessage msg, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;
        var userId = session.UserId;

        _taskManager.TaskOnMainThread(async () =>
        {
            var balance = await _db.GetNightCoinsBalanceAsync(userId);
            var records = await _db.GetMetaInventoryAsync(userId);

            var items = new List<LobbyRewardItem>();
            foreach (var record in records)
            {
                items.Add(new LobbyRewardItem(record.Id, record.ItemPrototype, record.Quantity, record.Selected));
            }

            RaiseNetworkEvent(new LobbyRewardsDataMessage(balance, items), session.Channel);
        });
    }

    private void OnToggleReward(ToggleLobbyRewardMessage msg, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        _taskManager.TaskOnMainThread(async () =>
        {
            // Update the selection state in the DB.
            await _db.SetMetaInventoryItemSelectedAsync(msg.ItemId, msg.Selected);
            
            // Re-fetch and send the updated list.
            var balance = await _db.GetNightCoinsBalanceAsync(session.UserId);
            var records = await _db.GetMetaInventoryAsync(session.UserId);

            var items = new List<LobbyRewardItem>();
            foreach (var record in records)
            {
                items.Add(new LobbyRewardItem(record.Id, record.ItemPrototype, record.Quantity, record.Selected));
            }

            RaiseNetworkEvent(new LobbyRewardsDataMessage(balance, items), session.Channel);
        });
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (args.Mob == EntityUid.Invalid || Deleted(args.Mob))
            return;

        var userId = args.Player.UserId;
        var mob = args.Mob;

        // Fetch records asynchronously, then spawn items on the main thread
        _taskManager.TaskOnMainThread(async () =>
        {
            var records = await _db.GetMetaInventoryAsync(userId);
            
            // We are back on the main thread after await. Check if mob is still valid.
            if (Deleted(mob))
                return;

            var spawnCoords = _transform.GetMapCoordinates(mob);
            
            foreach (var record in records)
            {
                if (record.Selected && record.Quantity > 0)
                {
                    // Spawn the item at the player's feet
                    Spawn(record.ItemPrototype, spawnCoords);
                    
                    // Deduct from the database
                    await _db.TryRemoveFromMetaInventoryAsync(userId, record.ItemPrototype, 1);
                }
            }
        });
    }
}
