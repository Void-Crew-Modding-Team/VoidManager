using BepInEx.Logging;
using CG.Profile;
using HarmonyLib;

namespace VoidManager
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        [HarmonyPostfix]
        public static void DiscoverCommandMods()
        {
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovering mods . . .");
            Mod.PluginHandler.DiscoverPlugins();
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] . . . Discovery finished");
        }
    }
}
