namespace CSCraft;

/// <summary>
/// Helpers for granting and revoking advancements.
/// Advancements themselves are defined as JSON in data/modid/advancement/.
/// These helpers trigger advancement criteria and grant/revoke at runtime.
/// Transpiles to AdvancementProgress / Advancement calls.
/// </summary>
public static class McAdvancement
{
    /// <summary>
    /// Grant all criteria of an advancement to a player, completing it.
    /// advancementId example: "mymod:my_advancement"
    /// </summary>
    [JavaMethod("{ var _adv = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); if (_adv != null) { var _prog = {0}.getAdvancementTracker().getProgress(_adv); for (var _crit : _prog.getUnobtainedCriteria()) {0}.getAdvancementTracker().grantCriterion(_adv, _crit); } }")]
    public static void Grant(McPlayer player, string advancementId) { }

    /// <summary>
    /// Revoke (reset) an advancement for a player.
    /// </summary>
    [JavaMethod("{ var _adv2 = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); if (_adv2 != null) { var _prog2 = {0}.getAdvancementTracker().getProgress(_adv2); for (var _crit2 : _prog2.getObtainedCriteria()) {0}.getAdvancementTracker().revokeCriterion(_adv2, _crit2); } }")]
    public static void Revoke(McPlayer player, string advancementId) { }

    /// <summary>
    /// Check if a player has completed a specific advancement.
    /// </summary>
    [JavaMethod("{ var _adv3 = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); _adv3 != null && {0}.getAdvancementTracker().getProgress(_adv3).isDone(); }")]
    public static bool HasCompleted(McPlayer player, string advancementId) => false;

    /// <summary>
    /// Grant a specific criterion of an advancement (for multi-step advancements).
    /// </summary>
    [JavaMethod("{ var _adv4 = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); if (_adv4 != null) {0}.getAdvancementTracker().grantCriterion(_adv4, {2}); }")]
    public static void GrantCriterion(McPlayer player, string advancementId, string criterionName) { }
}
