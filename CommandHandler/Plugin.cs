using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CommandHandler
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Void Crew.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log = new ManualLogSource(MyPluginInfo.PLUGIN_NAME);
        private void Awake()
        {
            Harmony.PatchAll();
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Initialized.");
            Log = Logger;
        }
    }
}
/* Public Mod Implementation
 * Workout how to identify players
 *  (Custom dictionary containing players as they join?)
 * Utility methods
 *  - Mod Messaging?
*/





