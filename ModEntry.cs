using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace ExpandedFridge
{
    //* The entry point of the mod handled by SMAPI.
    public class ModEntry : Mod
    {
        private static bool _instanceInitiated = false;
        private static ModEntry _instance = null;

        public ModConfig Config { get; private set; }
        public Manager Manager { get; private set; }

        //* Setup instance and mini fridge manager on entry.
        public override void Entry(IModHelper helper)
        {
            _instance = this;
            _instanceInitiated = true;

            //* Prepare GenericModConfigMenu
            Helper.Events.GameLoop.GameLaunched += onLaunched;

            //* Start Manager
            Manager = new Manager(this);
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
            api.SetDefaultIngameOptinValue( ModManifest, true );
            api.RegisterSimpleOption(ModManifest, "HideMiniFridges", "Hide the mini-fridges.", () => Config.HideMiniFridges, (bool val) => Config.HideMiniFridges = val);
            api.RegisterSimpleOption(ModManifest, "NextFridgeTabButton", "Button to navigate to next tab.", () => Config.NextFridgeTabButton, (SButton val) => Config.NextFridgeTabButton = val);
            api.RegisterSimpleOption(ModManifest, "LastFridgeTabButton", "Button to navigate to previous tab.", () => Config.LastFridgeTabButton, (SButton val) => Config.LastFridgeTabButton = val);

        }

        //* Prints message in console log with given log level.
        public static void DebugLog(string message, LogLevel level = LogLevel.Trace)
        {
            if (_instanceInitiated)
                _instance.Monitor.Log(message, level);
        }
    }
}
