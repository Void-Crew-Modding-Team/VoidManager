using CG.Profile;
using HarmonyLib;
using VoidManager.MPModChecks;

namespace VoidManager
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        [HarmonyPostfix]
        public static void PostAwakeInit()
        {
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovering mods . . .");
            PluginHandler.DiscoverPlugins();
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] . . . Discovery finished");

            new MPModCheckManager();

            new ModMessage.RecieveModMessage();
        }
    }
}
