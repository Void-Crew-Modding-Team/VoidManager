using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI.Matchmaking;

namespace VoidManager.Patches
{
    //Fixes a bug that causes the currently selected game to be deselected when the game list refreshes
    [HarmonyPatch(typeof(MatchmakingList), "RoomsUpdated")]
    internal class MatchmakingListSelectedPatch
    {
        private static readonly FieldInfo scrollListField = AccessTools.Field(typeof(MatchmakingList), "scrollList");

        static void Prefix(MatchmakingList __instance, out MatchmakingRoom __state)
        {
            object scrollList = scrollListField.GetValue(__instance);
            __state = (MatchmakingRoom)AccessTools.Method(scrollList.GetType(), "get_SelectedItem").Invoke(scrollList, null);
        }

        static void Postfix(MatchmakingList __instance, MatchmakingRoom __state)
        {
            if (__state == null) return;
            object scrollList = scrollListField.GetValue(__instance);
            List<MatchmakingRoom> rooms = (List<MatchmakingRoom>)AccessTools.Field(scrollList.GetType(), "source").GetValue(scrollList);
            MatchmakingRoom room = rooms.FirstOrDefault(room => room.RoomName == __state.RoomName);
            if (room == null) return;
            AccessTools.Method(scrollList.GetType(), "SelectWithoutNotify").Invoke(scrollList, new object[] { room });
        }
    }
}
