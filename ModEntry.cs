using StardewModdingAPI;
using StardewModdingAPI.Events;

using System;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;

namespace ExpandedFridgeAGAIN
{
    //* The entry point of the mod handled by SMAPI.
    public class ModEntry : Mod
    {
        private static ModEntry _instance = null;
        public ModConfig Config { get; private set; }
        public FridgeManager FridgeManager { get; private set; }

        private void DebugRestoreAllFridges (string command, string[] arg){
            FridgeManager.MoveAllMiniFridges(false);
        }

        private void DebugHideAllFridges (string command, string[] arg){
            FridgeManager.MoveAllMiniFridges(true);
        }

        //* Setup instance and load our first event-hooks
        public override void Entry(IModHelper helper)
        {
            //* Setup DebugLog
            _instance = this;

            //* Load Config
            Config = Helper.ReadConfig<ModConfig>();
            DebugLog(Helper.Translation.Get("Debug.ConfigurationLoaded"));

            //* Show debug info etc.
            var buildTime = GetBuildDate(Assembly.GetExecutingAssembly());
            buildTime = buildTime.ToLocalTime();
            DebugLog("ExpandedFridgeAGAIN v" + GetType().Assembly.GetName().Version.ToString(3) +" (" + Constants.TargetPlatform + ") loaded.", LogLevel.Info);
            if (Config.ShowDebugMessages){
                DebugLog("Binary Compiled: " + buildTime.ToString("d/M/yyyy h:mm tt"), LogLevel.Info);
            }

            //* Prepare Event Hooks
            Helper.Events.GameLoop.GameLaunched += onLaunched;
        }

         //* Setup for GenericModConfigMenu support.
        private void onLaunched(object sender, GameLaunchedEventArgs e)
        {

            //* Hook into GMCM
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (api != null){
                
                //* Register with GMCM
                api.Register(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));

                //* Our Key-binds
                api.AddKeybind(ModManifest, () => Config.NextFridgeTabButton, (SButton val) => Config.NextFridgeTabButton = val, () => Helper.Translation.Get("Config.NextFridgeTabButton"), () => Helper.Translation.Get("Config.NextFridgeTabButtonDesc"),"NextFridgeTabButton");
                api.AddKeybind(ModManifest, () => Config.LastFridgeTabButton, (SButton val) => Config.LastFridgeTabButton = val, () => Helper.Translation.Get("Config.LastFridgeTabButton"), () => Helper.Translation.Get("Config.LastFridgeTabButtonDesc"),"LastFridgeTabButton");

                //* Our Options
                api.AddBoolOption(ModManifest,() => Config.ShowDebugMessages, (bool val) => Config.ShowDebugMessages = val, ()=> Helper.Translation.Get("Config.ShowDebugMessages") + ".", () => Helper.Translation.Get("Config.ShowDebugMessagesDesc"),"ShowDebugMessages");
                api.AddBoolOption(ModManifest,() => Config.HideMiniFridges, (bool val) => Config.HideMiniFridges = val, ()  => Helper.Translation.Get("Config.HideMiniFridges") + ".", () => Helper.Translation.Get("Config.HideMiniFridgesDesc"),"HideMiniFridges");

                //* Detect changes mid-game.
                api.OnFieldChanged(ModManifest,onFieldChanged);

                DebugLog(Helper.Translation.Get("Debug.GenericModConfigMenuFound"));

            }else{ //* Could not find GenericModConfigMenu ??
                DebugLog(Helper.Translation.Get("Debug.GenericModConfigMenuMissing"));
            }

            //* Check for incompatible mods.
            //TODO: Check for more incompatible mods.
            bool BetterChestsLoaded = Helper.ModRegistry.IsLoaded("furyx639.BetterChests");
            if (BetterChestsLoaded){
                DebugLog(Helper.Translation.Get("Debug.BetterChestsDetected"), LogLevel.Warn);
            }    

