using System;
using System.Collections.Generic;
using Watch_Dogs_Vehicle_Looting.Classes.Items;

namespace Watch_Dogs_Vehicle_Looting.Classes
{
    [Serializable]
    public class Inventory
    {
        public string attachedModel { get; set; }
        public int totalValue { get; set; }
        public List<Item> pawnItems { get; set; }
    }
}
