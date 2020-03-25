using System.Collections.Generic;
using Watch_Dogs_Vehicle_Looting.Classes.Items;
using GTA.Native;
using Watch_Dogs_Vehicle_Looting.Classes;

namespace Watch_Dogs_Vehicle_Looting
{
    public class Defaults
    {
        // Define all of the default pawn items
        public static List<Item> defaultItems = new List<Item>()
        {
            new Item()
            {
                name = "Playstadium 4",
                value = 650
            },
            new Item()
            {
                name = "iFruit 9iX",
                value = 500
            },
            new Item()
            {
                name = "Silver Ring",
                value = 250
            },
            new Item()
            {
                name = "Gold Ring",
                value = 1000
            },
            new Item()
            {
                name = "Microshaft Winblows 98 Tablet",
                value = 200
            },
            new Item()
            {
                name = "Krolex Watch",
                value = 125
            }
        };

        // Define all of the default food items
        public static List<Food> defaultFood = new List<Food>()
        {
            new Food()
            {
                name = "Hot Dog",
                healsPlayer = true
            },
            new Food()
            {
                name = "Hamburger",
                healsPlayer = true
            },
            new Food()
            {
                name = "Moldy Hot Dog",
                healsPlayer = false
            },
            new Food()
            {
                name = "Moldy Hamburger",
                healsPlayer = false
            }
        };

        // Define all of the default weapons
        public static List<Weapon> defaultWeapons = new List<Weapon>()
        {
            new Weapon()
            {
                weaponName = "Pistol",
                weaponHash = WeaponHash.Pistol
            },
            new Weapon()
            {
                weaponName = "Grenade",
                weaponHash = WeaponHash.Grenade
            },
            new Weapon()
            {
                weaponName = "Sawn-Off Shotgun",
                weaponHash = WeaponHash.SawnOffShotgun
            }
        };

        // Define all of the default pawn shops
        public static List<PawnShop> defaultShops = new List<PawnShop>()
        {
            new PawnShop()
            {
                displayName = "Del Perro Pawn & Jewelry",
                markerX = -1459.451f,
                markerY = -414.505f,
                markerZ = 35.714f
            },
            new PawnShop()
            {
                displayName = "Strawberry Pawn & Jewelery",
                markerX = 183.448f,
                markerY = -1319.98f,
                markerZ = 29.322f
            },
            new PawnShop()
            {
                displayName = "Carson Ave Discount Jewels",
                markerX = 133.185f,
                markerY = -1769.581f,
                markerZ = 29.409f
            },
            new PawnShop()
            {
                displayName = "F.T. Pawn",
                markerX = 412.780243f,
                markerY = 313.337982f,
                markerZ = 103.020088f
            }
        };
    }
}
