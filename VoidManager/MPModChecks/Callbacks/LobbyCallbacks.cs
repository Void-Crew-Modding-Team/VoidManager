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
        internal static MatchmakingTerminal ActiveTerminal;

        public Hashtable SelectedRoomProperties; 

        static FieldInfo MatchlistFO = AccessTools.Field(typeof(MatchmakingTerminal), "matchList");

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
            if (ActiveTerminal == null)
            {
                return;
            }

            string SelectedRoomID = ((MatchmakingList)MatchlistFO.GetValue(ActiveTerminal)).GetSelectedRoom().RoomId;
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
    [HarmonyPatch(typeof(MatchmakingTerminal), "OnEnable")]
    class TerminalEnablePatch
    { 
        static void Postfix(MatchmakingTerminal __instance)
        {
            LobbyCallbacks.ActiveTerminal = __instance;
        }
    }

    [HarmonyPatch(typeof(MatchmakingTerminal), "OnDisable")]
    class TerminalDisablePatch
    {
        static void Postfix()
        {
            LobbyCallbacks.ActiveTerminal = null;
        }
    }
}
