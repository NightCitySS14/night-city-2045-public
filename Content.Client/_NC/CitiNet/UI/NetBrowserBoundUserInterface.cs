using Content.Client.UserInterface.Fragments;
using Content.Shared._NC.CitiNet;
using Content.Shared._NC.CitiNet.Components;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._NC.CitiNet.UI;

public sealed class NetBrowserBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private NetBrowserWindow? _window;
    private UIFragment? _activeSiteUI;
    private Control? _activeUiFragment;
    private string? _activeUrl;

    public NetBrowserBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindowCenteredLeft<NetBrowserWindow>();
        _window.OnNavigate += (url) => SendMessage(new NetBrowserNavigateMessage(url));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        try 
        {
            UpdateStateInternal(state);
        }
        catch (Exception e)
        {
            Logger.ErrorS("citinet.browser", $"Exception in UpdateState: {e}");
        }
    }

    private void UpdateStateInternal(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not NetBrowserUiState browserState)
        {
            _activeSiteUI?.UpdateState(state);
            return;
        }

        _window?.UpdateState(browserState);

        Logger.DebugS("citinet.browser", $"Client UpdateState: URL='{browserState.CurrentUrl}', ActiveUrl='{_activeUrl}', ActiveUI='{_activeSiteUI?.GetType().Name}'");

        if (_activeUrl == browserState.CurrentUrl && _activeSiteUI != null)
        {
            _activeSiteUI?.UpdateState(state);
            return;
        }

        _activeUrl = browserState.CurrentUrl;

        // Find the site prototype for the current URL
        NetSitePrototype? currentSite = null;
        foreach (var site in _prototypeManager.EnumeratePrototypes<NetSitePrototype>())
        {
            if (site.URL == browserState.CurrentUrl)
            {
                currentSite = site;
                break;
            }
        }

        if (currentSite == null)
        {
            Logger.WarningS("citinet.browser", $"No site prototype found for URL: {browserState.CurrentUrl}");
            DetachSiteUI();
            return;
        }

        Logger.DebugS("citinet.browser", $"Found site '{currentSite.ID}' with UIKey '{currentSite.UiKey}'");

        var ui = GetUIFragment(currentSite.UiKey);
        if (ui == null)
        {
            Logger.WarningS("citinet.browser", $"No UI fragment found for UIKey: {currentSite.UiKey}");
            DetachSiteUI();
            return;
        }
        
        // Setup before GetUIFragmentRoot to ensure it's initialized
        ui.Setup(this, Owner);
        var control = ui.GetUIFragmentRoot();

        if (control == null)
        {
            Logger.ErrorS("citinet.browser", $"UI fragment '{ui.GetType().Name}' returned null root control!");
            DetachSiteUI();
            return;
        }

        if (_activeUiFragment?.GetType() == control.GetType())
        {
            _activeSiteUI?.UpdateState(state);
            return;
        }

        Logger.DebugS("citinet.browser", $"Switching UI to {control.GetType().Name}");
        DetachSiteUI();
        AttachSiteUI(ui, control);
    }

    private void AttachSiteUI(UIFragment ui, Control control)
    {
        _activeSiteUI = ui;
        _activeUiFragment = control;
        _window?.Viewport.AddChild(control);
    }

    private void DetachSiteUI()
    {
        if (_activeUiFragment != null)
        {
            if (_window is { Disposed: false })
                _window.Viewport.RemoveChild(_activeUiFragment);
            
            _activeUiFragment = null;
        }
        _activeSiteUI = null;
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);
        // This is where we would handle messages FROM the server TO the UI fragment if needed,
        // but normally UpdateState handles that.
    }

    private UIFragment? GetUIFragment(string uiKey)
    {
        return uiKey switch
        {
            "NetHome" => new NetHomeSiteUIFragment(),
            "CitiNetComm" => new CitiNetUi(),
            "NcpdForensics" => new Forensics.NcpdForensicsUIFragment(),
            "FixerMarket" => new FixerMarket.FixerMarketUIFragment(),
            _ => null
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            DetachSiteUI();
            _window?.Dispose();
            _window = null;
        }
    }
}
