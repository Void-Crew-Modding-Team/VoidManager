using Gameplay.Chat;
using HarmonyLib;
using UI.Chat;
using VivoxUnity;

namespace CommandHandler.Chat.Commands
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
            Handler.ExecuteCommandFromAlias(alias, __result.Substring(alias.Length));
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
            if (!result.StartsWith("/")) return;
            result = result.Substring(1);
            string alias = result.Split(' ')[0];
            Handler.ExecutePublicCommandFromAlias(alias, result.Substring(alias.Length));
        }
    }
}
