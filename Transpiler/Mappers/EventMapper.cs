namespace Transpiler;

/// <summary>
/// Maps C# event subscriptions (Events.PlayerJoin += handler)
/// to Fabric API event registration boilerplate.
/// </summary>
public static class EventMapper
{
    public static readonly Dictionary<string, EventMapping> Events = new()
    {
        // ── Player lifecycle ──────────────────────────────────────────────────

        ["PlayerJoin"] = new(
            FabricClass:    "ServerPlayConnectionEvents",
            FabricEvent:    "JOIN",
            FabricImport:   "net.fabricmc.fabric.api.networking.v1.ServerPlayConnectionEvents",
            JavaArgs:       "(handler, sender, server)",
            Preamble:       "ServerPlayerEntity {0} = handler.player;",
            CsParamTypes:   ["McPlayer"]
        ),

        ["PlayerLeave"] = new(
            FabricClass:    "ServerPlayConnectionEvents",
            FabricEvent:    "DISCONNECT",
            FabricImport:   "net.fabricmc.fabric.api.networking.v1.ServerPlayConnectionEvents",
            JavaArgs:       "(handler, server)",
            Preamble:       "ServerPlayerEntity {0} = handler.player;",
            CsParamTypes:   ["McPlayer"]
        ),

        // ── Block events ──────────────────────────────────────────────────────

        ["BlockBreak"] = new(
            FabricClass:    "PlayerBlockBreakEvents",
            FabricEvent:    "AFTER",
            FabricImport:   "net.fabricmc.fabric.api.event.player.PlayerBlockBreakEvents",
            JavaArgs:       "(world, player, pos, state, blockEntity)",
            Preamble:       "ServerPlayerEntity {0} = player; BlockPos {1} = pos; MinecraftServer server = player.getServer();",
            CsParamTypes:   ["McPlayer", "BlockPos"]
        ),

        ["BlockPlace"] = new(
            FabricClass:    "PlayerBlockBreakEvents",
            FabricEvent:    "BEFORE",
            FabricImport:   "net.fabricmc.fabric.api.event.player.PlayerBlockBreakEvents",
            JavaArgs:       "(world, player, pos, state, blockEntity)",
            Preamble:       "ServerPlayerEntity {0} = player; BlockPos {1} = pos; MinecraftServer server = player.getServer();",
            CsParamTypes:   ["McPlayer", "BlockPos"]
        ),

        ["BlockInteract"] = new(
            FabricClass:    "UseBlockCallback",
            FabricEvent:    "EVENT",
            FabricImport:   "net.fabricmc.fabric.api.event.player.UseBlockCallback",
            JavaArgs:       "(player, world, hand, hitResult)",
            Preamble:       "ServerPlayerEntity {0} = (ServerPlayerEntity) player; MinecraftServer server = {0}.getServer();",
            CsParamTypes:   ["McPlayer"]
        ),

        // ── Chat ──────────────────────────────────────────────────────────────
        // rawMessage is the PlayerChatMessage lambda arg; we extract the String
        // content into a variable with the name the C# author used ({1}).
        // server is made available via sender.getServer() for methods that need it.

        ["ChatMessage"] = new(
            FabricClass:    "ServerMessageEvents",
            FabricEvent:    "CHAT_MESSAGE",
            FabricImport:   "net.fabricmc.fabric.api.message.v1.ServerMessageEvents",
            JavaArgs:       "(rawMessage, sender, params)",
            Preamble:       "ServerPlayerEntity {0} = sender; String {1} = rawMessage.getContent().getString(); MinecraftServer server = sender.getServer();",
            CsParamTypes:   ["McPlayer", "string"]
        ),

        ["CommandMessage"] = new(
            FabricClass:    "ServerMessageEvents",
            FabricEvent:    "COMMAND_MESSAGE",
            FabricImport:   "net.fabricmc.fabric.api.message.v1.ServerMessageEvents",
            JavaArgs:       "(rawMessage, sender, params)",
            Preamble:       "ServerPlayerEntity {0} = sender; String {1} = rawMessage.getContent().getString(); MinecraftServer server = sender.getServer();",
            CsParamTypes:   ["McPlayer", "string"]
        ),

        // ── Server lifecycle ──────────────────────────────────────────────────

        ["ServerStart"] = new(
            FabricClass:    "ServerLifecycleEvents",
            FabricEvent:    "SERVER_STARTED",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerLifecycleEvents",
            JavaArgs:       "(server)",
            Preamble:       "MinecraftServer {0} = server;",
            CsParamTypes:   ["McServer"]
        ),

        ["ServerStop"] = new(
            FabricClass:    "ServerLifecycleEvents",
            FabricEvent:    "SERVER_STOPPING",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerLifecycleEvents",
            JavaArgs:       "(server)",
            Preamble:       "MinecraftServer {0} = server;",
            CsParamTypes:   ["McServer"]
        ),

        ["ServerTick"] = new(
            FabricClass:    "ServerTickEvents",
            FabricEvent:    "END_SERVER_TICK",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents",
            JavaArgs:       "(server)",
            Preamble:       "",
            CsParamTypes:   ["McServer"]
        ),

        ["WorldTick"] = new(
            FabricClass:    "ServerTickEvents",
            FabricEvent:    "END_WORLD_TICK",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents",
            JavaArgs:       "(world)",
            Preamble:       "ServerWorld {0} = world;",
            CsParamTypes:   ["McWorld"]
        ),

        // ── Entity events ─────────────────────────────────────────────────────

        ["EntitySpawn"] = new(
            FabricClass:    "ServerEntityEvents",
            FabricEvent:    "ENTITY_LOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerEntityEvents",
            JavaArgs:       "(entity, world)",
            Preamble:       "Entity {0} = entity;",
            CsParamTypes:   ["McEntity"]
        ),

        ["EntityDeath"] = new(
            FabricClass:    "ServerLivingEntityEvents",
            FabricEvent:    "AFTER_DEATH",
            FabricImport:   "net.fabricmc.fabric.api.entity.event.v1.ServerLivingEntityEvents",
            JavaArgs:       "(entity, damageSource)",
            Preamble:       "LivingEntity {0} = entity; DamageSource {1} = damageSource;",
            CsParamTypes:   ["McEntity", "DamageSource"]
        ),

        ["PlayerDeath"] = new(
            FabricClass:    "ServerLivingEntityEvents",
            FabricEvent:    "AFTER_DEATH",
            FabricImport:   "net.fabricmc.fabric.api.entity.event.v1.ServerLivingEntityEvents",
            JavaArgs:       "(entity, damageSource)",
            Preamble:       "if (!(entity instanceof ServerPlayerEntity)) return; ServerPlayerEntity {0} = (ServerPlayerEntity) entity;",
            CsParamTypes:   ["McPlayer"]
        ),

        ["PlayerRespawn"] = new(
            FabricClass:    "ServerEntityEvents",
            FabricEvent:    "ENTITY_LOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerEntityEvents",
            JavaArgs:       "(entity, world)",
            Preamble:       "if (!(entity instanceof ServerPlayerEntity)) return; ServerPlayerEntity {0} = (ServerPlayerEntity) entity;",
            CsParamTypes:   ["McPlayer"]
        ),

        // ── Item events ───────────────────────────────────────────────────────

        ["ItemUse"] = new(
            FabricClass:    "UseItemCallback",
            FabricEvent:    "EVENT",
            FabricImport:   "net.fabricmc.fabric.api.event.player.UseItemCallback",
            JavaArgs:       "(player, world, hand)",
            Preamble:       "ServerPlayerEntity {0} = (ServerPlayerEntity) player; ItemStack {1} = player.getStackInHand(hand); MinecraftServer server = {0}.getServer();",
            CsParamTypes:   ["McPlayer", "ItemStack"]
        ),

        ["ItemPickup"] = new(
            FabricClass:    "EntitySleepEvents",
            FabricEvent:    "ALLOW_SLEEP_TIME",
            FabricImport:   "net.fabricmc.fabric.api.entity.event.v1.EntitySleepEvents",
            JavaArgs:       "(entity, pos)",
            Preamble:       "/* ItemPickup event — implement via mixin */",
            CsParamTypes:   ["McPlayer"]
        ),

        // ── Chunk events ──────────────────────────────────────────────────────

        ["ChunkLoad"] = new(
            FabricClass:    "ServerChunkEvents",
            FabricEvent:    "CHUNK_LOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerChunkEvents",
            JavaArgs:       "(world, chunk)",
            Preamble:       "",
            CsParamTypes:   ["McWorld"]
        ),

        ["ChunkUnload"] = new(
            FabricClass:    "ServerChunkEvents",
            FabricEvent:    "CHUNK_UNLOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerChunkEvents",
            JavaArgs:       "(world, chunk)",
            Preamble:       "",
            CsParamTypes:   ["McWorld"]
        ),
    };

    public static EventMapping? Get(string csEventName)
        => Events.TryGetValue(csEventName, out var m) ? m : null;
}

/// <summary>
/// Describes how a C# event maps to a Fabric event registration.
/// </summary>
public record EventMapping(
    string FabricClass,
    string FabricEvent,
    string FabricImport,
    string JavaArgs,
    string Preamble,
    string[]? CsParamTypes = null   // C# types of the lambda parameters (e.g. "McPlayer", "string")
);
