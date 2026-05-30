using Content.Shared._NC.CitiNet.Delivery;
using Content.Shared.Lock;
using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._NC.CitiNet.Delivery;

public sealed class OTPKeypadSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly LockSystem _lockSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        
        Subs.BuiEvents<OTPKeypadComponent>(OTPKeypadUiKey.Key, subs => {
            subs.Event<OTPKeypadSubmitPinMessage>(OnSubmitPin);
        });
    }

    private void OnSubmitPin(EntityUid uid, OTPKeypadComponent component, OTPKeypadSubmitPinMessage msg)
    {
        if (msg.Pin == component.CurrentPin)
        {
            component.IsLocked = false;
            component.CurrentPin = null; // One-time use
            Dirty(uid, component);

            if (TryComp<LockComponent>(uid, out var lockComp))
            {
                _lockSystem.Unlock(uid, msg.Actor, lockComp);
            }

            _popup.PopupEntity("Код верный. Замок открыт.", uid);
            
            if (TryComp<ActorComponent>(msg.Actor, out var actor))
            {
                _chatManager.DispatchServerMessage(actor.PlayerSession, "Доступ разрешен. Заберите ваш товар.");
            }

            _uiSystem.CloseUi(uid, OTPKeypadUiKey.Key);
        }
        else
        {
            _popup.PopupEntity("Неверный код!", uid, msg.Actor, Content.Shared.Popups.PopupType.MediumCaution);
        }
    }
}




