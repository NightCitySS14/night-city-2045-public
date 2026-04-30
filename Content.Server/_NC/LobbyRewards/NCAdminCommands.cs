using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._NC.LobbyRewards;

[AdminCommand(AdminFlags.Admin)]
public sealed class AddNightCoinsCommand : IConsoleCommand
{
    public string Command => "addnc";
    public string Description => "Adds Night Coins to a player.";
    public string Help => "Usage: addnc <player> <amount>";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError("Invalid arguments. Usage: addnc <player> <amount>");
            return;
        }

        var name = args[0];
        if (!int.TryParse(args[1], out var amount))
        {
            shell.WriteError($"Invalid amount: {args[1]}");
            return;
        }

        var locator = IoCManager.Resolve<IPlayerLocator>();
        var db = IoCManager.Resolve<IServerDbManager>();

        var located = await locator.LookupIdByNameOrIdAsync(name);
        if (located == null)
        {
            shell.WriteError($"Unable to find player: {name}");
            return;
        }

        try 
        {
            await db.AddNightCoinsAsync(located.UserId, amount);
            shell.WriteLine($"Successfully added {amount} Night Coins to {located.Username}.");
        }
        catch (Exception e)
        {
            shell.WriteError($"Database error: {e.Message}");
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player name");
        }
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class SetNightCoinsCommand : IConsoleCommand
{
    public string Command => "setnc";
    public string Description => "Sets Night Coins balance for a player.";
    public string Help => "Usage: setnc <player> <amount>";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError("Invalid arguments. Usage: setnc <player> <amount>");
            return;
        }

        var name = args[0];
        if (!int.TryParse(args[1], out var amount))
        {
            shell.WriteError($"Invalid amount: {args[1]}");
            return;
        }

        var locator = IoCManager.Resolve<IPlayerLocator>();
        var db = IoCManager.Resolve<IServerDbManager>();

        var located = await locator.LookupIdByNameOrIdAsync(name);
        if (located == null)
        {
            shell.WriteError($"Unable to find player: {name}");
            return;
        }

        try
        {
            var current = await db.GetNightCoinsBalanceAsync(located.UserId);
            await db.AddNightCoinsAsync(located.UserId, amount - current);
            shell.WriteLine($"Set Night Coins balance for {located.Username} to {amount}.");
        }
        catch (Exception e)
        {
            shell.WriteError($"Database error: {e.Message}");
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player name");
        }
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class ViewNightCoinsCommand : IConsoleCommand
{
    public string Command => "viewnc";
    public string Description => "Views Night Coins balance and meta-inventory for a player.";
    public string Help => "Usage: viewnc <player>";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Invalid arguments. Usage: viewnc <player>");
            return;
        }

        var name = args[0];
        var locator = IoCManager.Resolve<IPlayerLocator>();
        var db = IoCManager.Resolve<IServerDbManager>();

        var located = await locator.LookupIdByNameOrIdAsync(name);
        if (located == null)
        {
            shell.WriteError($"Unable to find player: {name}");
            return;
        }

        try
        {
            var balance = await db.GetNightCoinsBalanceAsync(located.UserId);
            var inventory = await db.GetMetaInventoryAsync(located.UserId);

            shell.WriteLine($"Player: {located.Username} ({located.UserId})");
            shell.WriteLine($"Balance: {balance} Night Coins");
            shell.WriteLine("Meta-Inventory:");
            if (inventory.Count == 0)
            {
                shell.WriteLine("  (empty)");
            }
            else
            {
                foreach (var item in inventory)
                {
                    shell.WriteLine($"  - {item.ItemPrototype} (x{item.Quantity}) [Selected: {item.Selected}] (ID: {item.Id})");
                }
            }
        }
        catch (Exception e)
        {
            shell.WriteError($"Database error: {e.Message}");
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player name");
        }
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class AddMetaItemCommand : IConsoleCommand
{
    public string Command => "addmetaitem";
    public string Description => "Adds an item to a player's meta-inventory.";
    public string Help => "Usage: addmetaitem <player> <prototypeId> [quantity]";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2 || args.Length > 3)
        {
            shell.WriteError("Invalid arguments. Usage: addmetaitem <player> <prototypeId> [quantity]");
            return;
        }

        var name = args[0];
        var protoId = args[1];
        var quantity = 1;

        if (args.Length == 3 && !int.TryParse(args[2], out quantity))
        {
            shell.WriteError($"Invalid quantity: {args[2]}");
            return;
        }

        var locator = IoCManager.Resolve<IPlayerLocator>();
        var db = IoCManager.Resolve<IServerDbManager>();

        var located = await locator.LookupIdByNameOrIdAsync(name);
        if (located == null)
        {
            shell.WriteError($"Unable to find player: {name}");
            return;
        }

        try
        {
            await db.AddToMetaInventoryAsync(located.UserId, protoId, quantity);
            shell.WriteLine($"Added {quantity}x {protoId} to {located.Username}'s meta-inventory.");
        }
        catch (Exception e)
        {
            shell.WriteError($"Database error: {e.Message}");
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player name");
        }
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class RemoveMetaItemCommand : IConsoleCommand
{
    public string Command => "removemetaitem";
    public string Description => "Removes an item from a player's meta-inventory.";
    public string Help => "Usage: removemetaitem <player> <prototypeId> [quantity]";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2 || args.Length > 3)
        {
            shell.WriteError("Invalid arguments. Usage: removemetaitem <player> <prototypeId> [quantity]");
            return;
        }

        var name = args[0];
        var protoId = args[1];
        var quantity = 1;

        if (args.Length == 3 && !int.TryParse(args[2], out quantity))
        {
            shell.WriteError($"Invalid quantity: {args[2]}");
            return;
        }

        var locator = IoCManager.Resolve<IPlayerLocator>();
        var db = IoCManager.Resolve<IServerDbManager>();

        var located = await locator.LookupIdByNameOrIdAsync(name);
        if (located == null)
        {
            shell.WriteError($"Unable to find player: {name}");
            return;
        }

        try
        {
            var success = await db.TryRemoveFromMetaInventoryAsync(located.UserId, protoId, quantity);
            if (success)
                shell.WriteLine($"Removed {quantity}x {protoId} from {located.Username}'s meta-inventory.");
            else
                shell.WriteError($"Failed to remove {quantity}x {protoId} from {located.Username}'s meta-inventory (insufficient quantity or item not found).");
        }
        catch (Exception e)
        {
            shell.WriteError($"Database error: {e.Message}");
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player name");
        }
        return CompletionResult.Empty;
    }
}
