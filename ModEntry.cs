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
            
            Config = helper.ReadConfig<ModConfig>();
            Manager = new Manager(this);
        }

        //* Prints message in console log with given log level.
        public static void DebugLog(string message, LogLevel level = LogLevel.Trace)
        {
            if (_instanceInitiated)
                _instance.Monitor.Log(message, level);
        }
    }
}
