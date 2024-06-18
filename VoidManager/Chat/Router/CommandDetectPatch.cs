using CG.Input;
using Gameplay.Chat;
using HarmonyLib;
using System.Linq;
using UI.Chat;
using VoidManager.Utilities;

namespace VoidManager.Chat.Router
{
    [HarmonyPatch(typeof(TextChatVE))]
    internal class ChatCommandDetectPatch
    { // Local player chat command
        [HarmonyPostfix]
        [HarmonyPatch("GetMessage")]
        static void DiscoverChatCommand(ref string __result)
        {
            if (!__result.StartsWith("/")) return;
            __result = __result.Substring(1);
            string alias = __result.Split(' ')[0];
            string arguments = __result.Substring(alias.Length + (__result.Split(' ').Count() == 1 ? 0 : 1));
            CommandHandler.ExecuteCommandFromAlias(alias, arguments);
            __result = "";
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowInput")]
        static void ShowChatWindow()
        {
            CursorUtility.ShowCursor(ChatCursorSource.Instance, true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("HideInput")]
        static void HideChatWindow()
        {
            CursorUtility.ShowCursor(ChatCursorSource.Instance, false);
        }
    }

    [HarmonyPatch(typeof(TextChat), "IncomingMessage")]
    internal class PublicCommandDetectPatch
    { // Other player chat command
        [HarmonyPostfix]
        static void DiscoverPublicCommand(string cloudID, string channelTextMessage)
        {
            Photon.Realtime.Player p = VoipService.CloudIDToPlayer(cloudID);
            if (!channelTextMessage.StartsWith("!")) return;
            channelTextMessage = channelTextMessage.Substring(1);
            string alias = channelTextMessage.Split(' ')[0];
            CG.Game.Player.Player Player = Game.GetPlayerByName(p.NickName);
            string arguments = channelTextMessage.Substring(alias.Length + (channelTextMessage.Split(' ').Count() == 1 ? 0 : 1));
            BepinPlugin.Log.LogInfo($"'!{alias} {arguments}' attempted by {p.NickName}");
            CommandHandler.ExecuteCommandFromAlias(alias, arguments, true, Game.GetIDFromPlayer(Player));
        }
    }

    internal class ChatCursorSource : IShowCursorSource
    {
        internal static readonly ChatCursorSource Instance = new();
        private ChatCursorSource() { }
    }
}
