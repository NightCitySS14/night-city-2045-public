using Content.Server.Administration;
using Content.Shared._NC.Director;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._NC.Director;

[AdminCommand(AdminFlags.Admin)]
public sealed class StartDirectorEventCommand : IConsoleCommand
{
    public string Command => "startdirectorevent";
    public string Description => "Starts a director event by ID.";
    public string Help => "Usage: startdirectorevent <prototypeId>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GlobalDirectorSystem>();
        var protoManager = IoCManager.Resolve<IPrototypeManager>();

        if (!protoManager.TryIndex<DirectorEventPrototype>(args[0], out var proto))
        {
            shell.WriteError($"Unknown director event prototype: {args[0]}");
            return;
        }

        system.StartEvent(proto);
        shell.WriteLine($"Started event {args[0]}");
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class AdvanceDirectorEventCommand : IConsoleCommand
{
    public string Command => "advancedirectorevent";
    public string Description => "Advances a director event to the next phase.";
    public string Help => "Usage: advancedirectorevent <entityUid>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (!EntityUid.TryParse(args[0], out var uid))
        {
             shell.WriteError("Invalid entity UID.");
             return;
        }

        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GlobalDirectorSystem>();
        system.AdvancePhase(uid);
        shell.WriteLine($"Advanced event {uid}");
    }
}
