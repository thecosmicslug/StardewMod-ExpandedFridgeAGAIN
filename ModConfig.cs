using StardewModdingAPI;

namespace ExpandedFridge
{
    //* Stores options for the Manager.
    public class ModConfig
    {
        public bool HideMiniFridges { get; set; } = true;
        public SButton NextFridgeTabButton { get; set; } = SButton.RightTrigger;
        public SButton LastFridgeTabButton { get; set; } = SButton.LeftTrigger;
    }
}
