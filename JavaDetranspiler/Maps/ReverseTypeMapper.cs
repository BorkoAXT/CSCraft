namespace CSCraft.Detranspiler.Maps;

public static class ReverseTypeMapper
{
    private static readonly Dictionary<string, string> JavaToCs = new()
    {
        // MC entities / world
        ["ServerPlayerEntity"]  = "McPlayer",
        ["PlayerEntity"]        = "McPlayer",
        ["ServerWorld"]         = "McWorld",
        ["World"]               = "McWorld",
        ["MinecraftServer"]     = "McServer",
        ["ServerCommandSource"] = "McCommandSource",
        // Items / blocks
        ["Block"]               = "McBlock",
        ["Item"]                = "McItem",
        ["ItemStack"]           = "McItemStack",
        ["BlockItem"]           = "McItem",
        ["SwordItem"]           = "McItem",
        ["PickaxeItem"]         = "McItem",
        ["AxeItem"]             = "McItem",
        ["ShovelItem"]          = "McItem",
        ["HoeItem"]             = "McItem",
        ["ArmorItem"]           = "McItem",
        ["BlockState"]          = "McBlockState",
        ["BlockPos"]            = "McBlockPos",
        // Entities / living
        ["Entity"]              = "McEntity",
        ["LivingEntity"]        = "McEntity",
        ["MobEntity"]           = "McEntity",
        // Misc
        ["ServerBossBar"]       = "McBossBar",
        ["NbtCompound"]         = "McNbt",
        ["Inventory"]           = "McInventory",
        ["BlockEntity"]         = "McBlockEntity",
        ["SoundEvent"]          = "McSound",
        // Primitives
        ["String"]              = "string",
        ["boolean"]             = "bool",
        ["int"]                 = "int",
        ["float"]               = "float",
        ["double"]              = "double",
        ["long"]                = "long",
        ["void"]                = "void",
        ["var"]                 = "var",
        ["Object"]              = "object",
        ["List"]                = "List",
        ["UUID"]                = "string",
    };

    public static string Map(string javaType)
    {
        // Strip fully qualified prefix: net.minecraft.server.network.ServerPlayerEntity -> ServerPlayerEntity
        string simple = javaType.Contains('.') ? javaType.Split('.').Last() : javaType;
        // Strip generic param: List<String> -> List
        int angle = simple.IndexOf('<');
        string bare = angle >= 0 ? simple[..angle] : simple;
        return JavaToCs.TryGetValue(bare, out var cs) ? cs : simple;
    }

    /// <summary>Returns true if this Java type is a well-known MC type we should map.</summary>
    public static bool IsMcType(string javaType) => JavaToCs.ContainsKey(javaType.Split('.').Last());
}
