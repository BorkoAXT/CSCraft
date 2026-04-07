package com.yourname.mymod;

import java.util.List;
import java.util.UUID;

import net.minecraft.block.AbstractBlock;
import net.minecraft.block.Block;
import net.minecraft.block.BlockState;
import net.minecraft.block.entity.BlockEntity;
import net.minecraft.command.argument.EntityArgumentType;
import net.minecraft.component.type.FoodComponent;
import net.minecraft.enchantment.Enchantment;
import net.minecraft.entity.boss.BossBar;
import net.minecraft.entity.boss.ServerBossBar;
import net.minecraft.entity.effect.StatusEffect;
import net.minecraft.entity.effect.StatusEffectInstance;
import net.minecraft.entity.Entity;
import net.minecraft.entity.EntityType;
import net.minecraft.entity.EquipmentSlot;
import net.minecraft.entity.LivingEntity;
import net.minecraft.inventory.Inventory;
import net.minecraft.item.ArmorItem;
import net.minecraft.item.ArmorMaterials;
import net.minecraft.item.AxeItem;
import net.minecraft.item.BlockItem;
import net.minecraft.item.HoeItem;
import net.minecraft.item.Item;
import net.minecraft.item.ItemGroups;
import net.minecraft.item.ItemStack;
import net.minecraft.item.PickaxeItem;
import net.minecraft.item.ShovelItem;
import net.minecraft.item.SwordItem;
import net.minecraft.item.ToolMaterials;
import net.minecraft.nbt.NbtCompound;
import net.minecraft.registry.Registries;
import net.minecraft.registry.Registry;
import net.minecraft.registry.RegistryKey;
import net.minecraft.registry.RegistryKeys;
import net.minecraft.scoreboard.ScoreboardCriterion;
import net.minecraft.server.command.CommandManager;
import net.minecraft.server.command.ServerCommandSource;
import net.minecraft.server.MinecraftServer;
import net.minecraft.server.network.ServerPlayerEntity;
import net.minecraft.server.world.ServerWorld;
import net.minecraft.sound.SoundCategory;
import net.minecraft.sound.SoundEvent;
import net.minecraft.text.Text;
import net.minecraft.util.ActionResult;
import net.minecraft.util.Identifier;
import net.minecraft.util.math.BlockPos;
import net.minecraft.util.TypeFilter;
import net.minecraft.world.GameMode;
import net.minecraft.world.GameRules;
import net.minecraft.world.Heightmap;
import net.minecraft.world.World;

import net.fabricmc.api.ModInitializer;
import net.fabricmc.fabric.api.command.v2.CommandRegistrationCallback;
import net.fabricmc.fabric.api.entity.event.v1.EntityPickupItemEvents;
import net.fabricmc.fabric.api.entity.event.v1.ServerLivingEntityEvents;
import net.fabricmc.fabric.api.entity.event.v1.ServerPlayerEvents;
import net.fabricmc.fabric.api.event.lifecycle.v1.ServerChunkEvents;
import net.fabricmc.fabric.api.event.lifecycle.v1.ServerEntityEvents;
import net.fabricmc.fabric.api.event.lifecycle.v1.ServerLifecycleEvents;
import net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents;
import net.fabricmc.fabric.api.event.lifecycle.v1.ServerWorldEvents;
import net.fabricmc.fabric.api.event.player.AttackEntityCallback;
import net.fabricmc.fabric.api.event.player.PlayerBlockBreakEvents;
import net.fabricmc.fabric.api.event.player.UseBlockCallback;
import net.fabricmc.fabric.api.event.player.UseEntityCallback;
import net.fabricmc.fabric.api.itemgroup.v1.ItemGroupEvents;
import net.fabricmc.fabric.api.message.v1.ServerMessageEvents;
import net.fabricmc.fabric.api.networking.v1.ServerPlayConnectionEvents;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.mojang.brigadier.arguments.StringArgumentType;

public class ExampleMod implements ModInitializer {

    public static final Logger LOGGER = LoggerFactory.getLogger("ExampleMod");

    public Block rubyOre = null;
    public Item ruby = null;
    public Item rubySword = null;
    public Item rubyPickaxe = null;
    public Item rubyHelmet = null;
    public Item rubyChest = null;
    public Item cookedMystery = null;
    public ServerBossBar eventBar = null;
    public void onInitialize() {
        registerContent();
        registerRecipes();
        registerCommands();
        registerEvents();
    }

