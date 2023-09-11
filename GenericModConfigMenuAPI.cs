using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ExpandedFridge
{
    public interface GenericModConfigMenuAPI
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
        void AddKeybind(IManifest mod, Func<SButton> getValue, Action<SButton> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
        void OnFieldChanged(IManifest mod, Action<string, object> onChange);
    }
}
