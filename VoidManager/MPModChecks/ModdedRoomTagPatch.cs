using HarmonyLib;
using Photon.Realtime;
using UI.Matchmaking;
using UnityEngine.UIElements;

namespace VoidManager.MPModChecks
{
    //Basically the latest point to find room name and photonRoomInfo in the same method. It is possible to iterate through a list of RoomInfos and compare against
    //the matchmaking room, but that's slower than using two patches.
    [HarmonyPatch(typeof(MatchmakingHandler), "ConvertRoom")]
    class ModdedRoomTagPatch
    {
        [HarmonyPostfix]
        static void ModdedRoomPatch(RoomInfo pRoom, MatchmakingRoom __result)
        {
            if(MPModCheckManager.RoomIsModded(pRoom))
            {
                if (__result.RoomName.StartsWith("[Mods Required]", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    __result.RoomName = __result.RoomName.Remove(0, 15);
                    __result.RoomName = "<color=red>[M]</color> " + __result.RoomName;
                    return;
                }

                if (__result.RoomName.StartsWith("[Modded] ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    __result.RoomName = __result.RoomName.Remove(0, 9);
                }
                __result.RoomName = "<color=yellow>[M]</color> " + __result.RoomName;
            }
        }
    }
    [HarmonyPatch(typeof(MatchmakingList), "BindItem")]
    class ModdedRoomTagPatch2
    {
        [HarmonyPostfix]
        static void RoomNameRichTextPatch(VisualElement item, MatchmakingRoom room)
        {
            ((TextElement)item.Q("RoomName")).enableRichText = true;
        }
    }
}
