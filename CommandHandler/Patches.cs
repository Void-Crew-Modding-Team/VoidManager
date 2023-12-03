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
    [HarmonyPatch(typeof(TextChatVE), "GetMessage")]
    internal class CommandDetectPatch
    {
        [HarmonyPostfix]
        public static void DiscoverChatCommand(ref string __result)
        {
            if (!__result.StartsWith("/")) return;
            __result = __result.Substring(1);
            string alias = __result.Split(' ')[0];
            Handler.ExecuteCommandFromAlias(alias, __result.Substring(alias.Length));
            __result = "";
        }
    }
}
