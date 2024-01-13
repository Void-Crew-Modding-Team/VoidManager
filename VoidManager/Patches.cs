using CG.Profile;
using HarmonyLib;
using VoidManager.Utilities;
using static VoidManager.Utilities.Logger;

namespace VoidManager
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        [HarmonyPostfix]
        public static void DiscoverCommandMods()
        {
            Logger.Info($"[{MyPluginInfo.PLUGIN_NAME}] Discovering mods . . .", LogType.GameLog);
            Mod.PluginHandler.DiscoverPlugins();
            Logger.Info($"[{MyPluginInfo.PLUGIN_NAME}] . . . Discovery finished", LogType.GameLog);
        }
    }
}
