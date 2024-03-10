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
            Plugin.Log.LogInfo($"- - - Void Manager Initialization - - -");
            PluginHandler.DiscoverPlugins();
            Plugin.Log.LogInfo($"- - - - - - - - - - - - - - - - - - - -");

            new MPModCheckManager();
        }
    }
}