    public void registerContent() {
        rubyOre = Registry.register(Registries.BLOCK, Identifier.of("examplemod:ruby_ore"), new Block(AbstractBlock.Settings.create().strength(3.0f)));
        Block rubyBlock = Registry.register(Registries.BLOCK, Identifier.of("examplemod:ruby_block"), new Block(AbstractBlock.Settings.create().strength(AbstractBlock.Settings.create().strength(5.0f, 10.0f).requiresTool().sounds("stone"))));
        ruby = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby"), new Item(new Item.Settings()));
        Item rubyOreItem = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_ore"), new BlockItem(rubyOre, new Item.Settings()));
        Item rubyBlockItem = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_block"), new BlockItem(rubyBlock, new Item.Settings()));
        cookedMystery = Registry.register(Registries.ITEM, Identifier.of("examplemod:cooked_mystery"), new Item(new Item.Settings().food(new FoodComponent.Builder().nutrition(8).saturationModifier(0.8f).build())));
        Item rubyArmor = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_shard"), new Item(new Item.Settings()));
        rubySword = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_sword"), new SwordItem(ToolMaterials.DIAMOND, new Item.Settings().attributeModifiers(SwordItem.createAttributeModifiers(ToolMaterials.DIAMOND, 4, -2.4f))));
        rubyPickaxe = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_pickaxe"), new PickaxeItem(ToolMaterials.DIAMOND, new Item.Settings().attributeModifiers(PickaxeItem.createAttributeModifiers(ToolMaterials.DIAMOND, 1, -2.8f))));
        Item rubyAxe = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_axe"), new AxeItem(ToolMaterials.DIAMOND, new Item.Settings().attributeModifiers(AxeItem.createAttributeModifiers(ToolMaterials.DIAMOND, 6.0f, -3.1f))));
        Item rubyShovel = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_shovel"), new ShovelItem(ToolMaterials.DIAMOND, new Item.Settings().attributeModifiers(ShovelItem.createAttributeModifiers(ToolMaterials.DIAMOND, 1.5f, -3.0f))));
        Item rubyHoe = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_hoe"), new HoeItem(ToolMaterials.DIAMOND, new Item.Settings().attributeModifiers(HoeItem.createAttributeModifiers(ToolMaterials.DIAMOND, 0, -3.0f))));
        rubyHelmet = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_helmet"), new ArmorItem(ArmorMaterials.DIAMOND, ArmorItem.Type.HELMET, new Item.Settings()));
        rubyChest = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_chestplate"), new ArmorItem(ArmorMaterials.DIAMOND, ArmorItem.Type.CHESTPLATE, new Item.Settings()));
        Item rubyLegs = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_leggings"), new ArmorItem(ArmorMaterials.DIAMOND, ArmorItem.Type.LEGGINGS, new Item.Settings()));
        Item rubyBoots = Registry.register(Registries.ITEM, Identifier.of("examplemod:ruby_boots"), new ArmorItem(ArmorMaterials.DIAMOND, ArmorItem.Type.BOOTS, new Item.Settings()));
        SoundEvent rubyChime = Registry.register(Registries.SOUND_EVENT, Identifier.of("examplemod:ruby_chime"), SoundEvent.of(Identifier.of("examplemod:ruby_chime")));
        McGameRule<boolean> doubleDrops = GameRuleRegistry.register("examplemod:double_drops", GameRules.Category.MISC, GameRuleFactory.createBooleanRule(false));
        McGameRule<int> maxRubies = GameRuleRegistry.register("examplemod:max_rubies", GameRules.Category.MISC, GameRuleFactory.createIntRule(64));
        ItemGroupEvents.modifyEntriesEvent(ItemGroups.NATURAL).register(e -> e.add(rubyOreItem));
        ItemGroupEvents.modifyEntriesEvent(ItemGroups.BUILDING_BLOCKS).register(e -> e.add(rubyBlockItem));
        ItemGroupEvents.modifyEntriesEvent(ItemGroups.INGREDIENTS).register(e -> e.add(ruby));
        ItemGroupEvents.modifyEntriesEvent(ItemGroups.COMBAT).register(e -> e.add(rubySword));
        ItemGroupEvents.modifyEntriesEvent(ItemGroups.TOOLS).register(e -> e.add(rubyPickaxe));
        ItemGroupEvents.modifyEntriesEvent(ItemGroups.FOOD_AND_DRINK).register(e -> e.add(cookedMystery));
        /* TODO: register sell trade for profession "minecraft:toolsmith" level 2 — see VillagerTrades.registerCustomTradeList */;
        /* TODO: register buy trade for profession "minecraft:toolsmith" level 3 — see VillagerTrades.registerCustomTradeList */;
    }

    public void registerRecipes() {
        /* TODO: register shaped recipe "examplemod:ruby_sword" as JSON in data/modid/recipes/ */;
        /* TODO: register shapeless recipe "examplemod:ruby_from_block" as JSON in data/modid/recipes/ */;
        /* TODO: register smelting recipe "examplemod:ruby_from_ore" as JSON in data/modid/recipes/ */;
        /* TODO: register blasting recipe "examplemod:ruby_from_ore_fast" as JSON in data/modid/recipes/ */;
        /* TODO: register smoking recipe "examplemod:cooked_mystery_smoked" as JSON in data/modid/recipes/ */;
        /* TODO: register campfire recipe "examplemod:campfire_mystery" as JSON in data/modid/recipes/ */;
        /* TODO: register stonecutting recipe "examplemod:ruby_slab" as JSON in data/modid/recipes/ */;
    }

