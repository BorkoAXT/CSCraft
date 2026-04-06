namespace CSCraft;

/// <summary>
/// Fluent command builder API — alternative to McCommand for more complex commands.
/// Supports typed arguments, subcommands, and permission levels.
///
/// Example — simple command:
///   Commands.Register("heal", "Heals you to full", ctx => {
///       ctx.Player?.Heal(ctx.Player.MaxHealth);
///       ctx.Player?.SendMessage("Healed!");
///   });
///
/// Example — command with arguments:
///   Commands.Register("give")
///       .Description("Give an item to yourself")
///       .Arg&lt;string&gt;("item")
///       .Arg&lt;int&gt;("amount", defaultValue: 1)
///       .Permission(PermissionLevel.Op)
///       .Execute(ctx => {
///           ctx.Player?.GiveItem(ctx.Get&lt;string&gt;("item"), ctx.Get&lt;int&gt;("amount"));
///       });
///
/// Example — subcommands:
///   Commands.Register("admin")
///       .Permission(PermissionLevel.Op)
///       .Sub("kick", ctx => { ... })
///       .Sub("ban", ctx => { ... })
///       .Sub("reload", ctx => { Config.Reload&lt;MyConfig&gt;(); });
/// </summary>
public static class Commands
{
    /// <summary>Register a command with an inline handler (no arguments).</summary>
    public static CommandBuilder Register(string name, string description, Action<CommandContext> handler) => null!;

    /// <summary>Start building a command. Call .Execute() or .Sub() to complete it.</summary>
    public static CommandBuilder Register(string name) => null!;
}

/// <summary>
/// Fluent command builder. Chain calls to add arguments, subcommands, and permissions.
/// </summary>
public class CommandBuilder
{
    /// <summary>Set the help description shown in /help.</summary>
    public CommandBuilder Description(string desc) => this;

    /// <summary>
    /// Add a typed argument. Supported types: string, int, float, bool, McPlayer, McBlockPos.
    /// </summary>
    public CommandBuilder Arg<T>(string name) => this;

    /// <summary>Add a typed argument with a default value (makes it optional).</summary>
    public CommandBuilder Arg<T>(string name, T defaultValue) => this;

    /// <summary>Require a minimum permission level to run this command.</summary>
    public CommandBuilder Permission(PermissionLevel level) => this;

    /// <summary>Add a subcommand (no arguments).</summary>
    public CommandBuilder Sub(string name, Action<CommandContext> handler) => this;

    /// <summary>Add a subcommand with a string argument.</summary>
    public CommandBuilder Sub<T>(string name, string argName, Action<CommandContext> handler) => this;

    /// <summary>Set the execution handler. Call this last to register the command.</summary>
    public void Execute(Action<CommandContext> handler) { }
}

/// <summary>
/// The execution context passed to command handlers.
/// </summary>
public class CommandContext
{
    /// <summary>The player who ran the command, or null if run from console.</summary>
    public McPlayer? Player { get; } = null!;

    /// <summary>The server instance.</summary>
    public McServer Server { get; } = null!;

    /// <summary>Display name of the command source.</summary>
    public string Name { get; } = null!;

    /// <summary>
    /// Get a typed argument by name.
    /// Example: ctx.Get&lt;int&gt;("amount")
    /// </summary>
    public T Get<T>(string argName) => default!;

    /// <summary>Send a success message back to the command source.</summary>
    public void SendMessage(string message) { }

    /// <summary>Send an error message back to the command source.</summary>
    public void SendError(string message) { }
}

/// <summary>Permission level required to run a command.</summary>
public enum PermissionLevel
{
    /// <summary>Anyone can run this command (default).</summary>
    Any = 0,
    /// <summary>Requires /op (level 2).</summary>
    Op = 2,
    /// <summary>Requires server-op level 3 (used for /ban, /kick, etc.).</summary>
    ServerOp = 3,
    /// <summary>Requires level 4 (only the server console has this by default).</summary>
    ConsoleOp = 4,
}
