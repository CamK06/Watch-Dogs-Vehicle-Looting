using GTA.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Watch_Dogs_Vehicle_Looting.Classes.Items
{
    public class Weapon
    {
        public string weaponName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WeaponHash weaponHash { get; set; }
    }
}
