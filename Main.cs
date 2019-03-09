using GTA;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using WatchDogsVehLoot;
using System.Linq;
using System.Windows.Forms;
using GTA.Math;
using System.Drawing;

namespace WDVH
{
    public class Main : Script
    {
        public Configuration modConfig; // The mod configuration
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
            if (e.KeyCode == Keys.E) // if the player is also pressing E
            {
                foreach (Vector3 markerPos in pawnLocations) // Iterate through all pawn shop locations
                {
                    if (World.GetDistance(Game.Player.Character.Position, markerPos) <= 1.25f && modConfig.inventory.totalValue > 0 && modConfig.inventory.items.Count >= 1) // If the player is near the pawn shop location and they have items to sell
                    {
                        Game.FadeScreenOut(1000); // Fade the screen out
                        Game.Player.Character.FreezePosition = true; // Freeze the player to prevent them from walking as the screen is faded
                        Wait(1000); // Wait 1000ms
                        int itemCount = modConfig.inventory.items.Count; // Create a value for the amount of items sold
                        int itemValue = modConfig.inventory.totalValue; // Create a value for the total value of the items
                        modConfig.inventory.items.Clear(); // Clear the inventory
                        modConfig.inventory.totalValue = 0; // Set the total value to 0
                        Save(); // Save the mod config
                        World.CurrentDayTime = new TimeSpan(World.CurrentDayTime.Hours + new Random().Next(1, itemCount), new Random().Next(1,59), new Random().Next(1, 59)); // Increase the time of the day
                        Wait(500); // Wait 500ms
                        Game.Player.Character.FreezePosition = false; // Unfreeze the player
                        Game.FadeScreenIn(1000); // Fade the screen in
                        Game.Player.Money = Game.Player.Money + itemValue; // Add the money from the items
                        if (itemCount > 1) UI.Notify($"You just sold {itemCount} items for ${itemValue}!"); // If the player is selling more than one item display that they sold {x} itemS
                        else if (itemCount == 1) UI.Notify($"You just sold {itemCount} item for ${itemValue}!");// If the player is selling one item display that they sold {x} item
                    }
                }
            }

            if (e.KeyCode == modConfig.settings.addPawnKey) // If the player pressed the key to add a pawn shop
            {
                if(modConfig.settings.doesAddPawnRequireShift) // If adding pawn shops requires shift
                {
                    if (e.Shift) // If shift is pressed
                    {
                        GetInputForNewPawnShop(); // Call the method used to get the information on the new shop and create it
                    }
                }
                else // If it doesn't require shift
                {
                    GetInputForNewPawnShop(); // Call the method used to get the information on the new shop and create it
                }
            }

            if(e.KeyCode == modConfig.settings.addItemKey) // If the player pressed the ky to add an item
            {
                if (modConfig.settings.doesAddItemRequireShift) // If adding items requires shift
                {
                    if (e.Shift)
                    {
                        GetInputForNewPawnItem(); // Call the method used to get the information on the new item and create it
                    }
                }
                else // If it doesn't require shift
                {
                    GetInputForNewPawnItem(); // Call the method used to get the information on the new item and create it
                }
            }
        }

        private void OnAbort(object sender, EventArgs e)
        {
            // Cleanup
            foreach(Blip blp in pawnBlips)
            {
                blp.Remove(); // Remove the blip
            }
        }

