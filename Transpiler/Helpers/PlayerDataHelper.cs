namespace Transpiler.Helpers;

/// <summary>
/// Generates the ModPlayerData helper Java class for persistent per-player NBT storage.
/// Uses Fabric's PersistentState API backed by a HashMap&lt;UUID, NbtCompound&gt;.
/// </summary>
public static class PlayerDataHelper
{
    public static string Generate(string packageName)
    {
        return $@"package {packageName};

import net.minecraft.nbt.NbtCompound;
import net.minecraft.registry.RegistryWrapper;
import net.minecraft.server.MinecraftServer;
import net.minecraft.server.network.ServerPlayerEntity;
import net.minecraft.world.PersistentState;
import net.minecraft.world.PersistentStateManager;
import net.minecraft.world.World;

import java.util.HashMap;
import java.util.UUID;

public class ModPlayerData extends PersistentState {{

    private final HashMap<UUID, NbtCompound> playerData = new HashMap<>();

    public NbtCompound getPlayerNbt(UUID uuid) {{
        return playerData.computeIfAbsent(uuid, k -> new NbtCompound());
    }}

    @Override
    public NbtCompound writeNbt(NbtCompound nbt, RegistryWrapper.WrapperLookup registries) {{
        NbtCompound players = new NbtCompound();
        playerData.forEach((uuid, data) -> players.put(uuid.toString(), data.copy()));
        nbt.put(""playerData"", players);
        return nbt;
    }}

    public static ModPlayerData fromNbt(NbtCompound nbt, RegistryWrapper.WrapperLookup registries) {{
        ModPlayerData state = new ModPlayerData();
        NbtCompound players = nbt.getCompound(""playerData"");
        for (String key : players.getKeys()) {{
            state.playerData.put(UUID.fromString(key), players.getCompound(key));
        }}
        return state;
    }}

    private static final PersistentState.Type<ModPlayerData> TYPE =
        new PersistentState.Type<>(ModPlayerData::new, ModPlayerData::fromNbt, null);

    public static ModPlayerData get(MinecraftServer server) {{
        PersistentStateManager mgr = server.getWorld(World.OVERWORLD).getPersistentStateManager();
        ModPlayerData state = mgr.getOrCreate(TYPE, ""mod_player_data"");
        state.markDirty();
        return state;
    }}

    public static NbtCompound getPlayerNbt(ServerPlayerEntity player) {{
        return get(player.getServer()).getPlayerNbt(player.getUuid());
    }}
}}
";
    }
}