    public void registerCommands() {
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("kit").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { src.sendError(Text.literal("Players only!")); return; } src.player.giveItem("examplemod:ruby_sword", 1); src.player.giveItem("examplemod:ruby_pickaxe", 1); src.player.giveItem("examplemod:ruby_helmet", 1); src.player.giveItem("minecraft:bread", 32); src.player.giveEffect("minecraft:speed", 600, 1); src.player.giveEffect("minecraft:haste", 600, 1); src.sendFeedback(() -> Text.literal("Ruby kit granted!"), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("heal").requires(src -> src.hasPermissionLevel(2)).then(CommandManager.argument("target", EntityArgumentType.player()).executes(ctx -> { ServerCommandSource src = ctx.getSource(); ServerPlayerEntity target = EntityArgumentType.getPlayer(ctx, "target"); target.heal(target.getMaxHealth()); target.clearStatusEffects(); target.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of("minecraft:regeneration")).get(), 100, 1)); target.sendMessage(Text.literal("You were healed by an admin.")); src.sendFeedback(() -> Text.literal("Healed " + target.getName().getString() + "!"), false); return 1; }))));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("spawn").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } BlockPos sp = src.player.world.spawnPos; src.player.teleport(sp.getX(), sp.getY(), sp.getZ()); src.player.playSound("minecraft:entity.enderman.teleport"); src.sendFeedback(() -> Text.literal("Teleported to spawn!"), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("ruby").then(CommandManager.literal("give").then(CommandManager.argument("count", StringArgumentType.string()).executes(ctx -> { ServerCommandSource src = ctx.getSource(); String count = StringArgumentType.getString(ctx, "count"); if (src.player == null) { src.sendError(Text.literal("Players only!")); return; } src.player.giveItem("examplemod:ruby", count); src.sendFeedback(() -> Text.literal("Gave " + count + " rubies."), false); return 1; })))));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("locate").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } String biome = src.player.getBiome(); String dim = src.player.getDimension(); src.sendFeedback(() -> Text.literal("Biome: " + biome), false); src.sendFeedback(() -> Text.literal("Dimension: " + dim), false); src.sendFeedback(() -> Text.literal("X=" + src.player.x + " Y=" + src.player.y + " Z=" + src.player.z), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("info").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } ServerPlayerEntity p = src.player; p.sendMessage(Text.literal("=== Player Info ===")); p.sendMessage(Text.literal("Name: " + p.getName().getString())); p.sendMessage(Text.literal("UUID: " + p.getUuidAsString())); p.sendMessage(Text.literal("HP: " + p.getHealth() + "/" + p.getMaxHealth())); p.sendMessage(Text.literal("Food: " + p.getHungerManager().getFoodLevel())); p.sendMessage(Text.literal("XP Level: " + p.experienceLevel)); p.sendMessage(Text.literal("Ping: " + p.getPing() + "ms")); p.sendMessage(Text.literal("Sneaking: " + p.isSneaking())); p.sendMessage(Text.literal("Flying: " + p.getAbilities().flying)); p.sendMessage(Text.literal("Blocking: " + p.isBlocking)); p.sendMessage(Text.literal("Game mode: " + p.interactionManager.getGameMode().getName())); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("visits").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } int visits = (src.player.getCustomData().contains("visits") ? src.player.getCustomData().getInt("visits") : 0); src.sendFeedback(() -> Text.literal("You have visited " + visits + " times."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("ruby").then(CommandManager.literal("resetvisits").then(CommandManager.argument("count", StringArgumentType.string()).executes(ctx -> { ServerCommandSource src = ctx.getSource(); String count = StringArgumentType.getString(ctx, "count"); if (src.player == null) { return; } src.player.getCustomData().putInt("visits", count); src.sendFeedback(() -> Text.literal("Visits reset to " + count + "."), false); return 1; })))));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("settime").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } src.player.world.setTime(ticks); src.sendFeedback(() -> Text.literal("Time set to " + ticks + "."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("fill").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } int x = ((int)src.player.x); int y = ((int)src.player.y) - 1; int z = ((int)src.player.z); src.player.world.fillBlocks(x - 2, y, z - 2, x + 2, y, z + 2, blockId); src.sendFeedback(() -> Text.literal("Filled 5x5 with " + blockId + "!"), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("serverinfo").executes(ctx -> { ServerCommandSource src = ctx.getSource(); MinecraftServer s = src.server; src.sendFeedback(() -> Text.literal("Version: " + s.getVersion()), false); src.sendFeedback(() -> Text.literal("Players: " + s.getPlayerManager().getCurrentPlayerCount() + "/" + s.getPlayerManager().getMaxPlayerCount()), false); src.sendFeedback(() -> Text.literal("TPS: " + s.getTickTime()), false); src.sendFeedback(() -> Text.literal("Hardcore: " + s.isHardcore), false); src.sendFeedback(() -> Text.literal("Seed: " + s.getSeed()), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("inventory").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } Inventory inv = src.player.getInventory(); int rubies = java.util.stream.IntStream.range(0, inv.size()).mapToObj(inv::getStack).filter(s -> !s.isEmpty() && Registries.ITEM.getId(s.getItem()).toString().equals("examplemod:ruby")).mapToInt(net.minecraft.item.ItemStack::getCount).sum(); int diamonds = java.util.stream.IntStream.range(0, inv.size()).mapToObj(inv::getStack).filter(s -> !s.isEmpty() && Registries.ITEM.getId(s.getItem()).toString().equals("minecraft:diamond")).mapToInt(net.minecraft.item.ItemStack::getCount).sum(); boolean hasRuby = java.util.stream.IntStream.range(0, inv.size()).anyMatch(i -> !inv.getStack(i).isEmpty() && Registries.ITEM.getId(inv.getStack(i).getItem()).toString().equals("examplemod:ruby")); src.sendFeedback(() -> Text.literal("Inventory size: " + inv.size()), false); src.sendFeedback(() -> Text.literal("Rubies: " + rubies + "  Diamonds: " + diamonds), false); src.sendFeedback(() -> Text.literal("Has rubies: " + hasRuby), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("weather").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } if (mode.equals("rain")) { src.player.world.setWeather(0, 6000, true, false); } else { src.player.world.setWeather(6000, 0, false, false); } src.sendFeedback(() -> Text.literal("Weather set to " + mode + "."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("effect").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } src.player.giveEffect(effectId, 1200, 0); src.sendFeedback(() -> Text.literal("Applied " + effectId + " for 60 seconds."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("creative").requires(src -> src.hasPermissionLevel(2)).executes(ctx -> { ServerCommandSource src = ctx.getSource(); src.Player?.SetGameMode("creative"); src.sendFeedback(() -> Text.literal("Game mode set to creative."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("survival").requires(src -> src.hasPermissionLevel(2)).executes(ctx -> { ServerCommandSource src = ctx.getSource(); src.Player?.SetGameMode("survival"); src.sendFeedback(() -> Text.literal("Game mode set to survival."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("border").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } src.player.world.animateBorderSize(size, 5.0); src.sendFeedback(() -> Text.literal("World border animating to " + size + "."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("lightning").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } src.player.world.spawnLightning(src.player.x, src.player.y, src.player.z); src.sendFeedback(() -> Text.literal("Lightning!"), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("explode").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } src.player.world.createExplosion(src.player.x, src.player.y, src.player.z, 3.0f); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("xp").executes(ctx -> { ServerCommandSource src = ctx.getSource(); if (src.player == null) { return; } src.player.giveXp(amount); src.sendFeedback(() -> Text.literal("Gave " + amount + " XP."), false); return 1; })));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("home").then(CommandManager.literal("set").then(CommandManager.argument((src) -> { if (src.player == null) { return; } src.player.getCustomData().putInt("home_x", ((int)src.player.x)); src.player.getCustomData().putInt("home_y", ((int)src.player.y)); src.player.getCustomData().putInt("home_z", ((int)src.player.z)); src.sendFeedback(() -> Text.literal("Home set!"), false); }, StringArgumentType.string()).executes(ctx -> { ServerCommandSource src = ctx.getSource(); String value = StringArgumentType.getString(ctx, (src) -> { if (src.player == null) { return; } src.player.getCustomData().putInt("home_x", ((int)src.player.x)); src.player.getCustomData().putInt("home_y", ((int)src.player.y)); src.player.getCustomData().putInt("home_z", ((int)src.player.z)); src.sendFeedback(() -> Text.literal("Home set!"), false); }); if (src.player == null) { return; } src.player.getCustomData().putInt("home_x", ((int)src.player.x)); src.player.getCustomData().putInt("home_y", ((int)src.player.y)); src.player.getCustomData().putInt("home_z", ((int)src.player.z)); src.sendFeedback(() -> Text.literal("Home set!"), false); return 1; })))));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("home").then(CommandManager.literal("go").then(CommandManager.argument((src) -> { if (src.player == null) { return; } if (!src.player.getCustomData().contains("home_x")) { src.sendError(Text.literal("No home set. Use /home set first.")); return; } int hx = (src.player.getCustomData().contains("home_x") ? src.player.getCustomData().getInt("home_x") : 0); int hy = (src.player.getCustomData().contains("home_y") ? src.player.getCustomData().getInt("home_y") : 64); int hz = (src.player.getCustomData().contains("home_z") ? src.player.getCustomData().getInt("home_z") : 0); src.player.teleport(hx, hy, hz); src.player.playSound("minecraft:entity.enderman.teleport"); src.sendFeedback(() -> Text.literal("Teleported home!"), false); }, StringArgumentType.string()).executes(ctx -> { ServerCommandSource src = ctx.getSource(); String value = StringArgumentType.getString(ctx, (src) -> { if (src.player == null) { return; } if (!src.player.getCustomData().contains("home_x")) { src.sendError(Text.literal("No home set. Use /home set first.")); return; } int hx = (src.player.getCustomData().contains("home_x") ? src.player.getCustomData().getInt("home_x") : 0); int hy = (src.player.getCustomData().contains("home_y") ? src.player.getCustomData().getInt("home_y") : 64); int hz = (src.player.getCustomData().contains("home_z") ? src.player.getCustomData().getInt("home_z") : 0); src.player.teleport(hx, hy, hz); src.player.playSound("minecraft:entity.enderman.teleport"); src.sendFeedback(() -> Text.literal("Teleported home!"), false); }); if (src.player == null) { return; } if (!src.player.getCustomData().contains("home_x")) { src.sendError(Text.literal("No home set. Use /home set first.")); return; } int hx = (src.player.getCustomData().contains("home_x") ? src.player.getCustomData().getInt("home_x") : 0); int hy = (src.player.getCustomData().contains("home_y") ? src.player.getCustomData().getInt("home_y") : 64); int hz = (src.player.getCustomData().contains("home_z") ? src.player.getCustomData().getInt("home_z") : 0); src.player.teleport(hx, hy, hz); src.player.playSound("minecraft:entity.enderman.teleport"); src.sendFeedback(() -> Text.literal("Teleported home!"), false); return 1; })))));
        CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal("nbt").then(CommandManager.literal("get").then(CommandManager.argument("key", StringArgumentType.string()).executes(ctx -> { ServerCommandSource src = ctx.getSource(); String key = StringArgumentType.getString(ctx, "key"); if (src.player == null) { return; } String val = src.player.getNbtString(key); src.sendFeedback(() -> Text.literal(key + " = " + val), false); return 1; })))));
    }

    public void registerEvents() {
        eventBar = new ServerBossBar(Text.literal("Server Events"), BossBar.Color.PURPLE, BossBar.Style.PROGRESS);
        eventBar.setVisible(true);
        ServerPlayConnectionEvents.JOIN.register((handler, sender, server) -> {
            ServerPlayerEntity player = handler.player;
            int visits = ModPlayerData.getPlayerNbt(player).getInt("visits");
            visits++;
            { NbtCompound _pNbt = ModPlayerData.getPlayerNbt(player); _pNbt.putInt("visits", visits); };
            player.networkHandler.sendPacket(new net.minecraft.network.packet.s2c.play.TitleS2CPacket(Text.literal("Welcome!"))); player.networkHandler.sendPacket(new net.minecraft.network.packet.s2c.play.SubtitleS2CPacket(Text.literal("Hello, " + player.getName().getString())));
            player.sendMessage(Text.literal("Welcome to the server! Visit #" + visits + "."));
            player.sendMessage(Text.literal("Server has " + player.getServer().playerCount + " player(s) online."), true);
            if (visits == 1) {
                player.getInventory().insertStack(new ItemStack(Registries.ITEM.get(Identifier.of("minecraft:bread")), 16));
                player.getInventory().insertStack(new ItemStack(Registries.ITEM.get(Identifier.of("minecraft:wooden_sword")), 1));
                player.sendMessage(Text.literal("First visit! Here's some bread and a sword."));
            }
            eventBar.addPlayer(player);
            eventBar.setName(Text.literal(player.getName().getString() + " joined!"));
            eventBar.setPercent(1.0f);
            { var _sb = player.getServer().getScoreboard(); if (_sb.getNullableObjective("kills") == null) _sb.addObjective("kills", ScoreboardCriterion.DUMMY, Text.literal("Kills"), ScoreboardCriterion.RenderType.INTEGER, false, null); };
            player.getServer().getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.SIDEBAR, player.getServer().getScoreboard().getNullableObjective("kills"));
            if (!player.getCustomData().contains("kills_init")) {
                player.getServer().getScoreboard().getOrCreateScore(player, player.getServer().getScoreboard().getNullableObjective("kills")).setScore(0);
                player.getCustomData().putInt("kills_init", true);
            } else {
                int savedKills = (player.getCustomData().contains("kills") ? player.getCustomData().getInt("kills") : 0);
                player.getServer().getScoreboard().getOrCreateScore(player, player.getServer().getScoreboard().getNullableObjective("kills")).setScore(savedKills);
            }
            { if (player.getServer().getScoreboard().getTeam("players") == null) player.getServer().getScoreboard().addTeam("players"); };
            { var _t3 = player.getServer().getScoreboard().getTeam("players"); if (_t3 != null) _t3.setColor(net.minecraft.util.Formatting.byName("green")); };
            player.getServer().getScoreboard().addPlayerToTeam(player.getEntityName(), player.getServer().getScoreboard().getTeam("players"));
            Inventory inv = player.getInventory();
            int rubyCount = java.util.stream.IntStream.range(0, inv.size()).mapToObj(inv::getStack).filter(s -> !s.isEmpty() && Registries.ITEM.getId(s.getItem()).toString().equals("examplemod:ruby")).mapToInt(net.minecraft.item.ItemStack::getCount).sum();
            if (rubyCount > 0) {
                player.sendMessage(Text.literal("You have " + rubyCount + " rubies."));
            }
        });
        ServerPlayConnectionEvents.DISCONNECT.register((handler, server) -> {
            ServerPlayerEntity player = handler.player;
            eventBar.removePlayer(player);
            int kills = player.getServer().getScoreboard().getOrCreateScore(player, player.getServer().getScoreboard().getNullableObjective("kills")).getScore();
            player.getCustomData().putInt("kills", kills);
            player.getServer().broadcast(player.getName().getString() + " left. Kills this session: " + kills + ".");
        });
        ServerLivingEntityEvents.AFTER_DEATH.register((entity, damageSource) -> {
            if (!(entity instanceof ServerPlayerEntity)) return;
            ServerPlayerEntity player = (ServerPlayerEntity) entity;
            ((ServerWorld)player.getWorld()).spawnLightning(player.getX(), player.getY(), player.getZ());
            ((ServerWorld)player.getWorld()).spawnParticle(McParticles.hugeExplosion, player.getX(), player.getY() + 1, player.getZ(), 3);
            player.getServer().broadcast(player.getName().getString() + " has fallen!");
            double currentSize = ((ServerWorld)player.getWorld()).getBorderSize();
            if (currentSize > 200) {
                ((ServerWorld)player.getWorld()).animateBorderSize(currentSize - 10, 5.0);
                player.getServer().broadcast("World border shrinking to " + (currentSize - 10) + "!");
            }
        });
        ServerPlayerEvents.AFTER_RESPAWN.register((oldPlayer, newPlayer, alive) -> {
            ServerPlayerEntity player = newPlayer;
            MinecraftServer server = newPlayer.getServer();
            player.sendMessage(Text.literal("You respawned! Stay alive."));
            player.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of("minecraft:resistance")).get(), 100, 1));
        });
        PlayerBlockBreakEvents.AFTER.register((world, player, pos, state, blockEntity) -> {
            MinecraftServer server = player.getServer();
            String block = ((ServerWorld)player.getWorld()).getBlock(pos.getX(), pos.getY(), pos.getZ());
            if (block.equals("examplemod:ruby_ore")) {
                player.getInventory().insertStack(new ItemStack(Registries.ITEM.get(Identifier.of("examplemod:ruby")), 2));
                ((ServerWorld)player.getWorld()).spawnParticle(McParticles.glow, pos.getX(), pos.getY() + 1, pos.getZ(), 20);
                ((ServerWorld)player.getWorld()).spawnParticle(McParticles.crit, pos.getX(), pos.getY() + 1, pos.getZ(), 10);
                ((ServerWorld)player.getWorld()).playSound("examplemod:ruby_chime", pos.getX(), pos.getY(), pos.getZ());
                player.sendMessage(Text.literal("Found rubies!"), true);
                int score = player.getServer().getScoreboard().getOrCreateScore(player, player.getServer().getScoreboard().getNullableObjective("kills")).getScore();
                player.getServer().getScoreboard().getOrCreateScore(player, player.getServer().getScoreboard().getNullableObjective("kills")).setScore(score + 1);
                player.getCustomData().putInt("kills", score + 1);
            }
            if (block.equals("minecraft:diamond_ore") || block.equals("minecraft:deepslate_diamond_ore")) {
                ((ServerWorld)player.getWorld()).spawnParticle(McParticles.enchantTable, pos.getX(), pos.getY() + 1, pos.getZ(), 30);
                player.sendMessage(Text.literal("Diamonds!"), true);
            }
        });
        UseBlockCallback.EVENT.register((player, world, hand, hitResult) -> {
            MinecraftServer server = player.getServer();
            BlockEntity be = ((ServerWorld)player.getWorld()).getBlockEntity(pos.getX(), pos.getY(), pos.getZ());
            if (be != null && be.isChest) {
                Inventory chest = be.getInventory();
                if (chest != null && chest.contains("examplemod:ruby")) {
                    player.sendMessage(Text.literal("This chest contains rubies!"));
                    ((ServerWorld)player.getWorld()).spawnParticle(McParticles.happyVillager, pos.getX(), pos.getY() + 1, pos.getZ(), 10);
                }
            }
        });
        ServerMessageEvents.CHAT_MESSAGE.register((rawMessage, sender, params) -> {
            ServerPlayerEntity player = sender;
            String message = rawMessage.getContent().getString();
            MinecraftServer server = sender.getServer();
            if (message.equals("!effects")) {
                player.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of("minecraft:speed")).get(), 600, 1));
                player.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of("minecraft:night_vision")).get(), 600, 0));
                player.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of("minecraft:resistance")).get(), 600, 1));
                player.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of("minecraft:strength")).get(), 600, 1));
                player.sendMessage(Text.literal("Buff package applied for 30 seconds!"));
            }
            if (message.equals("!clear")) {
                player.clearStatusEffects();
                player.sendMessage(Text.literal("All effects cleared."));
            }
            if (message.equals("!setspawn")) {
                player.setSpawnPoint(World.OVERWORLD, new BlockPos(((int)player.getX()),((int)player.getY()),((int)player.getZ())), 0f, true, false);
                player.sendMessage(Text.literal("Spawn point updated."));
            }
            if (message.equals("!fly")) {
                player.changeGameMode(GameMode.byName("creative"));
                player.sendMessage(Text.literal("Creative mode enabled for flight."));
            }
            if (message.equals("!health")) {
                player.sendMessage(Text.literal("HP: " + player.getHealth() + "/" + player.getMaxHealth()));
                player.sendMessage(Text.literal("Food: " + player.getHungerManager().getFoodLevel()));
            }
            if (message.equals("!world")) {
                ServerWorld world = ((ServerWorld)player.getWorld());
                player.sendMessage(Text.literal("Time: " + world.getTime()));
                player.sendMessage(Text.literal("Day: " + world.isDay() + " | Raining: " + world.isRaining()));
                player.sendMessage(Text.literal("Difficulty: " + world.getDifficulty().getName()));
                player.sendMessage(Text.literal("Dimension: " + world.dimension));
                int topY = world.getTopY(Heightmap.Type.WORLD_SURFACE, ((int)player.getX()), ((int)player.getZ()));
                player.sendMessage(Text.literal("Top Y at position: " + topY));
            }
            if (message.equals("!inv")) {
                Inventory inv = player.getInventory();
                ItemStack main = player.getMainHandStack();
                ItemStack off = player.getOffHandStack();
                player.sendMessage(Text.literal("Main hand: " + Registries.ITEM.getId(main.getItem()).toString()));
                player.sendMessage(Text.literal("Off hand: " + Registries.ITEM.getId(off.getItem()).toString()));
                player.sendMessage(Text.literal("Inventory slots: " + inv.size()));
            }
            if (message.equals("!armor")) {
                player.sendMessage(Text.literal("Helmet: " + player.getEquippedStack(net.minecraft.entity.EquipmentSlot.HEAD).getItem()));
                player.sendMessage(Text.literal("Chest: " + player.getEquippedStack(net.minecraft.entity.EquipmentSlot.CHEST).getItem()));
                player.sendMessage(Text.literal("Legs: " + player.getEquippedStack(net.minecraft.entity.EquipmentSlot.LEGS).getItem()));
                player.sendMessage(Text.literal("Boots: " + player.getEquippedStack(net.minecraft.entity.EquipmentSlot.FEET).getItem()));
            }
            if (message.equals("!border")) {
                ServerWorld world = ((ServerWorld)player.getWorld());
                player.sendMessage(Text.literal("Border size: " + world.getBorderSize()));
                player.sendMessage(Text.literal("Center: " + world.getBorderCenterX() + ", " + world.getBorderCenterZ()));
                player.sendMessage(Text.literal("In border: " + world.getWorldBorder().contains(new BlockPos(((int)player.getX()),((int)player.getY()),((int)player.getZ())))));
            }
            if (message.equals("!particles")) {
                ServerWorld world = ((ServerWorld)player.getWorld());
                world.spawnParticles(Registries.PARTICLE_TYPE.get(Identifier.of(McParticles.heart)), player.getX(), player.getY() + 2, player.getZ(), 15, 0, 0, 0, 0);
                world.spawnParticles(Registries.PARTICLE_TYPE.get(Identifier.of(McParticles.totem)), player.getX(), player.getY() + 1, player.getZ(), 20, 0, 0, 0, 0);
                world.spawnParticles(Registries.PARTICLE_TYPE.get(Identifier.of(McParticles.note)), player.getX(), player.getY() + 3, player.getZ(), 10, 0, 0, 0, 0);
                world.spawnParticles(Registries.PARTICLE_TYPE.get(Identifier.of(McParticles.cherry)), player.getX() + 1, player.getY() + 2, player.getZ(), 30, 0, 0, 0, 0);
                player.sendMessage(Text.literal("Particles!"), true);
            }
            if (message.equals("!sound")) {
                player.playSoundToPlayer(Registries.SOUND_EVENT.get(Identifier.of("minecraft:entity.experience_orb.pickup")), SoundCategory.PLAYERS, 1.0f, 1.0f);
                ((ServerWorld)player.getWorld()).playSound("minecraft:block.bell.use", player.getX(), player.getY(), player.getZ());
            }
            if (message.equals("!enchant")) {
                ItemStack sword = player.getMainHandStack();
                if (!sword.isEmpty()) {
                    { var _enchKey = net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.ENCHANTMENT, Identifier.of("minecraft:sharpness")); Registries.ENCHANTMENT.getEntry(_enchKey).ifPresent(e -> sword.addEnchantment(e, 5)); };
                    { var _enchKey = net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.ENCHANTMENT, Identifier.of("minecraft:unbreaking")); Registries.ENCHANTMENT.getEntry(_enchKey).ifPresent(e -> sword.addEnchantment(e, 3)); };
                    player.sendMessage(Text.literal("Enchanted your held item!"));
                } else {
                    player.sendMessage(Text.literal("Hold an item to enchant."));
                }
            }
            if (message.equals("!nbt")) {
                ItemStack held = player.getMainHandStack();
                { var _nbtS = held.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? held.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().copy() : new NbtCompound(); _nbtS.putString("owner", player.getName().getString()); held.set(net.minecraft.component.DataComponentTypes.CUSTOM_DATA, net.minecraft.component.type.NbtComponent.of(_nbtS)); };
                { var _nbtI = held.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? held.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().copy() : new NbtCompound(); _nbtI.putInt("power", 99); held.set(net.minecraft.component.DataComponentTypes.CUSTOM_DATA, net.minecraft.component.type.NbtComponent.of(_nbtI)); };
                String owner = held.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? held.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().getString("owner") : "";
                int power = held.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? held.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().getInt("power") : 0;
                player.sendMessage(Text.literal("Set owner=" + owner + " power=" + power + " on held item."));
            }
            if (message.equals("!tags")) {
                ServerWorld world = ((ServerWorld)player.getWorld());
                int bx = ((int)player.getX());
                int by = ((int)player.getY()) - 1;
                int bz = ((int)player.getZ());
                boolean isLog = world.getBlockState(new BlockPos(bx,by,bz)).isIn(net.minecraft.registry.tag.BlockTags.LOGS);
                boolean isDirt = world.getBlockState(new BlockPos(bx,by,bz)).isIn(net.minecraft.registry.tag.BlockTags.DIRT);
                boolean isSword = player.getMainHandStack().isIn(net.minecraft.registry.tag.ItemTags.SWORDS);
                player.sendMessage(Text.literal("Block below — log: " + isLog + " dirt: " + isDirt));
                player.sendMessage(Text.literal("Main hand is sword: " + isSword));
            }
            if (message.equals("!fluid")) {
                ServerWorld world = ((ServerWorld)player.getWorld());
                int fx = ((int)player.getX());
                int fy = ((int)player.getY());
                int fz = ((int)player.getZ());
                player.sendMessage(Text.literal("Water: " + world.getFluidState(new BlockPos(fx,fy,fz)).isIn(net.minecraft.registry.tag.FluidTags.WATER)));
                player.sendMessage(Text.literal("Lava: " + world.getFluidState(new BlockPos(fx,fy,fz)).isIn(net.minecraft.registry.tag.FluidTags.LAVA)));
                player.sendMessage(Text.literal("Submerged: " + player.isSubmergedInWater()));
            }
            if (message.equals("!entities")) {
                ServerWorld world = ((ServerWorld)player.getWorld());
                List<Entity> nearby = world.getEntitiesByType(TypeFilter.instanceOf(Entity.class), new net.minecraft.util.math.Box(player.getX()-16.0,player.getY()-16.0,player.getZ()-16.0,player.getX()+16.0,player.getY()+16.0,player.getZ()+16.0), e -> true);
                player.sendMessage(Text.literal("Nearby entities (16r): " + nearby.count));
                for (Entity e : nearby) {
                    player.sendMessage(Text.literal("  " + e.typeId + " — alive: " + e.isAlive));
                }
            }
        });
        ServerLivingEntityEvents.ALLOW_DAMAGE.register((entity, source, amount) -> {
            if (entity.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.UNDEAD)) {
                entity.setOnFireFor(3);
                entity.getWorld().spawnParticle(McParticles.flame, entity.getX(), entity.getY() + 1, entity.getZ(), 10);
            }
            return true;
        });
        AttackEntityCallback.EVENT.register((player, world, hand, entity, hitResult) -> {
            if (!(player instanceof ServerPlayerEntity)) return ActionResult.PASS;
            Entity target = entity;
            MinecraftServer server = player.getServer();
            ItemStack main = player.getMainHandStack();
            if (main.isIn(net.minecraft.registry.tag.ItemTags.SWORDS)) {
                target.getWorld().spawnParticle(McParticles.crit, target.getX(), target.getY() + 1, target.getZ(), 15);
                target.getWorld().spawnParticle(McParticles.magicCrit, target.getX(), target.getY() + 1, target.getZ(), 10);
                player.sendMessage(Text.literal("Hit " + target.getName().getString() + "!"), true);
            }
            return ActionResult.PASS;
        });
        UseEntityCallback.EVENT.register((player, world, hand, entity, hitResult) -> {
            if (!(player instanceof ServerPlayerEntity)) return ActionResult.PASS;
            MinecraftServer server = player.getServer();
            player.sendMessage(Text.literal("Entity: " + EntityType.getId(entity.getType()).toString()));
            player.sendMessage(Text.literal("Health: " + (entity instanceof LivingEntity ? ((LivingEntity)entity).getHealth() : 0f) + "/" + (entity instanceof LivingEntity ? ((LivingEntity)entity).getMaxHealth() : 0f)));
            player.sendMessage(Text.literal("Baby: " + entity.isBaby()));
            if (entity instanceof net.minecraft.entity.player.PlayerEntity) {
                player.sendMessage(Text.literal("(This is a player)"));
            }
            return ActionResult.PASS;
        });
        EntityPickupItemEvents.ALLOW_ENTITY_PICKUP.register((entity, itemEntity, slot) -> {
            if (!(entity instanceof ServerPlayerEntity)) return true;
            ServerPlayerEntity player = (ServerPlayerEntity) entity;
            ItemStack stack = itemEntity.getStack();
            MinecraftServer server = player.getServer();
            if (!stack.isEmpty() && Registries.ITEM.getId(stack.getItem()).toString().equals("examplemod:ruby")) {
                player.sendMessage(Text.literal("Picked up a ruby!"), true);
                ((ServerWorld)player.getWorld()).spawnParticle(McParticles.glow, player.getX(), player.getY() + 1, player.getZ(), 5);
            }
            return true;
        });
        ServerLifecycleEvents.SERVER_STARTED.register((server) -> {
            LOGGER.info("ExampleMod initialized!");
            { var _sb = server.getScoreboard(); if (_sb.getNullableObjective("kills") == null) _sb.addObjective("kills", ScoreboardCriterion.DUMMY, Text.literal("Kills"), ScoreboardCriterion.RenderType.INTEGER, false, null); };
            server.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.SIDEBAR, server.getScoreboard().getNullableObjective("kills"));
            { int _delay = 200; server.execute(() -> { try { Thread.sleep(_delay * 50L); } catch (Exception _e) {} }); };
            McScheduler.runRepeating(server, 6000, (s, cancel) -> { if (s.getPlayerManager().getCurrentPlayerCount() > 0) { s.getPlayerManager().broadcast(Text.literal("Reminder: Type /kit for a free ruby kit!"), false); } });
        });
        ServerLifecycleEvents.SERVER_STOPPING.register((server) -> {
            LOGGER.info("ExampleMod shutting down.");
        });
        ServerTickEvents.END_SERVER_TICK.register((server) -> {
            float prog = eventBar.getPercent();
            if (prog > 0) {
                eventBar.setPercent(prog - 0.0005f);
            }
        });
        ServerTickEvents.END_WORLD_TICK.register((world) -> {
            if (world.getTime() % 100 == 0 && world.isRaining() && world.isDay()) {
            }
        });
        ServerEntityEvents.ENTITY_LOAD.register((entity, world) -> {
            if (EntityType.getId(entity.getType()).toString().equals("minecraft:wither")) {
                entity.setCustomName(Text.literal("Server Boss"));
                entity.setCustomNameVisible(true);
                entity.addCommandTag("boss");
                LOGGER.info("A wither spawned!");
            }
        });
        ServerLivingEntityEvents.AFTER_DEATH.register((entity, damageSource) -> {
            DamageSource {1} = damageSource;
            if (EntityType.getId(entity.getType()).toString().equals("minecraft:ender_dragon")) {
                ServerWorld world = entity.getWorld();
                world.spawnParticles(Registries.PARTICLE_TYPE.get(Identifier.of(McParticles.dragon)), entity.getX(), entity.getY() + 5, entity.getZ(), 50, 0, 0, 0, 0);
                world.spawnParticles(Registries.PARTICLE_TYPE.get(Identifier.of(McParticles.end)), entity.getX(), entity.getY() + 5, entity.getZ(), 100, 0, 0, 0, 0);
                world.server.broadcast("The Ender Dragon has been defeated!");
            }
        });
        ServerChunkEvents.CHUNK_LOAD.register((world, chunk) -> {
        });
        ServerWorldEvents.LOAD.register((server, world) -> {
            LOGGER.info("World loaded: " + world.dimension);
        });
        ServerWorldEvents.UNLOAD.register((server, world) -> {
            LOGGER.info("World unloaded: " + world.dimension);
        });
        /* TODO: register mob loot for "minecraft:zombie": add "examplemod:ruby" with chance 0.05f in data/modid/loot_table/entities/ */;
        /* TODO: register mob loot for "minecraft:creeper": add "examplemod:ruby" with chance 0.1f in data/modid/loot_table/entities/ */;
        /* TODO: add "examplemod:ruby" to chest loot table — use Fabric LootTableEvents.MODIFY */;
        /* TODO: add "examplemod:ruby" to chest loot table — use Fabric LootTableEvents.MODIFY */;
    }

}
