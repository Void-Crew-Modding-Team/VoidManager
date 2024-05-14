using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static VoidManager.BepinPlugin.Bindings;

namespace VoidManager
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    public class BepinPlugin : BaseUnityPlugin
    {
        internal static BepinPlugin instance;
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        private void Awake()
        {
            instance = this;
            Log = Logger;

            Harmony.PatchAll();
            Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Initialized.");

            
            DebugMode = Config.Bind("General", "DebugMode", false, "");

            UnspecifiedModListOverride = Config.Bind("General", "Unspecified Mod Overrides", string.Empty, "Insert mods (not configured for VoidManager) for which you would like to override the MPType. Format: 'ModNameOrGUID:ClientOrAll', delineated by ','. Ex: VoidManager:all,Better Scoop:Client \n ModName/GUID can be gathered from log files and F5 menu.");


            ModInfoTextAnchor = Config.Bind("Menu", "ModInfoTextAnchor", TextAnchor.UpperLeft, "");

            MenuHeight = Config.Bind("Menu", "Height", .50f, "");
            MenuWidth = Config.Bind("Menu", "Width", .50f, "");
            MenuListWidth = Config.Bind("Menu", "List Width", .30f, "");
            MenuUnlockCursor = Config.Bind("Menu", "Unlock Cursor", true, "");

            MenuOpenKeybind = Config.Bind("Menu", "Open Keybind", OpenMenu, "");

            TrustMPTypeUnspecified = Config.Bind("Multiplayer", "TrustMPTypeUnspecified", false, "");

            
        }
        internal class Bindings
        {
            internal static ConfigEntry<UnityEngine.TextAnchor> ModInfoTextAnchor;
            internal static ConfigEntry<bool> DebugMode;

            public static void SetDefault()
            {
                ModInfoTextAnchor.Value = TextAnchor.UpperLeft;
            }

            internal static ConfigEntry<float> MenuHeight;
            internal static ConfigEntry<float> MenuWidth;
            internal static ConfigEntry<float> MenuListWidth;
            internal static ConfigEntry<bool> MenuUnlockCursor;

            internal static ConfigEntry<KeyboardShortcut> MenuOpenKeybind;
            internal static KeyboardShortcut OpenMenu = new KeyboardShortcut(KeyCode.F5);

            internal static ConfigEntry<bool> TrustMPTypeUnspecified;

            internal static ConfigEntry<string> UnspecifiedModListOverride;
            internal static Dictionary<string, MPModChecks.MultiplayerType> ModOverrideDictionary;
            internal static void LoadModListOverride()
            {
                ModOverrideDictionary = new Dictionary<string, MPModChecks.MultiplayerType>();
                if (UnspecifiedModListOverride.Value == string.Empty)
                    return;
                string[] inputs = UnspecifiedModListOverride.Value.Split(',');
                foreach(string value in inputs)
                {
                    if(value.EndsWith(":all", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ModOverrideDictionary.Add(value.Substring(0, value.Length - 4), MPModChecks.MultiplayerType.All);
                    }
                    else if(value.EndsWith(":client", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ModOverrideDictionary.Add(value.Substring(0, value.Length - 7), MPModChecks.MultiplayerType.Client);
                    }
                    else if (value.EndsWith(":h", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ModOverrideDictionary.Add(value.Substring(0, value.Length - 2), MPModChecks.MultiplayerType.Hidden);
                    }
                    else
                    {
                        Log.LogError($"Unspecified Mod Override - '{value}' is not a valid input.");
                    }
                }
            }
        }
    }
}