        private void OnTick(object sender, EventArgs e) // Runs every frame
        {
            if(modConfig.settings.doPawnShopsHave3dMarkers)
            {
                foreach(Vector3 pawnShop in pawnLocations)
                {
                    if(World.GetDistance(Game.Player.Character.Position, pawnShop) <= 150) World.DrawMarker(MarkerType.VerticalCylinder, new Vector3(pawnShop.X, pawnShop.Y, pawnShop.Z - pawnShop.Z / pawnShop.Z), Vector3.Zero, Vector3.Zero, new Vector3(1.5f, 1.5f, 0.5f), Color.Yellow); // If the player is 150 or less units from the shop location then show the marker
                }
            }

            if (Game.Player.Character.CurrentVehicle != null) // If the player is in a vehicle
            {
                if (!lootedVehicles.Contains(Game.Player.Character.CurrentVehicle)) // Check if the current vehicle is not already looted
                {
                    int randomNumber = new Random().Next(1, 100); // Create a random number to determine if the vehicle will even have loot to begin with
                    if(randomNumber <= 75) LootVehicle(Game.Player.Character.CurrentVehicle); // if the random number is less than or equal to 75 then loot the vehicle
                }
            }
            else
            {
                foreach(Vector3 markerPos in pawnLocations) // Iterate over all pawn shop locations
                {
                    if (World.GetDistance(Game.Player.Character.Position, markerPos) <= 1.25f && modConfig.inventory.totalValue > 0 && modConfig.inventory.items.Count >= 1) // If the player is near the pawn shop location and they have items to sell
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
        redraw:
            int number = r.Next(0, modConfig.lootItems.Count); // Generate a number using the Random from the amount of items in possibleLoot. This number determines what the loot is


            // This whole section of code is really useless, I have absolutely no idea how to actually get a random item based on probability, which is of course based on the value (START)

            // Create two random loot items
            LootItem loot1 = modConfig.lootItems[number];
            LootItem loot2 = modConfig.lootItems[number];
            // Create an integer for the probability of each item
            int loot1Prob = 75;
            int loot2Prob = 75;
            int randomPercent = r.Next(1, 99); // Create a random number from 1 to 99 to determine which loot item is picked
            // If loot1's value is greater than loot2's value set loot1's probability to be lower, and vice versa for loot2
            if (loot1.value > loot2.value) loot1Prob = 25;
            else if (loot2.value > loot1.value) loot2Prob = 25;

            LootItem loot; // Create a new loot item variable

            // Determine which of the two loot items is picked
            if (loot1Prob >= randomPercent) loot = loot1;
            else loot = loot2;

            // END


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
                int moneyToGain = r.Next(modConfig.settings.money.FirstOrDefault(x => x.Key == "minMoney").Value, modConfig.settings.money.FirstOrDefault(x => x.Key == "maxMoney").Value); // Determine a random amount of money based on the config minimum and maximum
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
                modConfig.settings = new Settings();
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

                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "Krolex Watch",
                    value = 125
                });

                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "Diamond Earrings",
                    value = 100
                });

                modConfig.lootItems.Add(new LootItem()
                {
                    displayName = "Electronic Components",
                    value = 25
                });

                modConfig.settings.money = new Dictionary<string, int> // Set the min and max money possible to get
                {
                    { "minMoney", 25 },
                    { "maxMoney", 5000 }
                };

                // Define the default keys for adding items and pawn shops
                modConfig.settings.addItemKey = Keys.K;
                modConfig.settings.addPawnKey = Keys.L;
                // Make both adding pawn shops and items require shift
                modConfig.settings.doesAddItemRequireShift = true;
                modConfig.settings.doesAddPawnRequireShift = true;

                modConfig.settings.doPawnShopsHave3dMarkers = false; // Make pawn shops not have a 3d marker at their location

                // Add the pawn shops to the pawn list
                modConfig.pawnShops.Add(new PawnShop
                {
                    friendlyName = "Boulevard Del Perro Pawn & Jewelry",
                    markerX = -1459.451f,
                    markerY = -414.505f,
                    markerZ = 35.714f
                });

                modConfig.pawnShops.Add(new PawnShop
                {
                    friendlyName = "Strawberry Ave Pawn & Jewelry",
                    markerX = 183.448f,
                    markerY = -1319.98f,
                    markerZ = 29.322f
                });

                modConfig.pawnShops.Add(new PawnShop
                {
                    friendlyName = "Carson Ave Discount Jewels",
                    markerX = 133.185f,
                    markerY = -1769.581f,
                    markerZ = 29.409f
                });

                modConfig.pawnShops.Add(new PawnShop
                {
                    friendlyName = "F.T. Pawn",
                    markerX = 412.780243f,
                    markerY = 313.337982f,
                    markerZ = 103.020088f
                });

                string json = JsonConvert.SerializeObject(modConfig, Formatting.Indented); // Serialize the loot dictionary to json
                File.WriteAllText("scripts\\WatchDogsVehLoot.json", json); // Write a new json file with the dictionary in it
                VerifyAndLoadJson();
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
                // Create two strings for non plural words
                string itemOrItemS = "item";
                string shopOrShopS = "Shop";

                // Change the non plural strings to be plural if there's more than one item or shop
                if (modConfig.lootItems.Count > 1) itemOrItemS = "items";
                if (modConfig.pawnShops.Count > 1) shopOrShopS = "Shops";

                UI.Notify($"Loaded ~y~{modConfig.lootItems.Count} ~w~loot {itemOrItemS} & ~y~{modConfig.pawnShops.Count} ~w~Pawn {shopOrShopS}!"); // Notify the user that the config loaded
            }
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(modConfig, Formatting.Indented); // Serialize the config
            if(json != null && json != "") File.WriteAllText("scripts\\WatchDogsVehLoot.json", json); // Write the config if the json is not blank
        }

        private void GetInputForNewPawnItem()
        {
            string itemName = Game.GetUserInput("Item Name", 25); // Get the item name and store it in a string
            int itemValue; // Define the item value integer
        returnpoint:
            try
            {
                Int32.TryParse(Game.GetUserInput("Value", 7), out itemValue); // Try to convert the item value to an int and store it in itemValue
            }
            catch // if the game could not parse the string
            {
                UI.Notify("Invalid Input. Please try again"); // Notify the user of their fault
                Wait(500); // Give the player a tiny bit of time before re opening the GetUserInput window
                goto returnpoint; // Return to returnpoint to get input again
            }
            if (itemValue != 0 && itemName != "" && itemName != null)
            {
                CreatePawnItem(itemName, itemValue);
                UI.Notify("Successfully added item!");
            }
        }

        private void GetInputForNewPawnShop()
        {
            string pawnShopName = Game.GetUserInput("Pawn Shop Name", 25); // Get the name of the pawn shop
            CreatePawnShop(Game.Player.Character.Position, pawnShopName); // Create the pawn shop
            UI.Notify("Successfully created pawn shop!"); // Notify the user that the pawn shop was created
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

        private void CreatePawnItem(string displayName, int value)
        {
            LootItem item = new LootItem()
            {
                displayName = displayName,
                value = value
            };
            modConfig.lootItems.Add(item); // Add the item to the modconfig
            Save(); // Save
        }
    }
}
