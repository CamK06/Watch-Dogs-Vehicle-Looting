using System;
using System.Collections.Generic;
using System.IO;
using GTA;
using GTA.Math;
using Newtonsoft.Json;
using Watch_Dogs_Vehicle_Looting.Classes;
using Watch_Dogs_Vehicle_Looting.Classes.Items;

namespace Watch_Dogs_Vehicle_Looting
{
    public class Mod
    {
        // Configuration stuff
        public static Configuration config;
        public static List<Item> items;
        public static List<Food> food;
        public static List<Classes.Items.Weapon> weapons;

        // Directories
        public static string resourceDir = "scripts\\Watch Dogs Vehicle Looting";
        public static string inventoryDir = $"{resourceDir}\\Inventories";
        public static string itemDir = $"{resourceDir}\\Items";

        // Files
        public static string pawnItemsJson = $"{itemDir}\\pawnItems.json";
        public static string foodItemsJson = $"{itemDir}\\foodItems.json";
        public static string weaponsJson = $"{itemDir}\\weapons.json";
        public static string modConfig = $"{resourceDir}\\modConfig.json";

        // Misc
        public static List<Blip> blips = new List<Blip>();

        public static void VerifyFileStructure()
        {
            // Ensure that all necessary directories and files exist
            if (!Directory.Exists(resourceDir)) Directory.CreateDirectory(resourceDir);
            if (!Directory.Exists(inventoryDir)) Directory.CreateDirectory(inventoryDir);
            if (!Directory.Exists(itemDir)) Directory.CreateDirectory(itemDir);

            // Items
            if (!File.Exists(pawnItemsJson)) File.WriteAllText(pawnItemsJson, JsonConvert.SerializeObject(Defaults.defaultItems, Formatting.Indented));
            if (!File.Exists(foodItemsJson)) File.WriteAllText(foodItemsJson, JsonConvert.SerializeObject(Defaults.defaultFood, Formatting.Indented));
            if (!File.Exists(weaponsJson)) File.WriteAllText(weaponsJson, JsonConvert.SerializeObject(Defaults.defaultWeapons, Formatting.Indented));

            // Configuration
            if (!File.Exists(modConfig)) File.WriteAllText(modConfig, JsonConvert.SerializeObject(new Configuration() { pawnShops = Defaults.defaultShops, blockedVehicles = new List<string>(), settings = new Settings() }, Formatting.Indented));
        }

        public static void LoadConfig()
        {
            // Deserialize all of the json files
            config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(modConfig));
            items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(pawnItemsJson));
            food = JsonConvert.DeserializeObject<List<Food>>(File.ReadAllText(foodItemsJson));
            weapons = JsonConvert.DeserializeObject<List<Classes.Items.Weapon>>(File.ReadAllText(weaponsJson));
        }

        public static void CreateBlips()
        {
            // Iterate over each pawnshop and create a blip
            foreach(PawnShop shop in config.pawnShops)
            {
                Blip newBlip = World.CreateBlip(new Vector3(shop.markerX, shop.markerY, shop.markerZ));
                newBlip.IsShortRange = true;
                newBlip.Sprite = BlipSprite.DollarBill;
                newBlip.Color = BlipColor.Green;
                newBlip.Name = "Pawn Shop";
                blips.Add(newBlip);
            }
        }

        public static void CreatePawnShop(Vector3 location)
        {
            // Create the pawnshop blip
            Blip newBlip = World.CreateBlip(location);
            newBlip.IsShortRange = true;
            newBlip.Sprite = BlipSprite.DollarBill;
            newBlip.Color = BlipColor.Green;
            newBlip.Name = "Pawn Shop";

            PawnShop shop = new PawnShop()
            {
                displayName = World.GetStreetName(location) + " Pawn",
                markerX = location.X,
                markerY = location.Y,
                markerZ = location.Z
            };

            // Add the blip and shop
            config.pawnShops.Add(shop);
            blips.Add(newBlip);

            // Save the configuration json
            File.WriteAllText(modConfig, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        internal static void OnAbort(object sender, EventArgs e)
        {
            // Iterate over and remove each blip
            foreach (Blip blip in blips) blip.Remove();
        }
    }
}