using System.Collections.Generic;

namespace WatchDogsVehLoot
{
    public class Inventory
    {
        public List<LootItem> items { get; set; } // Create a list of items in the inventory
        public int totalValue { get; set; } // Create an int for the total value of the items
    }
}
