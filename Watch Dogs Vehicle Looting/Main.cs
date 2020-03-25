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

namespace Watch_Dogs_Vehicle_Looting
{
    public class Main : Script
    {
        // Initialization
        public Main()
        {
            Mod.VerifyFileStructure();
            Mod.LoadConfig();
            Mod.CreateBlips();

            Tick += OnTick;
            Aborted += Mod.OnAbort;
            KeyDown += OnKeyDown;

            UI.Notify("Watch Dogs Vehicle Looting successfully loaded!");
        }

        private void OnTick(object sender, EventArgs e)
        {
            
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Developer commands
            if (Mod.config.settings.devMode)
            {
                if(e.KeyCode == Keys.L && e.Shift) Mod.CreatePawnShop(Game.Player.Character.Position);
            }

            // Pawnshop use key
            if(e.KeyCode == Keys.E)
            {
                foreach(PawnShop shop in Mod.config.pawnShops)
                {
                    Vector3 markerPos = new Vector3(shop.markerX, shop.markerY, shop.markerZ);
                    if (World.GetDistance(Game.Player.Character.Position, markerPos) <= 1.25f)
                    {

                    }
                }
            }
        }
    }
}