using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VoidManager
{
    [BepInPlugin("Mest.VoidManager", "VoidManager", "0.0.1")]
    [BepInProcess("Void Crew.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin instance;
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        private void Awake()
        {
            instance = this;
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





