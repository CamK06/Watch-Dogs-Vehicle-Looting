using GTA;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using WatchDogsVehLoot;
using System.Linq;
using System.Windows.Forms;
using GTA.Math;

namespace WDVH
{
    public class Main : Script
    {
        private Configuration modConfig; // The mod configuration
        private List<Vehicle> lootedVehicles = new List<Vehicle>(); // Create a list of vehicles that have been looted so they cannot be looted again
        private List<Blip> pawnBlips = new List<Blip>(); // Create a list of pawnshop blips to be deleted when the mod is unloaded
        private List<Vector3> pawnLocations = new List<Vector3>(); // Create a list of pawn shop locations to be used in getting if the player is near a shop

        public Main() // Runs when the script is loaded
        {
            VerifyAndLoadJson();
            Tick += OnTick; // Set up OnTick
            Aborted += OnAbort; // When the mod is closed
            KeyDown += OnKeyDown; // When a key is pressed
            UI.Notify("Watch_Dogs Vehicle Loot by ~y~Derpy-Canadian ~w~loaded!"); // Sends a notification above the minimap telling the user that the mod has loaded
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            foreach (Vector3 markerPos in pawnLocations) // Iterate through all pawn shop locations
            {
                if (World.GetDistance(Game.Player.Character.Position, markerPos) <= 2.5f && modConfig.inventory.totalValue > 0 && modConfig.inventory.items.Count >= 1) // If the player is near the pawn shop location and they have items to sell
                {
                    if(e.KeyCode == Keys.E) // if the player is also pressing E
                    {
                        int itemCount = modConfig.inventory.items.Count; // Create a value for the amount of items sold
                        int itemValue = modConfig.inventory.totalValue; // Create a value for the total value of the items
                        modConfig.inventory.items.Clear(); // Clear the inventory
                        modConfig.inventory.totalValue = 0; // Set the total value to 0
                        Game.Player.Money = Game.Player.Money + itemValue; // Add the money from the items
                        UI.Notify($"You just sold {itemCount} items for ${itemValue}!"); // Notify the player about their transaction
                        Save(); // Save the mod config
                    }
                }
            }

            if(e.KeyCode == Keys.L && e.Shift)
            {
                string pawnShopName = Game.GetUserInput("Pawn Shop Name", 25); // Get the name of the pawn shop
                CreatePawnShop(Game.Player.Character.Position, pawnShopName); // Create the pawn shop
                UI.Notify("Successfully created pawn shop!"); // Notify the user that the pawn shop was created
            }
        }

        private void OnAbort(object sender, EventArgs e)
        {
            // Cleanup
            foreach(Blip blp in pawnBlips)
            {
                blp.Remove();
            }
        }

        private void OnTick(object sender, EventArgs e) // Runs every frame
        {
            if (Game.Player.Character.CurrentVehicle != null) // If the player is in a vehicle
            {
                if (!lootedVehicles.Contains(Game.Player.Character.CurrentVehicle)) // Check if the current vehicle is not already looted
                {
                    LootVehicle(Game.Player.Character.CurrentVehicle); // Loot the vehicle
                }
            }
            else
            {
                foreach(Vector3 markerPos in pawnLocations) // Iterate over all pawn shop locations
                {
                    if (World.GetDistance(Game.Player.Character.Position, markerPos) <= 2.5f && modConfig.inventory.totalValue > 0 && modConfig.inventory.items.Count >= 1) // If the player is near the pawn shop location and they have items to sell
                    {
                        UI.ShowSubtitle($"Press ~y~E ~w~to sell your loot to the pawn shop (${modConfig.inventory.totalValue})", 1); // Tell them they can sell their loot
                    }
                }
            }
        }

        private void LootVehicle(Vehicle veh)
        {
            Random r = new Random(); // Create the Random for use with getting random amounts of money and getting a random item for loot
            bool lootIsMoney = false; // Create a bool to determine if the loot is going to be money
            int moneyOrNoMoney = r.Next(0, 100); // Create a random number to randomize the loot and money
            if (moneyOrNoMoney >= 30)  // If the moneyOrNoMoney var is greater or equal than 30 then the player wont get money but will get loot
                lootIsMoney = false;
            else // Else the value is below 50 so the player will get a random amount of cash
                lootIsMoney = true;
            string item = null; // Blank string to be set as the item once one is determined
            int number = r.Next(0, modConfig.lootItems.Count); // Generate a number using the Random from the amount of items in possibleLoot. This number determines what the loot is
            LootItem loot = modConfig.lootItems[number]; // Uses the previously made random number to get an item from the enum

            returnpos:
            if (modConfig == null) { VerifyAndLoadJson(); goto returnpos; } // If the modconfig doesn't exist verify the json and check again
            Wait(1500);

            if (!lootIsMoney) // If the player doesn't get money
            {
                item = $@"{loot.displayName} (${loot.value}) 
Visit a ~g~Pawn Shop ~w~to sell your loot"; // Set the item to be the item retrieved
                modConfig.inventory.items.Add(loot); // Add the item to the inventory
                Save();
            }
            else // If the player does get money
            {
                int moneyToGain = r.Next(modConfig.money.FirstOrDefault(x => x.Key == "minMoney").Value, modConfig.money.FirstOrDefault(x => x.Key == "maxMoney").Value); // Determine a random amount of money based on the config minimum and maximum
                item = $"${moneyToGain}"; // Set the text to say $[moneyEarned]
                Game.Player.Money = Game.Player.Money + moneyToGain; // Add the money to the players wallet
            }

            int totalValue = 0; // Temporary total value var
            foreach (LootItem item2 in modConfig.inventory.items) // For each item in the players inventory
            {
                totalValue = totalValue + item2.value; // Increase the total value
            }
            modConfig.inventory.totalValue = totalValue; // Increase the modconfig total value
            Save(); // Save


            if (veh.Model.IsCar) // If the players current vehicle is a car make the message say Glovebox
                UI.Notify($"Glovebox looted: {item}");
            else if(veh.Model.IsPlane) // If the players current vehicle is a plane make the message say Cargo Bay
                UI.Notify($"Cargo Bay looted: {item}");
            else // If the players current vehicle is not a car or plane make the message say Storage Compartment
                UI.Notify($"Storage Compartment looted: {item}");
            lootedVehicles.Add(veh); // Add the players current vehicle to the list of vehicles not to loot again
        }

