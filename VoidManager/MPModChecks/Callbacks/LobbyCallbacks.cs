using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Reflection;
using UI.Matchmaking;

namespace VoidManager.MPModChecks.Callbacks
{
    class LobbyCallbacks : ILobbyCallbacks //Exists for ClientSide mod check in lobby join menus.
    {
        internal static LobbyCallbacks Instance;

        public Hashtable SelectedRoomProperties; 

        static FieldInfo MatchlistFO = AccessTools.Field(typeof(MatchmakingHandler), "matchList");

        public void OnJoinedLobby()
        {
        }

        public void OnLeftLobby()
        {
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            string SelectedRoomID = ((MatchmakingList)MatchlistFO.GetValue(MatchmakingHandler.Instance)).GetSelectedRoom().RoomId;
            foreach (RoomInfo roomInfo in roomList)
            {
                if(roomInfo.Name == SelectedRoomID)
                {
                    SelectedRoomProperties = roomInfo.CustomProperties;
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MatchmakingHandler), "StartRetrievingRooms")]
    class SubscribePatch
    {
        [HarmonyPostfix]
        static void AddTarget()
        {
            LobbyCallbacks.Instance = new LobbyCallbacks();
            PhotonNetwork.AddCallbackTarget(LobbyCallbacks.Instance);
        }
    }
    [HarmonyPatch(typeof(MatchmakingHandler), "StartRetrievingRooms")]
    class UnSubscribePatch
    {
        [HarmonyPostfix]
        static void RemoveTarget()
        {
            PhotonNetwork.RemoveCallbackTarget(LobbyCallbacks.Instance);
            LobbyCallbacks.Instance = null;
        }
    }
}
