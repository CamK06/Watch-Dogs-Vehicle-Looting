using System.Collections.Generic;

namespace WatchDogsVehLoot
{
    public class Configuration
    {
        public Dictionary<string, int> money { get; set; } // Define the dictionary that will have the min and max money values
        public List<LootItem> lootItems { get; set; } // Define the list that will have the possible loot items
        public List<PawnShop> pawnShops { get; set; } // Define the list that will contain the pawn shops
        public Inventory inventory { get; set; } // Define the inventory
    }
}
