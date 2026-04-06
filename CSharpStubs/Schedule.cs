namespace CSCraft;

/// <summary>
/// Schedule delayed and repeating tasks.
/// Uses the server tick loop (20 ticks = 1 second) internally.
/// All callbacks run on the main server thread — safe to call any API.
/// </summary>
public static class Schedule
{
    /// <summary>
    /// Run once after the given real-time delay.
    /// Example: Schedule.After(TimeSpan.FromSeconds(5), () => server.Broadcast("5 seconds!"));
    /// </summary>
    public static void After(TimeSpan delay, Action callback) { }

    /// <summary>
    /// Run once after a delay in server ticks (20 ticks = 1 second).
    /// Example: Schedule.After(100, () => player.SendMessage("Done."));
    /// </summary>
    public static void After(int ticks, Action callback) { }

    /// <summary>
    /// Run on a repeating interval. Returns a handle that can cancel the task.
    /// Example: var task = Schedule.Every(TimeSpan.FromSeconds(30), () => server.Broadcast("vote!"));
    /// </summary>
    public static ScheduledTask Every(TimeSpan interval, Action callback) => null!;

    /// <summary>
    /// Run exactly N times, then stop automatically.
    /// Example: Schedule.Every(TimeSpan.FromSeconds(10), times: 3, () => player.SendMessage("Starting..."));
    /// </summary>
    public static ScheduledTask Every(TimeSpan interval, int times, Action callback) => null!;

    /// <summary>
    /// Run on a repeating interval defined in ticks.
    /// </summary>
    public static ScheduledTask Every(int ticks, Action callback) => null!;

    /// <summary>
    /// Run exactly N times (tick-based interval).
    /// </summary>
    public static ScheduledTask Every(int ticks, int times, Action callback) => null!;
}

/// <summary>
/// A handle to a task created by Schedule.Every(). Call Cancel() to stop it.
/// </summary>
public class ScheduledTask
{
    /// <summary>Stop this repeating task from firing again.</summary>
    public void Cancel() { }

    /// <summary>Whether this task has been cancelled or has finished running.</summary>
    public bool IsDone { get; }
}
