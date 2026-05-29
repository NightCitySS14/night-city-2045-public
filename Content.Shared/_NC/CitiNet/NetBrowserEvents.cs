namespace Content.Shared._NC.CitiNet;

/// <summary>
/// Raised when a CitiNet browser navigates to a new URL or is opened.
/// </summary>
public sealed class NetBrowserUrlChangedEvent : EntityEventArgs
{
    public EntityUid Browser { get; }
    public string NewUrl { get; }

    public NetBrowserUrlChangedEvent(EntityUid browser, string newUrl)
    {
        Browser = browser;
        NewUrl = newUrl;
    }
}
