using StardewModdingAPI;
using StardewModdingAPI.Events;

using System;
using System.Collections.Generic;

namespace ExpandedFridge
{
    //* The entry point of the mod handled by SMAPI.
    public class ModEntry : Mod
    {
        private static bool _instanceInitiated = false;
        private static ModEntry _instance = null;

        public ModConfig Config { get; private set; }
        public FridgeManager FridgeManager { get; private set; }

        //* Setup instance and mini-fridge manager on entry.
        public override void Entry(IModHelper helper)
        {
            _instance = this;
            
            //* Prepare GenericModConfigMenu
            Helper.Events.GameLoop.GameLaunched += onLaunched;

            //* Start FridgeManager
            FridgeManager = new FridgeManager(this);
        }

         //* Setup for GenericModConfigMenu
        private void onLaunched(object sender, GameLaunchedEventArgs e)
        {

            //* Load Config
            Config = Helper.ReadConfig<ModConfig>();

            //* Now Setup GenericModConfigMenu support
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            api.RegisterModConfig(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));

            //* Our Options
            //NOTE: Add Translation support some day.
            api.SetDefaultIngameOptinValue(ModManifest, true);
            api.RegisterSimpleOption(ModManifest, "ShowDebugMessages", "Show Debugging Messages.", () => Config.ShowDebugMessages, (bool val) => Config.ShowDebugMessages = val);
            api.RegisterSimpleOption(ModManifest, "HideMiniFridges", "Hide the mini-fridges.", () => Config.HideMiniFridges, (bool val) => Config.HideMiniFridges = val);
            api.RegisterSimpleOption(ModManifest, "NextFridgeTabButton", "Button to navigate to next tab.", () => Config.NextFridgeTabButton, (SButton val) => Config.NextFridgeTabButton = val);
            api.RegisterSimpleOption(ModManifest, "LastFridgeTabButton", "Button to navigate to previous tab.", () => Config.LastFridgeTabButton, (SButton val) => Config.LastFridgeTabButton = val);

            //* Setup Completed.
            _instanceInitiated = true;
        }

        //* Prints message in console log with given log level.
        public static void DebugLog(string message, LogLevel level = LogLevel.Debug)
        {
            if (_instanceInitiated)
                //* Always show errors.
                if (level == LogLevel.Error){
                     _instance.Monitor.Log(message, level);
                }else{
                    //* Option to hide anything else.
                    if (_instance.Config.ShowDebugMessages){
                        _instance.Monitor.Log(message, level);
                    }
                }
                
        }
    }
}
