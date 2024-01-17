﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VoidManager.Mod;

namespace VoidManager
{
    [ManagerPlugin("Mest", "VoidManager", MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        private void Awake()
        {
            Harmony.PatchAll();
            Log = Logger;
            Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Initialized.");
            
        }
    }
}
/* Public Mod Implementation
 * Workout how to identify players
 *  (Custom dictionary containing players as they join?)
 * Utility methods
 *  - Mod Messaging?
*/





