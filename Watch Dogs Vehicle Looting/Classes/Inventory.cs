using GTA.Native;
using System.Collections.Generic;
using Watch_Dogs_Vehicle_Looting.Classes.Items;

namespace Watch_Dogs_Vehicle_Looting.Classes
{
    public class Inventory
    {
        public PedHash attachedPed { get; set; }
        public int totalValue { get; set; }
        public List<Item> pawnItems { get; set; }
    }
}
