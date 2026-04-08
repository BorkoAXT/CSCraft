namespace CSCraft.Detranspiler.Maps;

/// <summary>
/// Maps Fabric event registration patterns back to CSCraft Events.X += ... syntax.
/// </summary>
public static class ReverseEventMapper
{
    public record EventMapping(
        string FabricPattern,   // what to look for at the start of a registration call
        string CsEvent,         // CSCraft event name
        string[] CsParams,      // C# parameter names to use in the lambda
        string[] CsParamTypes,  // C# types for each parameter
        string[] PreambleVars   // Java variable names declared in the preamble (to strip)
    );

    public static readonly EventMapping[] Mappings =
    [
        new(
            FabricPattern:  "ServerLifecycleEvents.SERVER_STARTED",
            CsEvent:        "ServerStart",
            CsParams:       ["server"],
            CsParamTypes:   ["McServer"],
            PreambleVars:   []
        ),
        new(
            FabricPattern:  "ServerLifecycleEvents.SERVER_STOPPING",
            CsEvent:        "ServerStop",
            CsParams:       ["server"],
            CsParamTypes:   ["McServer"],
            PreambleVars:   []
        ),
        new(
            FabricPattern:  "ServerTickEvents.END_SERVER_TICK",
            CsEvent:        "ServerTick",
            CsParams:       ["server"],
            CsParamTypes:   ["McServer"],
            PreambleVars:   []
        ),
        new(
            FabricPattern:  "ServerPlayConnectionEvents.JOIN",
            CsEvent:        "PlayerJoin",
            CsParams:       ["player"],
            CsParamTypes:   ["McPlayer"],
            PreambleVars:   ["handler", "sender", "server", "srv", "player"]
        ),
        new(
            FabricPattern:  "ServerPlayConnectionEvents.DISCONNECT",
            CsEvent:        "PlayerLeave",
            CsParams:       ["player"],
            CsParamTypes:   ["McPlayer"],
            PreambleVars:   ["handler", "server"]
        ),
        new(
            FabricPattern:  "ServerMessageEvents.CHAT_MESSAGE",
            CsEvent:        "ChatMessage",
            CsParams:       ["player", "message"],
            CsParamTypes:   ["McPlayer", "string"],
            PreambleVars:   ["rawMessage", "sender", "params", "server"]
        ),
        new(
            FabricPattern:  "PlayerBlockBreakEvents.AFTER",
            CsEvent:        "BlockBreak",
            CsParams:       ["player", "world", "pos"],
            CsParamTypes:   ["McPlayer", "McWorld", "McBlockPos"],
            PreambleVars:   ["world", "player", "pos", "state", "blockEntity", "server"]
        ),
        new(
            FabricPattern:  "UseBlockCallback.EVENT",
            CsEvent:        "BlockInteract",
            CsParams:       ["player", "world", "pos"],
            CsParamTypes:   ["McPlayer", "McWorld", "McBlockPos"],
            PreambleVars:   ["player", "world", "hand", "hitResult", "server", "pos"]
        ),
        new(
            FabricPattern:  "AttackEntityCallback.EVENT",
            CsEvent:        "PlayerAttack",
            CsParams:       ["player", "target"],
            CsParamTypes:   ["McPlayer", "McEntity"],
            PreambleVars:   ["player", "world", "hand", "entity", "hitResult", "server", "target"]
        ),
        new(
            FabricPattern:  "ServerLivingEntityEvents.AFTER_DEATH",
            CsEvent:        "EntityDeath",
            CsParams:       ["entity"],
            CsParamTypes:   ["McEntity"],
            PreambleVars:   ["entity", "damageSource", "server"]
        ),
        new(
            FabricPattern:  "ServerEntityEvents.ENTITY_LOAD",
            CsEvent:        "EntitySpawn",
            CsParams:       ["entity", "world"],
            CsParamTypes:   ["McEntity", "McWorld"],
            PreambleVars:   ["entity", "world", "server"]
        ),
        new(
            FabricPattern:  "ServerEntityEvents.ENTITY_UNLOAD",
            CsEvent:        "EntityDespawn",
            CsParams:       ["entity", "world"],
            CsParamTypes:   ["McEntity", "McWorld"],
            PreambleVars:   ["entity", "world", "server"]
        ),
        new(
            FabricPattern:  "ServerPlayerEvents.AFTER_RESPAWN",
            CsEvent:        "PlayerRespawn",
            CsParams:       ["player"],
            CsParamTypes:   ["McPlayer"],
            PreambleVars:   ["oldPlayer", "newPlayer", "alive", "server", "player"]
        ),
        new(
            FabricPattern:  "ServerWorldEvents.LOAD",
            CsEvent:        "WorldLoad",
            CsParams:       ["server", "world"],
            CsParamTypes:   ["McServer", "McWorld"],
            PreambleVars:   []
        ),
        new(
            FabricPattern:  "ServerWorldEvents.UNLOAD",
            CsEvent:        "WorldUnload",
            CsParams:       ["server", "world"],
            CsParamTypes:   ["McServer", "McWorld"],
            PreambleVars:   []
        ),
        new(
            FabricPattern:  "ServerChunkEvents.CHUNK_LOAD",
            CsEvent:        "ChunkLoad",
            CsParams:       ["world", "chunk"],
            CsParamTypes:   ["McWorld", "object"],
            PreambleVars:   []
        ),
    ];

    public static EventMapping? Match(string javaLine)
    {
        foreach (var m in Mappings)
            if (javaLine.Contains(m.FabricPattern))
                return m;
        return null;
    }
}
