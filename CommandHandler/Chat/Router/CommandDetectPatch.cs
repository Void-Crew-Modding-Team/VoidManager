using Gameplay.Chat;
using HarmonyLib;
using System.Linq;
using UI.Chat;
using VivoxUnity;
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
        public static void DiscoverPublicCommand(string displayName, IChannelTextMessage channelTextMessage)
        {
            string result = channelTextMessage.Message;
            if (!result.StartsWith("!")) return;
            result = result.Substring(1);
            string alias = result.Split(' ')[0];
            CG.Game.Player.Player Player = Game.GetPlayerByName(channelTextMessage.Sender.DisplayName);
            string arguments = result.Substring(alias.Length + (result.Split(' ').Count() == 1 ? 0 : 1));
            Logger.Info($"'!{alias} {arguments}' attempted by {displayName}");
            CommandHandler.ExecuteCommandFromAlias(alias, arguments, true, Game.GetIDFromPlayer(Player));
        }
    }
}
