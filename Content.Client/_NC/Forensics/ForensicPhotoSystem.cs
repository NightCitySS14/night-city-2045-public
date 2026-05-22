using Content.Shared._NC.Forensics;
using Content.Shared.Interaction;
using Robust.Client.GameObjects;

namespace Content.Client._NC.Forensics;

public sealed class ForensicPhotoSystem : EntitySystem
{
    private ForensicPhotoWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ForensicPhotoComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, ForensicPhotoComponent component, ActivateInWorldEvent args)
    {
        if (_window == null)
            _window = new ForensicPhotoWindow();

        _window.UpdateData(component);
        _window.OpenCentered();
        args.Handled = true;
    }
}
