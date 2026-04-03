namespace CSCraft;

/// <summary>
/// Helpers for spawning projectiles in the world.
/// Transpiles to Java's ProjectileEntity / ThrownItemEntity patterns.
/// </summary>
public static class McProjectile
{
    /// <summary>Spawn a snowball from a player (flies in the direction the player is looking).</summary>
    [JavaMethod("{ SnowballEntity _proj = new SnowballEntity({0}.getServerWorld(), {0}); _proj.setVelocity({0}, {0}.getPitch(), {0}.getYaw(), 0, 1.5f, 1.0f); {0}.getServerWorld().spawnEntity(_proj); }")]
    public static void ThrowSnowball(McPlayer player) { }

    /// <summary>Spawn an arrow from a player.</summary>
    [JavaMethod("{ ArrowEntity _arr = new ArrowEntity({0}.getServerWorld(), {0}, {0}.getMainHandStack(), null); _arr.setVelocity({0}, {0}.getPitch(), {0}.getYaw(), 0, 3.0f, 1.0f); {0}.getServerWorld().spawnEntity(_arr); }")]
    public static void ShootArrow(McPlayer player) { }

    /// <summary>Spawn a fireball at a world position.</summary>
    [JavaMethod("{ SmallFireballEntity _fb = new SmallFireballEntity({0}.getServer().getWorld(net.minecraft.world.World.OVERWORLD), null, {1}, {2}, {3}); {0}.getServer().getWorld(net.minecraft.world.World.OVERWORLD).spawnEntity(_fb); }")]
    public static void SpawnFireball(McWorld world, double x, double y, double z) { }

    /// <summary>Spawn a thrown potion at a world position.</summary>
    [JavaMethod("{ PotionEntity _pot = new PotionEntity({0}.getServerWorld(), {0}); _pot.setItem(PotionUtil.setPotion(new net.minecraft.item.ItemStack(net.minecraft.item.Items.SPLASH_POTION), Registries.POTION.get(new net.minecraft.util.Identifier({1})))); _pot.setVelocity({0}, {0}.getPitch(), {0}.getYaw(), -20.0f, 0.5f, 0.5f); {0}.getServerWorld().spawnEntity(_pot); }")]
    public static void ThrowPotion(McPlayer player, string potionId) { }
}
