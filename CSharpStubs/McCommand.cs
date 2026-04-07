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
    /// Register a command with two string arguments.
    /// Example: McCommand.Register("msg", "player", "message", (src, p, m) => ...);
    /// </summary>
    public static void Register(string name, string arg1, string arg2, Action<McCommandSource, string, string> handler) { }

    /// <summary>
    /// Register a subcommand under a parent command.
    /// Example: McCommand.RegisterSub("myplugin", "reload", src => ...) → /myplugin reload
    /// </summary>
    public static void RegisterSub(string parent, string sub, Action<McCommandSource> handler) { }

    /// <summary>
    /// Register a subcommand with a string argument.
    /// Example: McCommand.RegisterSub("myplugin", "ban", "player", (src, p) => ...) → /myplugin ban &lt;player&gt;
    /// </summary>
    public static void RegisterSub(string parent, string sub, string argName, Action<McCommandSource, string> handler) { }

    /// <summary>
    /// Register a subcommand with an integer argument.
    /// Example: McCommand.RegisterSub("myplugin", "setlevel", "level", (src, n) => ...) → /myplugin setlevel &lt;level&gt;
    /// </summary>
    public static void RegisterSub(string parent, string sub, string argName, Action<McCommandSource, int> handler) { }

    /// <summary>
    /// Register a command that requires operator permissions (level 2+).
    /// </summary>
    public static void RegisterOp(string name, Action<McCommandSource> handler) { }

    /// <summary>
    /// Register an op-only command with a string argument.
    /// </summary>
    public static void RegisterOp(string name, string argName, Action<McCommandSource, string> handler) { }

    /// <summary>
    /// Register a command with a string argument (explicit name — preferred for transpiler).
    /// </summary>
    public static void RegisterWithString(string name, string argName, Action<McCommandSource, string> handler) { }

    /// <summary>
    /// Register a command with an integer argument (explicit name — preferred for transpiler).
    /// </summary>
    public static void RegisterWithInt(string name, string argName, Action<McCommandSource, int> handler) { }

    /// <summary>
    /// Register a subcommand with an integer argument (explicit name — preferred for transpiler).
    /// </summary>
    public static void RegisterSubWithInt(string parent, string sub, string argName, Action<McCommandSource, int> handler) { }

    /// <summary>
    /// Register a command with a float argument.
    /// </summary>
    public static void Register(string name, string argName, Action<McCommandSource, float> handler) { }

    /// <summary>
    /// Register a command with a boolean argument.
    /// </summary>
    public static void Register(string name, string argName, Action<McCommandSource, bool> handler) { }

    /// <summary>
    /// Register a command with a player selector argument (resolves to the first matched online player).
    /// Example: McCommand.Register("heal", "target", (src, player) => player.Heal(20));
    /// </summary>
    public static void RegisterWithPlayer(string name, string argName, Action<McCommandSource, McPlayer> handler) { }

    /// <summary>
    /// Register a command with a player selector and a string argument.
    /// </summary>
    public static void RegisterWithPlayer(string name, string playerArgName, string strArgName, Action<McCommandSource, McPlayer, string> handler) { }

    /// <summary>
    /// Register a command with a player selector and an integer argument.
    /// </summary>
    public static void RegisterWithPlayer(string name, string playerArgName, string intArgName, Action<McCommandSource, McPlayer, int> handler) { }

    /// <summary>
    /// Register a command with a BlockPos argument.
    /// </summary>
    public static void RegisterWithPos(string name, string argName, Action<McCommandSource, McBlockPos> handler) { }

    /// <summary>
    /// Register an op-only command with a player selector argument.
    /// </summary>
    public static void RegisterOpWithPlayer(string name, string argName, Action<McCommandSource, McPlayer> handler) { }
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
