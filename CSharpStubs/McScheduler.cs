namespace CSCraft;

/// <summary>
/// Schedule delayed and repeating tasks using the server tick loop.
/// All callbacks run on the main server thread — safe to call any API.
/// </summary>
public static class McScheduler
{
    /// <summary>
    /// Run an action after a delay in ticks (20 ticks = 1 second).
    /// Example: McScheduler.RunLater(server, 100, s => s.Broadcast("10 seconds passed!"));
    /// </summary>
    [JavaMethod("{ var _delay = {1}; var _action = (java.lang.Runnable)(() -> {2}); {0}.execute(() -> { try { Thread.sleep(_delay * 50L); } catch (Exception _e) {} {0}.execute(_action); }); }")]
    public static void RunLater(McServer server, int delayTicks, Action<McServer> action) { }

    /// <summary>
    /// Run an action every intervalTicks, starting after the first interval.
    /// The action receives the server and a cancel handle — call cancel() to stop.
    /// Example: McScheduler.RunRepeating(server, 20, (s, cancel) => { ... });
    /// </summary>
    [JavaMethod("net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents.END_SERVER_TICK.register(_srv -> { {2}; })")]
    public static void RunRepeating(McServer server, int intervalTicks, Action<McServer, Action> action) { }

    /// <summary>
    /// Run an action asynchronously (off the main thread).
    /// WARNING: Do NOT call Minecraft API from the async callback — use RunLater inside it to return to main thread.
    /// </summary>
    [JavaMethod("java.util.concurrent.CompletableFuture.runAsync(() -> {1})")]
    public static void RunAsync(McServer server, Action action) { }
}
