using System.Collections.Generic;

namespace Watch_Dogs_Vehicle_Looting.Classes
{
    public class Configuration
    {
        public Settings settings { get; set; }
        public List<string> blockedVehicles { get; set; }
        public List<PawnShop> pawnShops { get; set; }
    }

    public class Settings
    {
        public bool pawnShop3dMarkers { get; set; } = true;
        public bool devMode { get; set; } = false;
        public Money money { get; set; } = new Money();
    }

    public class Money
    {
        public int minMoney { get; set; } = 25;
        public int maxMoney { get; set; } = 2500;
    }
}
