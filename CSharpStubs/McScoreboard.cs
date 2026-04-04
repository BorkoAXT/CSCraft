namespace CSCraft;

/// <summary>
/// Full scoreboard manipulation: objectives, teams, and sidebar display.
/// Wraps Minecraft's Scoreboard class.
/// </summary>
public static class McScoreboard
{
    // ── Objectives ────────────────────────────────────────────────────────────

    /// <summary>Create a new dummy objective, or get it if it already exists.</summary>
    [JavaMethod("{ var _sb = {0}.getScoreboard(); _sb.getNullableObjective({1}) != null ? _sb.getNullableObjective({1}) : _sb.addObjective({1}, ScoreboardCriterion.DUMMY, Text.literal({2}), ScoreboardCriterion.RenderType.INTEGER); }")]
    public static void CreateObjective(McServer server, string name, string displayName) { }

    [JavaMethod("{0}.getScoreboard().removeObjective({0}.getScoreboard().getNullableObjective({1}))")]
    public static void RemoveObjective(McServer server, string name) { }

    /// <summary>Show objective on the sidebar.</summary>
    [JavaMethod("{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.SIDEBAR, {0}.getScoreboard().getNullableObjective({1}))")]
    public static void ShowSidebar(McServer server, string objectiveName) { }

    [JavaMethod("{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.SIDEBAR, null)")]
    public static void HideSidebar(McServer server) { }

    /// <summary>Show objective in the tab list.</summary>
    [JavaMethod("{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.LIST, {0}.getScoreboard().getNullableObjective({1}))")]
    public static void ShowTabList(McServer server, string objectiveName) { }

    /// <summary>Show objective below player names.</summary>
    [JavaMethod("{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.BELOW_NAME, {0}.getScoreboard().getNullableObjective({1}))")]
    public static void ShowBelowName(McServer server, string objectiveName) { }

    // ── Scores ────────────────────────────────────────────────────────────────

    [JavaMethod("{0}.getScoreboard().getOrCreateScore({1}, {0}.getScoreboard().getNullableObjective({2})).getScore()")]
    public static int GetScore(McServer server, McPlayer player, string objective) => 0;

    [JavaMethod("{0}.getScoreboard().getOrCreateScore({1}, {0}.getScoreboard().getNullableObjective({2})).setScore({3})")]
    public static void SetScore(McServer server, McPlayer player, string objective, int value) { }

    [JavaMethod("{0}.getScoreboard().getOrCreateScore({1}, {0}.getScoreboard().getNullableObjective({2})).incrementScore({3})")]
    public static void AddScore(McServer server, McPlayer player, string objective, int amount) { }

    [JavaMethod("{0}.getScoreboard().resetPlayerScore({1}.getEntityName(), {0}.getScoreboard().getNullableObjective({2}))")]
    public static void ResetScore(McServer server, McPlayer player, string objective) { }

    // ── Teams ─────────────────────────────────────────────────────────────────

    /// <summary>Create a team, or get it if it already exists.</summary>
    [JavaMethod("{0}.getScoreboard().getTeam({1}) != null ? {0}.getScoreboard().getTeam({1}) : {0}.getScoreboard().addTeam({1})")]
    public static void CreateTeam(McServer server, string teamName) { }

    [JavaMethod("if ({0}.getScoreboard().getTeam({1}) != null) {0}.getScoreboard().removeTeam({0}.getScoreboard().getTeam({1}))")]
    public static void RemoveTeam(McServer server, string teamName) { }

    [JavaMethod("{0}.getScoreboard().addPlayerToTeam({1}.getEntityName(), {0}.getScoreboard().getTeam({2}))")]
    public static void AddPlayerToTeam(McServer server, McPlayer player, string teamName) { }

    [JavaMethod("{0}.getScoreboard().removePlayerFromTeam({1}.getEntityName(), {0}.getScoreboard().getPlayerTeam({1}.getEntityName()))")]
    public static void RemovePlayerFromTeam(McServer server, McPlayer player) { }

    [JavaMethod("{0}.getScoreboard().getPlayerTeam({1}.getEntityName()) != null ? {0}.getScoreboard().getPlayerTeam({1}.getEntityName()).getName() : null")]
    public static string? GetPlayerTeam(McServer server, McPlayer player) => null;

    [JavaMethod("{ var _t = {0}.getScoreboard().getTeam({1}); if (_t != null) _t.setPrefix(Text.literal({2})); }")]
    public static void SetTeamPrefix(McServer server, string teamName, string prefix) { }

    [JavaMethod("{ var _t2 = {0}.getScoreboard().getTeam({1}); if (_t2 != null) _t2.setSuffix(Text.literal({2})); }")]
    public static void SetTeamSuffix(McServer server, string teamName, string suffix) { }

    /// <summary>Set team color. colorName: e.g. "red", "blue", "gold", "white", "reset"</summary>
    [JavaMethod("{ var _t3 = {0}.getScoreboard().getTeam({1}); if (_t3 != null) _t3.setColor(net.minecraft.util.Formatting.byName({2})); }")]
    public static void SetTeamColor(McServer server, string teamName, string colorName) { }

    [JavaMethod("{ var _t4 = {0}.getScoreboard().getTeam({1}); if (_t4 != null) _t4.setFriendlyFireAllowed({2}); }")]
    public static void SetFriendlyFire(McServer server, string teamName, bool allowed) { }
}
