using GTA;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GTA.Math;
using System.Drawing;
using Watch_Dogs_Vehicle_Looting.Classes;
using GTA.Native;

namespace Watch_Dogs_Vehicle_Looting
{
    public class Main : Script
    {
        // Looted vehicles (to prevent re-looting)
        private List<Vehicle> lootedVehicles = new List<Vehicle>();

        // Initialization
        public Main()
        {
            Mod.VerifyFileStructure();
            Mod.LoadConfig();
            Mod.CreateBlips();
            InventoryManagement.LoadInventories();

            Tick += OnTick;
            Aborted += Mod.OnAbort;
            KeyDown += OnKeyDown;

            UI.Notify("~y~Watch Dogs Vehicle Looting ~w~successfully loaded!");
        }

        private void OnTick(object sender, EventArgs e)
        {
            // Draw 3d markers at each pawn shop if necessary
            if(Mod.config.settings.pawnShop3dMarkers)
                foreach(PawnShop shop in Mod.config.pawnShops)
                    if (World.GetDistance(Game.Player.Character.Position, new Vector3(shop.markerX, shop.markerY, shop.markerZ)) <= 150) World.DrawMarker(MarkerType.VerticalCylinder, new Vector3(shop.markerX, shop.markerY, shop.markerZ - shop.markerZ / shop.markerZ), Vector3.Zero, Vector3.Zero, new Vector3(1.5f, 1.5f, 0.5f), Color.Yellow);


            // If the player is driving a vehicle, try to loot it
            if(Game.Player.Character.IsInVehicle() && Game.Player.Character.SeatIndex == VehicleSeat.Driver)
            {
                if (!lootedVehicles.Contains(Game.Player.Character.CurrentVehicle) && !Mod.blockedModels.Contains(Game.Player.Character.CurrentVehicle.Model))
                {
                    Wait(2500);
                    if (new Random().Next(1, 100) <= 75) Mod.LootVehicle(Game.Player.Character.CurrentVehicle);
                    lootedVehicles.Add(Game.Player.Character.CurrentVehicle);
                }
            }
            // Otherwise, display the pawnshop sell message if necessary
            else
            {
                Inventory inventory = InventoryManagement.GetInventory((PedHash)Game.Player.Character.Model.GetHashCode());
                foreach (PawnShop shop in Mod.config.pawnShops)
                    if (World.GetDistance(Game.Player.Character.Position, new Vector3(shop.markerX, shop.markerY, shop.markerZ)) <= 1.25f && inventory.pawnItems.Count >= 1)
                        UI.ShowSubtitle($"Press ~y~E ~w~to sell your loot to the pawn shop (${inventory.totalValue})", 1);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Developer commands
            if (Mod.config.settings.devMode)
            {
                if(e.KeyCode == Keys.L && e.Shift) Mod.CreatePawnShop(Game.Player.Character.Position);
            }

            // If the pawnshop use key is pressed
            if(e.KeyCode == Keys.E && !Game.Player.Character.IsInVehicle())
            {
                foreach(PawnShop shop in Mod.config.pawnShops)
                {
                    // If the player is at a pawn shop
                    Vector3 markerPos = new Vector3(shop.markerX, shop.markerY, shop.markerZ);
                    Inventory inventory = InventoryManagement.GetInventory((PedHash)Game.Player.Character.Model.GetHashCode());
                    if (World.GetDistance(Game.Player.Character.Position, markerPos) <= 1.25f && inventory.pawnItems.Count >= 1)
                    {
                        // Sell every item
                        Game.FadeScreenOut(1000);
                        Game.Player.Character.FreezePosition = true;
                        Wait(1000);
                        int itemCount = inventory.pawnItems.Count;
                        int itemValue = inventory.totalValue;
                        inventory.pawnItems.Clear();
                        inventory.totalValue = 0;
                        InventoryManagement.SaveInventory(inventory); 
                        World.CurrentDayTime = new TimeSpan(World.CurrentDayTime.Hours + new Random().Next(1, itemCount), new Random().Next(1, 59), new Random().Next(1, 59));
                        Wait(500);
                        Game.Player.Character.FreezePosition = false; 
                        Game.FadeScreenIn(1000);

                        bool caughtByCops = false;
                        if (new Random().Next(0, 100) >= 50) caughtByCops = true;
                        UI.Notify(caughtByCops.ToString());
                        if (!caughtByCops) // If the item is actually sold
                        {
                            Game.Player.Money = Game.Player.Money + itemValue;
                            if (itemCount > 1) UI.Notify($"You just sold {itemCount} items for ${itemValue}!");
                            else if (itemCount == 1) UI.Notify($"You just sold {itemCount} item for ${itemValue}!");
                        } 
                        else // If the cops are called
                        {
                            UI.Notify("The pawn shop has alerted the cops about your items being stolen!");
                            Game.Player.WantedLevel = 2;
                        }
                    }
                }
            }
        }
    }
}