namespace CSCraft;

/// <summary>
/// Facade for a Minecraft item stack (ItemStack).
/// Transpiles to Java's ItemStack.
/// </summary>
[JavaClass("net.minecraft.item.ItemStack")]
public class McItemStack
{
    /// <summary>new ItemStack("minecraft:diamond", 1)</summary>
    public McItemStack(string itemId, int count = 1) { }

    [JavaMethod("{target}.getCount()")]
    public int Count { get; set; }

    [JavaMethod("{target}.isEmpty()")]
    public bool IsEmpty { get; }

    [JavaMethod("{target}.hasNbt()")]
    public bool HasNbt { get; }

    [JavaMethod("{target}.getCount()")]
    public int GetCount() => 0;

    [JavaMethod("{target}.setCount({0})")]
    public void SetCount(int count) { }

    [JavaMethod("Registries.ITEM.getId({target}.getItem()).toString()")]
    public string GetItem() => null!;

    [JavaMethod("{target}.getNbt()")]
    public object? GetNbt() => null;

    [JavaMethod("{target}.copy()")]
    public McItemStack Copy() => null!;

    /// <summary>Check whether this stack contains the given item ID (e.g. "minecraft:diamond").</summary>
    [JavaMethod("Registries.ITEM.getId({target}.getItem()).toString().equals({0})")]
    public bool IsOf(string itemId) => false;

    [JavaMethod("{target}.getName().getString()")]
    public string GetCustomName() => null!;

    [JavaMethod("{target}.setCustomName(Text.literal({0}))")]
    public void SetCustomName(string name) { }

    /// <summary>Get the durability damage applied to this stack (0 = full durability).</summary>
    [JavaMethod("{target}.getDamage()")]
    public int GetDamage() => 0;

    [JavaMethod("{target}.setDamage({0})")]
    public void SetDamage(int damage) { }

    /// <summary>Add an enchantment. enchId example: "minecraft:sharpness"</summary>
    [JavaMethod("{ var _enchKey = net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.ENCHANTMENT, new Identifier({0})); Registries.ENCHANTMENT.getEntry(_enchKey).ifPresent(e -> {target}.addEnchantment(e, {1})); }")]
    public void AddEnchantment(string enchId, int level) { }

    // ── Custom NBT data on the stack ─────────────────────────────────────────

    [JavaMethod("{target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().getString({0}) : \"\"")]
    public string GetNbtString(string key) => null!;

    [JavaMethod("{ var _nbtS = {target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().copy() : new NbtCompound(); _nbtS.putString({0}, {1}); {target}.set(net.minecraft.component.DataComponentTypes.CUSTOM_DATA, net.minecraft.component.type.NbtComponent.of(_nbtS)); }")]
    public void SetNbtString(string key, string value) { }

    [JavaMethod("{target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().getInt({0}) : 0")]
    public int GetNbtInt(string key) => 0;

    [JavaMethod("{ var _nbtI = {target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().copy() : new NbtCompound(); _nbtI.putInt({0}, {1}); {target}.set(net.minecraft.component.DataComponentTypes.CUSTOM_DATA, net.minecraft.component.type.NbtComponent.of(_nbtI)); }")]
    public void SetNbtInt(string key, int value) { }
}
