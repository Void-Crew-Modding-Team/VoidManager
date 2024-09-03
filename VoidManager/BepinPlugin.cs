using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using VoidManager.Chat.Additions;
using VoidManager.Chat.Router;
using static VoidManager.BepinPlugin.Bindings;

namespace VoidManager
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.USERS_PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    public class BepinPlugin : BaseUnityPlugin
    {
        internal static BepinPlugin instance;
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "N/A")]
        private void Awake()
        {
            instance = this;
            Log = Logger;

            //Compliance
            ModdingUtils.SessionModdingType = ModdingType.mod_session;

            Harmony.PatchAll();
            Content.Craftables.Instance = new();
            Content.Unlocks.Instance = new();
            Events.Instance = new();

            
            DebugMode = Config.Bind("General", "DebugMode", false, "");

            UnspecifiedModListOverride = Config.Bind("General", "Unspecified Mod Overrides", string.Empty, $"Insert mods (not configured for {MyPluginInfo.USERS_PLUGIN_NAME}) for which you would like to override the MPType. \nAvailable MPTypes: client,host,all \nFormat: 'ModNameOrGUID:MPType', delineated by ','. \nEx: {MyPluginInfo.USERS_PLUGIN_NAME}:all,Better Scoop:Host \n ModName/GUID can be gathered from log files and F5 menu.");

            ModInfoTextAnchor = Config.Bind("Menu", "ModInfoTextAnchor", TextAnchor.UpperLeft, "");

            MenuHeight = Config.Bind("Menu", "Height", .50f, "");
            MenuWidth = Config.Bind("Menu", "Width", .50f, "");
            MenuListWidth = Config.Bind("Menu", "List Width", .30f, "");
            PlayerListWidth = Config.Bind("Menu", "Player List Width", .30f, "");
            MenuUnlockCursor = Config.Bind("Menu", "Unlock Cursor", true, "");

            MenuOpenKeybind = Config.Bind("Menu", "Open Keybind", OpenMenu, "");

            TrustMPTypeUnspecified = Config.Bind("Multiplayer", "TrustMPTypeUnspecified", true, "");

            //Fix chainloader getting deleted by GC?
            Chainloader.ManagerObject.hideFlags = HideFlags.HideAndDontSave;

            Events.Instance.ChatWindowOpened += ChatHistory.OnChatOpened;
            Events.Instance.ChatWindowOpened += CursorUnlock.OnChatOpened;
            Events.Instance.ChatWindowOpened += AutoComplete.OnChatOpened;
            Events.Instance.ChatWindowClosed += ChatHistory.OnChatClosed;
            Events.Instance.ChatWindowClosed += CursorUnlock.OnChatClosed;
            Events.Instance.ChatWindowClosed += AutoComplete.OnChatClosed;
            Events.Instance.LateUpdate += ChatHistory.Tick;
            Events.Instance.LateUpdate += AutoComplete.Tick;
            Events.Instance.JoinedRoom += PublicCommandHandler.RefreshPublicCommandCache;
            Events.Instance.ClientModlistRecieved += PublicCommandHandler.RefreshPublicCommandCache;
            Events.Instance.MasterClientSwitched += PublicCommandHandler.RefreshPublicCommandCache;
            Events.Instance.PlayerEnteredRoom += AutoComplete.RefreshPlayerList;
            Events.Instance.PlayerLeftRoom += AutoComplete.RefreshPlayerList;
            Events.Instance.JoinedRoom += AutoComplete.RefreshPlayerList;

            Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Initialized.");
        }
        public class Bindings
        {
            public static ConfigEntry<UnityEngine.TextAnchor> ModInfoTextAnchor;
            public static ConfigEntry<bool> DebugMode;

            public static void SetDefault()
            {
                ModInfoTextAnchor.Value = TextAnchor.UpperLeft;
            }

            internal static ConfigEntry<float> MenuHeight;
            internal static ConfigEntry<float> MenuWidth;
            internal static ConfigEntry<float> MenuListWidth;
            internal static ConfigEntry<float> PlayerListWidth;
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
                    else if (value.EndsWith(":host", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ModOverrideDictionary.Add(value.Substring(0, value.Length - 5), MPModChecks.MultiplayerType.Host);
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member