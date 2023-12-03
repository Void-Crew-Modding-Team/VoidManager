using CG;
using CG.Profile;
using Gameplay.Chat;
using HarmonyLib;
using UI.Chat;

namespace CommandHandler
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        [HarmonyPostfix]
        public static void DiscoverCommandMods()
        {
            Debug.Log($"[{MyPluginInfo.PLUGIN_NAME}] Discovering mods w/Commands . . .");
            Handler.DiscoverPlugins();
            Debug.Log($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {Handler.chatCommandCount} chat commands");
        }
    }
}
