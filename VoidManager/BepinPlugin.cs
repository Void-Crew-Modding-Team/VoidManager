using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
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

            ModInfoTextAnchor = Config.Bind("General", "ModInfoTextAnchor", TextAnchor.UpperLeft, "");
            DebugMode = Config.Bind("General", "DebugMode", false, "");

            MenuHeight = Config.Bind("Menu", "Height", .40f, "");
            MenuWidth = Config.Bind("Menu", "Width", .40f, "");
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
        }
    }
}