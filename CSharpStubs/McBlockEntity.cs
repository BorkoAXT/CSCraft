namespace CSCraft;

/// <summary>
/// Facade for a Minecraft block entity (tile entity) such as chests, furnaces, etc.
/// Wraps BlockEntity.
/// </summary>
[JavaClass("net.minecraft.block.entity.BlockEntity")]
public class McBlockEntity
{
    [JavaMethod("{target}.getPos()")]
    public McBlockPos Pos { get; } = null!;

    [JavaMethod("{target}.getWorld()")]
    public McWorld? World { get; }

    /// <summary>The block entity type ID, e.g. "minecraft:chest".</summary>
    [JavaMethod("net.minecraft.registry.Registries.BLOCK_ENTITY_TYPE.getId({target}.getType()).toString()")]
    public string TypeId { get; } = null!;

    [JavaMethod("{target}.isRemoved()")]
    public bool IsRemoved { get; }

    [JavaMethod("{target}.markDirty()")]
    public void MarkDirty() { }

    // ── NBT ───────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.createNbt()")]
    public McNbt GetNbt() => null!;

    [JavaMethod("{target}.readNbt({0})")]
    public void SetNbt(McNbt nbt) { }

    // ── Inventory block entities (chest, barrel, hopper, etc.) ────────────────

    /// <summary>
    /// Get the inventory of this block entity, if it has one.
    /// Returns null for non-inventory block entities.
    /// </summary>
    [JavaMethod("{target} instanceof net.minecraft.inventory.Inventory _inv ? _inv : null")]
    public McInventory? GetInventory() => null;

    // ── Chest specific ────────────────────────────────────────────────────────

    /// <summary>Check if this block entity is a chest type.</summary>
    [JavaMethod("{target} instanceof net.minecraft.block.entity.ChestBlockEntity")]
    public bool IsChest { get; }

    /// <summary>Check if this block entity is a furnace type.</summary>
    [JavaMethod("{target} instanceof net.minecraft.block.entity.AbstractFurnaceBlockEntity")]
    public bool IsFurnace { get; }

    /// <summary>Check if this block entity is a hopper.</summary>
    [JavaMethod("{target} instanceof net.minecraft.block.entity.HopperBlockEntity")]
    public bool IsHopper { get; }

    // ── Furnace specific ──────────────────────────────────────────────────────

    /// <summary>Get the cook time remaining (furnace/blast furnace/smoker).</summary>
    [JavaMethod("{target} instanceof net.minecraft.block.entity.AbstractFurnaceBlockEntity _fbe ? _fbe.getPropertyDelegate().get(0) : 0")]
    public int GetFurnaceCookTime() => 0;

    /// <summary>Check if the furnace is currently smelting.</summary>
    [JavaMethod("{target} instanceof net.minecraft.block.entity.AbstractFurnaceBlockEntity _fbe2 && _fbe2.isBurning()")]
    public bool IsBurning() => false;

    // ── Sign specific ─────────────────────────────────────────────────────────

    /// <summary>Get the text on line (0-3) of a sign.</summary>
    [JavaMethod("{target} instanceof net.minecraft.block.entity.SignBlockEntity _sbe ? _sbe.getFrontText().getMessage({0}, false).getString() : \"\"")]
    public string GetSignLine(int line) => null!;
}
