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
            Logger.Info($"[{MyPluginInfo.PLUGIN_NAME}] Discovering mods w/Commands . . .", LogType.GameLog);
            Chat.Router.CommandHandler.DiscoverPlugins();
            Logger.Info($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {Chat.Router.CommandHandler.chatCommandCount} chat commands", LogType.GameLog);
        }
    }
}
