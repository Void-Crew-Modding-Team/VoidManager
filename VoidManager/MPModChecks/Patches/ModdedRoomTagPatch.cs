using HarmonyLib;
using UI.Matchmaking;
using UnityEngine.UIElements;

namespace VoidManager.MPModChecks.Patches
{
    //Basically the latest point to find room name and photonRoomInfo in the same method.
    [HarmonyPatch(typeof(MatchmakingList), "BindItem")]
    class ModdedRoomTagPatch
    {
        public const string ModSessionString = "<b><color=#ff8000>[mod_session] </color></b>";
        public const int MSSLength = 44;
        public const string ModLocalString = "<b><color=#FFD700>[mod_local] </color></b>";
        public const int MSLLength = 42;
        public const string ModsRequiredString = "[Mods Required]";
        public static readonly string ModsRequiredLobbyListString = $"{ModSessionString}{ModsRequiredString}";

        const string RedM = $"<b><color={CustomGUI.GUIMain.AllMPTypeColorCode}>[M]</color></b> ";
        const string YellowM = $"<b><color={CustomGUI.GUIMain.SessionMPTypeColorCode}>[M]</color></b> ";
        const string GreenM = $"<b><color={CustomGUI.GUIMain.ClientMPTypeColorCode}>[M]</color></b> ";

        [HarmonyPostfix]
        static void ModdedRoomPatch(VisualElement item, MatchmakingRoom room)
        {
            TextElement textElement = item.Q<TextElement>("RoomName", default, default);
            if (textElement.text.StartsWith(ModsRequiredLobbyListString, System.StringComparison.CurrentCultureIgnoreCase))
            {
                textElement.text = textElement.text.Replace(ModsRequiredLobbyListString, RedM);
            }
            else if (textElement.text.StartsWith(ModSessionString, System.StringComparison.CurrentCultureIgnoreCase))
            {
                textElement.text = textElement.text.Replace(ModSessionString, YellowM);
            }
            else if (textElement.text.StartsWith(ModLocalString, System.StringComparison.CurrentCultureIgnoreCase))
            {
                textElement.text = textElement.text.Replace(ModLocalString, GreenM);
            }
        }
    }
}
