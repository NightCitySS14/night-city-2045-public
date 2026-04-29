using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._NC.LobbyRewards;

[AnyCommand]
public sealed class LobbyRewardsCommand : IConsoleCommand
{
    public string Command => "lobbyrewards";
    public string Description => "Opens the lobby rewards menu.";
    public string Help => "Usage: lobbyrewards";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
            return;

        var system = IoCManager.Resolve<IEntityManager>().System<LobbyRewardsSystem>();
        system.OpenEui(player);
    }
}
