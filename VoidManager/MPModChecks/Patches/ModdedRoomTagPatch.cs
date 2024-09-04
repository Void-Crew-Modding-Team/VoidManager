using HarmonyLib;
using Photon.Realtime;

namespace VoidManager.MPModChecks.Patches
{
    //Basically the latest point to find room name and photonRoomInfo in the same method.
    [HarmonyPatch(typeof(MatchmakingRoom), "FromRoomInfo")]
    class ModdedRoomTagPatch
    {
        public const string ModSessionString = "<b><color=#ff8000>[mod_session] </color></b>";
        public const int MSSLength = 44;
        public const string ModLocalString = "<b><color=#FFD700>[mod_local] </color></b>";
        public const int MSLLength = 42;
        public const string ModsRequiredString = "[Mods Required]";
        public static readonly string ModsRequiredLobbyListString = $"{ModSessionString} {ModsRequiredString}";

        const string RedM = "<b><color=red>[M]</color></b> ";
        const string YellowM = "<b><color=yellow>[M]</color></b> ";
        const string GreenM = "<b><color=#249d48>[M]</color></b> ";

        [HarmonyPostfix]
        static void ModdedRoomPatch(RoomInfo pRoom, MatchmakingRoom __result)
        {
            if (MPModCheckManager.RoomIsModded(pRoom))
            {
                if (__result.RoomName.StartsWith(ModsRequiredLobbyListString, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    __result.RoomName = __result.RoomName.Replace(ModsRequiredLobbyListString, RedM);
                }
                else if (__result.RoomName.StartsWith(ModSessionString, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    __result.RoomName = __result.RoomName.Replace(ModSessionString, YellowM);
                }
                else if (__result.RoomName.StartsWith(ModLocalString, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    __result.RoomName = __result.RoomName.Replace(ModLocalString, GreenM);
                }
            }
        }
    }
}
