namespace CSCraft;

/// <summary>
/// Simplified command registration via Fabric's CommandRegistrationCallback.
/// Transpiles to Brigadier command trees.
/// </summary>
public static class McCommand
{
    /// <summary>
    /// Register a simple command with no arguments.
    /// Example: McCommand.Register("heal", src => src.Player.Heal(20));
    /// </summary>
    public static void Register(string name, Action<McCommandSource> handler) { }

    /// <summary>
    /// Register a command with a single required string argument.
    /// Example: McCommand.Register("say", "message", (src, msg) => src.Server.Broadcast(msg));
    /// </summary>
    public static void Register(string name, string argName, Action<McCommandSource, string> handler) { }

    /// <summary>
    /// Register a command with a single integer argument.
    /// </summary>
    public static void Register(string name, string argName, Action<McCommandSource, int> handler) { }

    /// <summary>
    /// Register a command that requires operator permissions (level 2+).
    /// </summary>
    public static void RegisterOp(string name, Action<McCommandSource> handler) { }
}

/// <summary>
/// Represents the source of a command execution (player or console).
/// </summary>
[JavaClass("net.minecraft.server.command.ServerCommandSource")]
public class McCommandSource
{
    /// <summary>The player who ran the command, or null if run from console.</summary>
    [JavaMethod("{target}.getPlayer()")]
    public McPlayer? Player { get; }

    /// <summary>The server instance.</summary>
    [JavaMethod("{target}.getServer()")]
    public McServer Server { get; } = null!;

    /// <summary>The display name of the command source.</summary>
    [JavaMethod("{target}.getName()")]
    public string Name { get; } = null!;

    /// <summary>Whether the source has operator permissions at the given level.</summary>
    [JavaMethod("{target}.hasPermissionLevel({0})")]
    public bool HasPermission(int level) => false;

    /// <summary>Send a success message back to the command source.</summary>
    [JavaMethod("{target}.sendFeedback(() -> Text.literal({0}), false)")]
    public void SendMessage(string message) { }

    /// <summary>Send an error message back to the command source.</summary>
    [JavaMethod("{target}.sendError(Text.literal({0}))")]
    public void SendError(string message) { }
}
