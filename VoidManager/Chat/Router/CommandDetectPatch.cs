using Gameplay.Chat;
using HarmonyLib;
using System.Linq;
using UI.Chat;
using VoidManager.Utilities;

namespace VoidManager.Chat.Router
{
    [HarmonyPatch(typeof(TextChatVE), "GetMessage")]
    internal class ChatCommandDetectPatch
    { // Local player chat command
        [HarmonyPostfix]
        public static void DiscoverChatCommand(ref string __result)
        {
            if (!__result.StartsWith("/")) return;
            __result = __result.Substring(1);
            string alias = __result.Split(' ')[0];
            string arguments = __result.Substring(alias.Length + (__result.Split(' ').Count() == 1 ? 0 : 1));
            CommandHandler.ExecuteCommandFromAlias(alias, arguments);
            __result = "";
        }
    }

    [HarmonyPatch(typeof(TextChat), "IncomingMessage")]
    internal class PublicCommandDetectPatch
    { // Other player chat command
        [HarmonyPostfix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "Not relevant")]
        public static void DiscoverPublicCommand(Photon.Realtime.Player p, string channelTextMessage)
        {
            if (!channelTextMessage.StartsWith("!")) return;
            channelTextMessage = channelTextMessage.Substring(1);
            string alias = channelTextMessage.Split(' ')[0];
            CG.Game.Player.Player Player = Game.GetPlayerByName(p.NickName);
            string arguments = channelTextMessage.Substring(alias.Length + (channelTextMessage.Split(' ').Count() == 1 ? 0 : 1));
            Plugin.Log.LogInfo($"'!{alias} {arguments}' attempted by {p.NickName}");
            CommandHandler.ExecuteCommandFromAlias(alias, arguments, true, Game.GetIDFromPlayer(Player));
        }
    }
}
