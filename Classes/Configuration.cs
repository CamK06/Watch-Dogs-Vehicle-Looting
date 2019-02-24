using System.Collections.Generic;

namespace WatchDogsVehLoot
{
    public class Configuration
    {
        public Settings settings { get; set; } // Define the settings that will contain things like the min and max money amount, keybinds, etc
        public List<LootItem> lootItems { get; set; } // Define the list that will have the possible loot items
        public List<PawnShop> pawnShops { get; set; } // Define the list that will contain the pawn shops
        public Inventory inventory { get; set; } // Define the inventory
    }
}
