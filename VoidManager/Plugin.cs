using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using VoidManager.ModMessage;

namespace VoidManager
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly Dictionary<string, VoidCrewMod> activeMods;
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log = new ManualLogSource(MyPluginInfo.PLUGIN_NAME);
        private void Awake()
        {
            Harmony.PatchAll();
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Initialized.");
            Log = Logger;
        }
    }
    public abstract class VoidCrewMod
    {
        internal string HarmonyIdentifier()
        {
            throw new NotImplementedException();
        }
    }
}
/* Public Mod Implementation
 * Workout how to identify players
 *  (Custom dictionary containing players as they join?)
 * Utility methods
 *  - Mod Messaging?
*/