        private void VerifyAndLoadJson()
        {
            if (!File.Exists("scripts\\WatchDogsVehLoot.json"))
            {
                modConfig = new Configuration(); // Create the modconfig
                modConfig.lootItems = new List<LootItem>(); // Create a list of loot
                modConfig.pawnShops = new List<PawnShop>(); // Create a list of pawn shops
                modConfig.inventory = new Inventory()
                {
                    items = new List<LootItem>(),
                    totalValue = 0
                }; // Create a list of inventories

                // Add the items to the loot list
                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "iFruit 9iX",
                    value = 500
                });

                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "PlayStadium 4",
                    value = 650
                });

                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "Silver Ring",
                    value = 500
                });

                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "Gold Ring",
                    value = 1000
                });

                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "Microshaft Winblows 98 Tablet",
                    value = 750
                });

                modConfig.money = new Dictionary<string, int> // Set the min and max money possible to get
                {
                    { "minMoney", 25 },
                    { "maxMoney", 5000 }
                };

                try
                {
                    // Add the pawn shops to the pawn list
                    modConfig.pawnShops.Add(new PawnShop
                    {
                        friendlyName = "Boulevard Del Perro Pawn & Jewelry",
                        markerX = -1459.451f,
                        markerY = -414.505f,
                        markerZ = 35.714f
                    });



                    string json = JsonConvert.SerializeObject(modConfig, Formatting.Indented); // Serialize the loot dictionary to json
                    File.WriteAllText("scripts\\WatchDogsVehLoot.json", json); // Write a new json file with the dictionary in it
                    VerifyAndLoadJson();
                }
                catch(Exception e)
                {
                    UI.Notify(e.Message);
                }
            }
            else
            {
                string json = File.ReadAllText("scripts\\WatchDogsVehLoot.json"); // Read the json for use in conversion to List
                Configuration config = JsonConvert.DeserializeObject<Configuration>(json); // Convert the json to list
                modConfig = config; // Set modconfig to be equivelant to the json config
                foreach(PawnShop shop in modConfig.pawnShops) // For every pawn shop
                {
                    Vector3 markerPositions = new Vector3(shop.markerX, shop.markerY, shop.markerZ); // Make a vector3 with the marker position
                    Blip shopBlip = World.CreateBlip(markerPositions); // Create a blip on the shop
                    shopBlip.IsShortRange = true; // Set the blip to be short range
                    shopBlip.Sprite = BlipSprite.DollarBill; // Set the icon to be a bill
                    shopBlip.Color = BlipColor.Green; // Make the colour green
                    shopBlip.Name = "Pawn Shop"; // Rename the blip
                    pawnBlips.Add(shopBlip); // Add the blip to the list of pawn blips
                    pawnLocations.Add(markerPositions); // Add the location to the locations list
                }
                UI.Notify($"Loaded ~y~{modConfig.lootItems.Count} ~w~loot item(s) & ~y~{modConfig.pawnShops.Count} ~w~Pawn Shop(s)!"); // Notify the user that the config loaded
            }
        }


        private void Save()
        {
            string json = JsonConvert.SerializeObject(modConfig, Formatting.Indented); // Serialize the config
            if(json != null && json != "") File.WriteAllText("scripts\\WatchDogsVehLoot.json", json); // Write the config if the json is not blank
        }

        private void CreatePawnShop(Vector3 positionOfShop, string shopName)
        {
            Blip shopBlip = World.CreateBlip(positionOfShop); // Create a blip on the shop
            shopBlip.IsShortRange = true; // Set the blip to be short range
            shopBlip.Sprite = BlipSprite.DollarBill; // Set the icon to be a bill
            shopBlip.Color = BlipColor.Green; // Make the colour green
            shopBlip.Name = "Pawn Shop"; // Rename the blip
            pawnBlips.Add(shopBlip); // Add the blip to the list of pawn blips
            pawnLocations.Add(positionOfShop); // Add the location to the locations list
            PawnShop shop = new PawnShop() // Create the pawn shop variable that will be saved
            {
                friendlyName = shopName,
                markerX = positionOfShop.X,
                markerY = positionOfShop.Y,
                markerZ = positionOfShop.Z
            };
            modConfig.pawnShops.Add(shop); // Add the shop to the modconfig
            Save(); // Save
        }
    }
}
