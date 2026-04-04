namespace CSCraft;

/// <summary>
/// Create and manage boss bars shown to players.
/// Transpiles to Fabric's ServerBossBar.
/// </summary>
[JavaClass("net.minecraft.server.ServerBossBar")]
public class McBossBar
{
    public enum BarColor
    {
        Pink, Blue, Red, Green, Yellow, Purple, White
    }

    public enum BarStyle
    {
        Progress,
        Notched6,
        Notched10,
        Notched12,
        Notched20
    }

    /// <summary>Create a new boss bar with the given title.</summary>
    public McBossBar(string title, BarColor color = BarColor.Purple) { }

    [JavaMethod("{target}.getName().getString()")]
    public string Title { get; } = null!;

    [JavaMethod("{target}.setName(Text.literal({0}))")]
    public void SetTitle(string title) { }

    /// <summary>Progress from 0.0 to 1.0.</summary>
    [JavaMethod("{target}.getPercent()")]
    public float Progress { get; }

    [JavaMethod("{target}.setPercent({0})")]
    public void SetProgress(float progress) { }

    [JavaMethod("{target}.getColor().name().toLowerCase()")]
    public string Color { get; } = null!;

    [JavaMethod("{target}.setColor(BossBar.Color.valueOf({0}.toUpperCase()))")]
    public void SetColor(string color) { }

    [JavaMethod("{target}.getStyle().name()")]
    public string Style { get; } = null!;

    [JavaMethod("{target}.setStyle(BossBar.Style.valueOf({0}))")]
    public void SetStyle(string style) { }

    [JavaMethod("{target}.isVisible()")]
    public bool IsVisible { get; }

    [JavaMethod("{target}.setVisible({0})")]
    public void SetVisible(bool visible) { }

    /// <summary>Show this boss bar to a player.</summary>
    [JavaMethod("{target}.addPlayer({0})")]
    public void AddPlayer(McPlayer player) { }

    /// <summary>Hide this boss bar from a player.</summary>
    [JavaMethod("{target}.removePlayer({0})")]
    public void RemovePlayer(McPlayer player) { }

    /// <summary>Show this boss bar to all online players.</summary>
    [JavaMethod("{0}.getPlayerManager().getPlayerList().forEach({target}::addPlayer)")]
    public void AddAllPlayers(McServer server) { }

    /// <summary>Hide this boss bar from all players.</summary>
    [JavaMethod("{target}.clearPlayers()")]
    public void RemoveAllPlayers() { }

    [JavaMethod("{target}.getPlayers()")]
    public List<McPlayer> GetPlayers() => null!;

    [JavaMethod("{target}.setDarkenSky({0})")]
    public void SetDarkenSky(bool darken) { }

    [JavaMethod("{target}.setDragonMusic({0})")]
    public void SetDragonMusic(bool dragonMusic) { }

    [JavaMethod("{target}.setThickenFog({0})")]
    public void SetThickenFog(bool thickenFog) { }
}
