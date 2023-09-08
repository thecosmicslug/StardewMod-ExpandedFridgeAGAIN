using StardewModdingAPI;
using StardewModdingAPI.Events;

using System;
using System.Collections.Generic;

namespace ExpandedFridge
{
    //* The entry point of the mod handled by SMAPI.
    public class ModEntry : Mod
    {
        private static ModEntry _instance = null;

        public ModConfig Config { get; private set; }
        public FridgeManager FridgeManager { get; private set; }

        //* Setup instance and mini-fridge manager on Game Loading.
        public override void Entry(IModHelper helper)
        {
            //* Load Config
            _instance = this;
            Config = Helper.ReadConfig<ModConfig>();
            DebugLog("Configuration loaded from 'config.json'");

            //* Prepare Event Hooks
            Helper.Events.GameLoop.GameLaunched += onLaunched;
            Helper.Events.GameLoop.SaveLoaded +=   onSaveLoaded;
        }

         //* Setup for GenericModConfigMenu
        private void onLaunched(object sender, GameLaunchedEventArgs e)
        {
            //* Hook into GMCM
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (api != null){

                api.RegisterModConfig(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));

                //* Our Options
                api.SetDefaultIngameOptinValue(ModManifest, true);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("Config.ShowDebugMessages"), Helper.Translation.Get("Config.ShowDebugMessagesDesc"), () => Config.ShowDebugMessages, (bool val) => Config.ShowDebugMessages = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("Config.HideMiniFridges"), Helper.Translation.Get("Config.HideMiniFridgesDesc"), () => Config.HideMiniFridges, (bool val) => Config.HideMiniFridges = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("Config.BetterChestSupport"), Helper.Translation.Get("Config.BetterChestSupportDesc"), () => Config.BetterChestSupport, (bool val) => Config.BetterChestSupport = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("Config.NextFridgeTabButton"), Helper.Translation.Get("Config.NextFridgeTabButtonDesc"), () => Config.NextFridgeTabButton, (SButton val) => Config.NextFridgeTabButton = val);
                api.RegisterSimpleOption(ModManifest, Helper.Translation.Get("Config.LastFridgeTabButton"), Helper.Translation.Get("Config.LastFridgeTabButtonDesc"), () => Config.LastFridgeTabButton, (SButton val) => Config.LastFridgeTabButton = val);
                DebugLog("GenericModConfigMenu setup completed.");

            }else{ //* Could not find GenericModConfigMenu ??
                DebugLog("GenericModConfigMenu not found. Configuration can be edited from 'config.json'");
            }

        }

        //* The method invoked when the player loads a save.
        private void onSaveLoaded(object sender, EventArgs e)
        {
            //* Start FridgeManager
            FridgeManager = new FridgeManager(_instance);
        }

        //* Prints message in console log with given log level.
        public static void DebugLog(string message, LogLevel level = LogLevel.Debug)
        {
            if (_instance.Config.ShowDebugMessages){
                _instance.Monitor.Log(message, level);
            }
        }
    }
}
