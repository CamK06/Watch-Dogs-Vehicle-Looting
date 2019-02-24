using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WatchDogsVehLoot
{
    public class Settings
    {
        public Dictionary<string, int> money { get; set; } // Define the dictionary that will have the min and max money values
        public bool doPawnShopsHave3dMarkers { get; set; } // Define the boolean to check if pawn shops will have a 3d marker
        public bool doesAddPawnRequireShift { get; set; } // Define the boolean to check if adding a pawn shop will require shift to be pressed
        public bool doesAddItemRequireShift { get; set; } // Define the boolean to check if adding an item shop will require shift to be pressed
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys addPawnKey { get; set; } // Define the key to press to add a pawn shop
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys addItemKey { get; set; } // Define the key to press to add an item
    }
}
