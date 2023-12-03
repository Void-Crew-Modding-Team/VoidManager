using CG;
using CG.Profile;
using CommandHandler.Chat.Router;
using CommandHandler.Utilities;
using HarmonyLib;

namespace CommandHandler
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        [HarmonyPostfix]
        public static void DiscoverCommandMods()
        {
            Logger.Info($"[{MyPluginInfo.PLUGIN_NAME}] Discovering mods w/Commands . . .", Logger.LogType.GameLog);
            Handler.DiscoverPlugins();
            Logger.Info($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {Handler.chatCommandCount} chat commands", Logger.LogType.GameLog);
        }
    }
}
