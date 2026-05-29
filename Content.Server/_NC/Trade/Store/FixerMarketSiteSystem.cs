using Content.Shared._NC.Trade;
using Content.Shared._NC.CitiNet.Components;
using Content.Shared._NC.CitiNet;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._NC.Trade;

public sealed class FixerMarketSiteSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly NcStoreLogicSystem _storeLogic = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly StoreSystemStructuredLoader _loader = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NetBrowserComponent, NetBrowserUrlChangedEvent>(OnUrlChanged);
        
        Subs.BuiEvents<NetBrowserComponent>(NetBrowserUiKey.Key, subs => {
            subs.Event<FixerMarketBuyMessage>(OnBuyRequest);
            subs.Event<FixerMarketRequestRefreshMessage>(OnRefreshRequest);
        });
    }

    private void OnRefreshRequest(EntityUid uid, NetBrowserComponent component, FixerMarketRequestRefreshMessage args)
    {
        if (component.CurrentUrl != "fixer.nc/market")
            return;

        var user = _uiSystem.GetActors(uid, NetBrowserUiKey.Key).FirstOrDefault();
        if (user != default)
            UpdateMarketState(uid, component, user);
    }

    private void OnUrlChanged(EntityUid uid, NetBrowserComponent component, NetBrowserUrlChangedEvent args)
    {
        if (args.NewUrl != "fixer.nc/market")
            return;

        var user = _uiSystem.GetActors(uid, NetBrowserUiKey.Key).FirstOrDefault();
        if (user != default)
            UpdateMarketState(uid, component, user);
    }

    private void OnBuyRequest(EntityUid uid, NetBrowserComponent component, FixerMarketBuyMessage msg)
    {
        if (component.CurrentUrl != "fixer.nc/market")
            return;

        var user = _uiSystem.GetActors(uid, NetBrowserUiKey.Key).FirstOrDefault();
        if (user == default) return;

        var store = EnsureComp<NcStoreComponent>(uid);
        if (store.BuyPresets.Count == 0)
        {
             store.BuyPresets.Add("NightMarket_Buy");
             _loader.EnsureLoaded(uid, store, "CitiNet Fixer Market Navigate");
        }

        if (_storeLogic.TryBuy(msg.ListingId, uid, store, user, msg.Count))
        {
            UpdateMarketState(uid, component, user);
        }
    }

    public void UpdateMarketState(EntityUid uid, NetBrowserComponent component, EntityUid user)
    {
        var store = EnsureComp<NcStoreComponent>(uid);
        if (store.BuyPresets.Count == 0)
        {
            store.BuyPresets.Add("NightMarket_Buy");
            _loader.EnsureLoaded(uid, store, "CitiNet Fixer Market Update");
            store.RebuildListingIndex();
        }

        // Get balance
        var itemsBuffer = new List<EntityUid>();
        var snap = new NcInventorySnapshot();
        _storeLogic.ScanInventory(user, itemsBuffer, snap);
        var balance = snap.StackTypeCounts.TryGetValue("Credit", out var b) ? b : 0;

        var listings = new List<FixerMarketListingData>();
        var categories = new HashSet<string>();

        foreach (var listing in store.Listings)
        {
            if (listing.Mode != StoreMode.Buy) continue;
            
            if (!_prototypeManager.TryIndex<EntityPrototype>(listing.ProductEntity, out var proto))
                continue;

            var category = listing.Categories.FirstOrDefault() ?? "General";
            categories.Add(category);
            listings.Add(new FixerMarketListingData(
                listing.Id,
                proto.Name,
                proto.Description,
                category,
                listing.Cost.Values.FirstOrDefault(),
                null,
                listing.RemainingCount
            ));
        }

        var state = new FixerMarketStateMessage(balance, listings, categories.ToList());
        _uiSystem.SetUiState(uid, NetBrowserUiKey.Key, state);
    }
}