            bool ExpandedFridgeLoaded = Helper.ModRegistry.IsLoaded("Uwazouri.ExpandedFridge");
            if (ExpandedFridgeLoaded){
                DebugLog(Helper.Translation.Get("Debug.ExpandedFridgeDetected"), LogLevel.Warn);
            }    

            //* Print options to the log
            if (Config.ShowDebugMessages){
                DebugLog(Helper.Translation.Get("Debug.OptionEnabledRuntime", new { option = Helper.Translation.Get("Config.ShowDebugMessages") }), LogLevel.Info);
                if (Config.HideMiniFridges){
                    DebugLog(Helper.Translation.Get("Debug.OptionEnabledRuntime", new { option = Helper.Translation.Get("Config.HideMiniFridges") }), LogLevel.Info);
                }else{
                    DebugLog(Helper.Translation.Get("Debug.OptionDisabledRuntime", new { option = Helper.Translation.Get("Config.HideMiniFridges") }), LogLevel.Info);
                }

                DebugLog(Helper.Translation.Get("Debug.KeySetTo", new { option = "NextFridgeTabButton" }) + Config.NextFridgeTabButton.ToString() + ".", LogLevel.Info);
                DebugLog(Helper.Translation.Get("Debug.KeySetTo", new { option = "LastFridgeTabButton" }) + Config.LastFridgeTabButton.ToString() + ".", LogLevel.Info);

            }
  
            //* Start FridgeManager once we are all setup & before the start of the first day.
            FridgeManager = new FridgeManager(_instance);

            //* Debug Commands
            Helper.ConsoleCommands.Add("FridgeExpandedAGAIN_ShowFridges", "Moves all mini-fridges back into the cabins.", this.DebugRestoreAllFridges);
            Helper.ConsoleCommands.Add("FridgeExpandedAGAIN_HideFridges", "Moves all mini-fridges out of view.", this.DebugHideAllFridges);
        }

        //* The method invoked when we detect configuration changes.
        private void onFieldChanged(string str, object obj)
        {
            if (str == "ShowDebugMessages"){
                if((bool)obj){
                    DebugLog(Helper.Translation.Get("Debug.OptionEnabledRuntime", new { option = str }));
                }else{
                    DebugLog(Helper.Translation.Get("Debug.OptionDisabledRuntime", new { option = str }));
                }
            }
            else if(str == "HideMiniFridges"){
                if((bool)obj){
                    DebugLog(Helper.Translation.Get("Debug.OptionEnabledRuntime", new { option = str }));
                    //* Hide the mini-fridges straight away.
                    string[] arrTeamMembers = new string[] { "" };
                    DebugHideAllFridges("",arrTeamMembers);
                }else{
                    DebugLog(Helper.Translation.Get("Debug.OptionDisabledRuntime", new { option = str }));
                    //* Restore the mini-fridges straight away.
                    string[] arrTeamMembers = new string[] { "" };
                    DebugRestoreAllFridges("",arrTeamMembers);
                }
            }
            else if(str == "NextFridgeTabButton"){
                DebugLog(Helper.Translation.Get("Debug.KeySetTo", new { option = str }) + obj.ToString() + ".");
            }
            else if(str == "LastFridgeTabButton"){
                DebugLog(Helper.Translation.Get("Debug.KeySetTo", new { option = str }) + obj.ToString() + ".");
            }
        }

        public DateTime GetBuildDate(Assembly assembly){

            const string BuildVersionMetadataPrefix = "+build";
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null){
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0){
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)){
                        return result;
                    }
                }
            }
            return default;
        }

        //* Prints message in console log with given log level if enabled.
        public static void DebugLog(string message, LogLevel level = LogLevel.Debug)
        {
            if(level != LogLevel.Debug){
                _instance.Monitor.Log(message, level);
            }else{
            if (_instance.Config.ShowDebugMessages){
                _instance.Monitor.Log(message, level);
            }
            }
        }
    }
}
