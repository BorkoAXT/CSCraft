namespace Transpiler;

/// <summary>
/// Maps C# event subscriptions (Events.PlayerJoin += handler)
/// to Fabric API event registration boilerplate.
///
/// When the emitter sees:
///   Events.PlayerJoin += (player) => { ... };
///
/// It uses this mapper to emit:
///   ServerPlayConnectionEvents.JOIN.register((handler, sender, server) -> {
///       ServerPlayerEntity player = handler.player;
///       ...
///   });
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
            // Extracts the player from handler and names it whatever the C# lambda used
            Preamble:       "ServerPlayerEntity {0} = handler.player;"
        ),

        ["PlayerLeave"] = new(
            FabricClass:    "ServerPlayConnectionEvents",
            FabricEvent:    "DISCONNECT",
            FabricImport:   "net.fabricmc.fabric.api.networking.v1.ServerPlayConnectionEvents",
            JavaArgs:       "(handler, server)",
            Preamble:       "ServerPlayerEntity {0} = handler.player;"
        ),

        // ── Block events ──────────────────────────────────────────────────────

        ["BlockBreak"] = new(
            FabricClass:    "PlayerBlockBreakEvents",
            FabricEvent:    "AFTER",
            FabricImport:   "net.fabricmc.fabric.api.event.player.PlayerBlockBreakEvents",
            JavaArgs:       "(world, player, pos, state, blockEntity)",
            // C# lambda gets (player, pos) — we alias those from the Java args
            Preamble:       "ServerPlayerEntity {0} = player; BlockPos {1} = pos;"
        ),

        ["BlockPlace"] = new(
            FabricClass:    "PlayerBlockBreakEvents",   // Fabric doesn't have a block place event in the same API
            FabricEvent:    "BEFORE",
            FabricImport:   "net.fabricmc.fabric.api.event.player.PlayerBlockBreakEvents",
            JavaArgs:       "(world, player, pos, state, blockEntity)",
            Preamble:       "ServerPlayerEntity {0} = player; BlockPos {1} = pos;"
        ),

        ["BlockInteract"] = new(
            FabricClass:    "UseBlockCallback",
            FabricEvent:    "EVENT",
            FabricImport:   "net.fabricmc.fabric.api.event.player.UseBlockCallback",
            JavaArgs:       "(player, world, hand, hitResult)",
            Preamble:       "ServerPlayerEntity {0} = (ServerPlayerEntity) player;"
        ),

        // ── Chat ──────────────────────────────────────────────────────────────

        ["ChatMessage"] = new(
            FabricClass:    "ServerMessageEvents",
            FabricEvent:    "CHAT_MESSAGE",
            FabricImport:   "net.fabricmc.fabric.api.message.v1.ServerMessageEvents",
            JavaArgs:       "(message, sender, params)",
            Preamble:       "ServerPlayerEntity {0} = sender; String {1} = message.getContent().getString();"
        ),

        ["CommandMessage"] = new(
            FabricClass:    "ServerMessageEvents",
            FabricEvent:    "COMMAND_MESSAGE",
            FabricImport:   "net.fabricmc.fabric.api.message.v1.ServerMessageEvents",
            JavaArgs:       "(message, sender, params)",
            Preamble:       "ServerPlayerEntity {0} = sender; String {1} = message.getContent().getString();"
        ),

        // ── Server lifecycle ──────────────────────────────────────────────────

        ["ServerStart"] = new(
            FabricClass:    "ServerLifecycleEvents",
            FabricEvent:    "SERVER_STARTED",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerLifecycleEvents",
            JavaArgs:       "(server)",
            Preamble:       "MinecraftServer {0} = server;"
        ),

        ["ServerStop"] = new(
            FabricClass:    "ServerLifecycleEvents",
            FabricEvent:    "SERVER_STOPPING",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerLifecycleEvents",
            JavaArgs:       "(server)",
            Preamble:       "MinecraftServer {0} = server;"
        ),

        ["ServerTick"] = new(
            FabricClass:    "ServerTickEvents",
            FabricEvent:    "END_SERVER_TICK",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents",
            JavaArgs:       "(server)",
            Preamble:       ""   // no extra variables needed
        ),

        ["WorldTick"] = new(
            FabricClass:    "ServerTickEvents",
            FabricEvent:    "END_WORLD_TICK",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents",
            JavaArgs:       "(world)",
            Preamble:       "ServerWorld {0} = world;"
        ),

        // ── Entity events ─────────────────────────────────────────────────────

        ["EntitySpawn"] = new(
            FabricClass:    "ServerEntityEvents",
            FabricEvent:    "ENTITY_LOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerEntityEvents",
            JavaArgs:       "(entity, world)",
            Preamble:       "Entity {0} = entity;"
        ),

        ["EntityDeath"] = new(
            FabricClass:    "ServerLivingEntityEvents",
            FabricEvent:    "AFTER_DEATH",
            FabricImport:   "net.fabricmc.fabric.api.entity.event.v1.ServerLivingEntityEvents",
            JavaArgs:       "(entity, damageSource)",
            Preamble:       "LivingEntity {0} = entity; DamageSource {1} = damageSource;"
        ),

        ["PlayerDeath"] = new(
            FabricClass:    "ServerLivingEntityEvents",
            FabricEvent:    "AFTER_DEATH",
            FabricImport:   "net.fabricmc.fabric.api.entity.event.v1.ServerLivingEntityEvents",
            JavaArgs:       "(entity, damageSource)",
            Preamble:       "if (!(entity instanceof ServerPlayerEntity)) return; ServerPlayerEntity {0} = (ServerPlayerEntity) entity;"
        ),

        ["PlayerRespawn"] = new(
            FabricClass:    "ServerEntityEvents",
            FabricEvent:    "ENTITY_LOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerEntityEvents",
            JavaArgs:       "(entity, world)",
            Preamble:       "if (!(entity instanceof ServerPlayerEntity)) return; ServerPlayerEntity {0} = (ServerPlayerEntity) entity;"
        ),

        // ── Item events ───────────────────────────────────────────────────────

        ["ItemUse"] = new(
            FabricClass:    "UseItemCallback",
            FabricEvent:    "EVENT",
            FabricImport:   "net.fabricmc.fabric.api.event.player.UseItemCallback",
            JavaArgs:       "(player, world, hand)",
            Preamble:       "ServerPlayerEntity {0} = (ServerPlayerEntity) player; ItemStack {1} = player.getStackInHand(hand);"
        ),

        ["ItemPickup"] = new(
            FabricClass:    "EntitySleepEvents",   // placeholder — no direct Fabric API
            FabricEvent:    "ALLOW_SLEEP_TIME",
            FabricImport:   "net.fabricmc.fabric.api.entity.event.v1.EntitySleepEvents",
            JavaArgs:       "(entity, pos)",
            Preamble:       "/* ItemPickup event — implement via mixin */"
        ),

        // ── Chunk events ──────────────────────────────────────────────────────

        ["ChunkLoad"] = new(
            FabricClass:    "ChunkEvents",
            FabricEvent:    "CHUNK_LOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerChunkEvents",
            JavaArgs:       "(world, chunk)",
            Preamble:       ""
        ),

        ["ChunkUnload"] = new(
            FabricClass:    "ServerChunkEvents",
            FabricEvent:    "CHUNK_UNLOAD",
            FabricImport:   "net.fabricmc.fabric.api.event.lifecycle.v1.ServerChunkEvents",
            JavaArgs:       "(world, chunk)",
            Preamble:       ""
        ),
    };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Looks up an event mapping by C# event name (e.g. "PlayerJoin").
    /// Returns null if the event is not mapped.
    /// </summary>
    public static EventMapping? Get(string csEventName)
        => Events.TryGetValue(csEventName, out var m) ? m : null;

    /// <summary>
    /// Emits the full Java registration statement for an event.
    ///
    /// lambdaParamNames: the C# lambda parameter names the mod author used
    ///   e.g. for Events.PlayerJoin += (player) => {...}, this is ["player"]
    ///
    /// bodyLines: the already-translated body lines of the lambda
    /// </summary>
    public static string EmitRegistration(
        EventMapping mapping,
        string[] lambdaParamNames,
        IEnumerable<string> bodyLines)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"{mapping.FabricClass}.{mapping.FabricEvent}.register({mapping.JavaArgs} -> {{");

        // Emit preamble (variable aliases), substituting lambda param names
        if (!string.IsNullOrWhiteSpace(mapping.Preamble))
        {
            string preamble = mapping.Preamble;
            for (int i = 0; i < lambdaParamNames.Length; i++)
                preamble = preamble.Replace($"{{{i}}}", lambdaParamNames[i]);
            sb.AppendLine($"    {preamble}");
        }

        // Emit the translated body
        foreach (var line in bodyLines)
            sb.AppendLine($"    {line}");

        sb.Append("});");
        return sb.ToString();
    }
}

/// <summary>
/// Describes how a C# event maps to a Fabric event registration.
/// </summary>
public record EventMapping(
    string FabricClass,     // e.g. "ServerPlayConnectionEvents"
    string FabricEvent,     // e.g. "JOIN"
    string FabricImport,    // full import path
    string JavaArgs,        // the lambda arg list in Java e.g. "(handler, sender, server)"
    string Preamble         // Java lines emitted at the top of the lambda body
                            // to expose the right variables with the right names
);