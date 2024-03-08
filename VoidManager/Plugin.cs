using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VoidManager
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin instance;
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        private void Awake()
        {
            instance = this;
            Log = Logger;

            Harmony.PatchAll();
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





