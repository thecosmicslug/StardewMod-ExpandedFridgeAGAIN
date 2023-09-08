using StardewModdingAPI;

namespace ExpandedFridge
{
    //* Stores options for the FridgeManager.
    public class ModConfig
    {
        public bool HideMiniFridges { get; set; } = true;
        public bool ShowDebugMessages { get; set; } = false;
        public bool BetterChestSupport { get; set; } = false;
        public SButton NextFridgeTabButton { get; set; } = SButton.RightTrigger;
        public SButton LastFridgeTabButton { get; set; } = SButton.LeftTrigger;
    }
}
