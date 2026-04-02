using CSCraft;

public class ExampleMod : IMod
{
    public void OnInitialize()
    {
        // Welcome message + starter kit on join
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage($"Welcome to the server, {player.Name}!");
            player.GiveItem("minecraft:bread", 16);
            player.GiveItem("minecraft:iron_sword", 1);
        };

        // Commands via chat
        Events.ChatMessage += (player, message) =>
        {
            if (message == "!heal")
            {
                player.Heal(20);
                player.SendMessage("You have been healed!");
            }

            if (message == "!spawn")
            {
                player.Teleport(0, 64, 0);
                player.SendMessage("Teleported to spawn!");
            }

            if (message == "!gamemode creative")
            {
                player.SetGameMode("creative");
                player.SendMessage("Switched to creative mode.");
            }
        };

        // Notify everyone when a block is broken
        Events.BlockBreak += (player, pos) =>
        {
            player.SendMessage($"You broke a block at {pos.X}, {pos.Y}, {pos.Z}");
        };

        // Goodbye message on leave
        Events.PlayerLeave += (player) =>
        {
            player.SendMessage("Goodbye!");
        };
    }
}