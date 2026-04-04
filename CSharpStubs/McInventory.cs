namespace CSCraft;

/// <summary>
/// Generic inventory manipulation (player inventory, ender chest, chests, etc.).
/// Wraps Inventory / SimpleInventory.
/// </summary>
[JavaClass("net.minecraft.inventory.Inventory")]
public class McInventory
{
    [JavaMethod("{target}.size()")]
    public int Size { get; }

    [JavaMethod("{target}.isEmpty()")]
    public bool IsEmpty { get; }

    /// <summary>Get the item stack in slot index (0-based).</summary>
    [JavaMethod("{target}.getStack({0})")]
    public McItemStack GetSlot(int slot) => null!;

    /// <summary>Set the item stack in a slot.</summary>
    [JavaMethod("{target}.setStack({0}, {1})")]
    public void SetSlot(int slot, McItemStack item) { }

    /// <summary>Remove up to count items from a slot, returns what was removed.</summary>
    [JavaMethod("{target}.removeStack({0}, {1})")]
    public McItemStack TakeSlot(int slot, int count) => null!;

    /// <summary>Clear a slot entirely.</summary>
    [JavaMethod("{target}.removeStack({0})")]
    public McItemStack ClearSlot(int slot) => null!;

    [JavaMethod("{target}.clear()")]
    public void Clear() { }

    /// <summary>Mark the inventory as dirty (triggers save/sync).</summary>
    [JavaMethod("{target}.markDirty()")]
    public void MarkDirty() { }

    // ── Search helpers ────────────────────────────────────────────────────────

    /// <summary>Count how many of a given item ID are in the inventory.</summary>
    [JavaMethod("java.util.stream.IntStream.range(0, {target}.size()).mapToObj({target}::getStack).filter(s -> !s.isEmpty() && Registries.ITEM.getId(s.getItem()).toString().equals({0})).mapToInt(net.minecraft.item.ItemStack::getCount).sum()")]
    public int Count(string itemId) => 0;

    /// <summary>Returns true if the inventory contains at least one of the given item.</summary>
    [JavaMethod("java.util.stream.IntStream.range(0, {target}.size()).anyMatch(i -> !{target}.getStack(i).isEmpty() && Registries.ITEM.getId({target}.getStack(i).getItem()).toString().equals({0}))")]
    public bool Contains(string itemId) => false;

    /// <summary>Find the first slot index containing the given item, or -1.</summary>
    [JavaMethod("java.util.stream.IntStream.range(0, {target}.size()).filter(i -> !{target}.getStack(i).isEmpty() && Registries.ITEM.getId({target}.getStack(i).getItem()).toString().equals({0})).findFirst().orElse(-1)")]
    public int FindSlot(string itemId) => -1;

    // ── Player inventory shortcuts ────────────────────────────────────────────

    /// <summary>Get a player's main inventory (36 slots: hotbar 0-8, main 9-35).</summary>
    [JavaMethod("{0}.getInventory()")]
    public static McInventory FromPlayer(McPlayer player) => null!;

    /// <summary>Get a player's ender chest inventory (27 slots).</summary>
    [JavaMethod("{0}.getEnderChestInventory()")]
    public static McInventory EnderChest(McPlayer player) => null!;

    /// <summary>
    /// Add items to a player's inventory (same as GiveItem but with McItemStack).
    /// Returns true if all items were added, false if some were dropped.
    /// </summary>
    [JavaMethod("{0}.getInventory().insertStack({1})")]
    public static bool Give(McPlayer player, McItemStack item) => false;

    /// <summary>Remove up to count of itemId from a player's inventory.</summary>
    [JavaMethod("{ int _rem = {2}; for (int _i = 0; _i < {0}.getInventory().size() && _rem > 0; _i++) { var _s = {0}.getInventory().getStack(_i); if (!_s.isEmpty() && Registries.ITEM.getId(_s.getItem()).toString().equals({1})) { int _take = Math.min(_s.getCount(), _rem); _s.decrement(_take); _rem -= _take; } } }")]
    public static void Take(McPlayer player, string itemId, int count = 1) { }
}
