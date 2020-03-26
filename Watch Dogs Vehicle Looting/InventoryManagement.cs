using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTA.Native;
using Newtonsoft.Json;
using Watch_Dogs_Vehicle_Looting.Classes;
using Watch_Dogs_Vehicle_Looting.Classes.Items;

namespace Watch_Dogs_Vehicle_Looting
{
    public class InventoryManagement
    {
        private static List<Inventory> inventories;

        public static void LoadInventories()
        {
            // Load the inventories
            inventories = JsonConvert.DeserializeObject<List<Inventory>>(File.ReadAllText(Mod.inventories));
        }

        public static Inventory GetInventory(PedHash ped)
        {
            // Try to get and return an inventory for the model
            Inventory inventory = inventories.FirstOrDefault(x => x.attachedPed == ped);
            if (inventory != null) return inventory;
            else
            {
                // Create a new inventory
                Inventory newInventory = new Inventory()
                {
                    attachedPed = ped,
                    pawnItems = new List<Item>(),
                    totalValue = 0
                };

                // Add and return the inventory
                inventories.Add(newInventory);
                return newInventory;
            }
        }

        public static void SaveInventory(Inventory inv)
        {
            File.WriteAllText(Mod.inventories, JsonConvert.SerializeObject(inventories, Formatting.Indented));
        }
    }
}
