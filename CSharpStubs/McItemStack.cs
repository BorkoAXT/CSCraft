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
}
