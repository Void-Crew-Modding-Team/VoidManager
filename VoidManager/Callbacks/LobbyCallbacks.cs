using HarmonyLib;
using Photon.Realtime;
using System.Collections.Generic;
using UI.Core;
using System.Linq;
using UI.Matchmaking;

namespace VoidManager.Callbacks
{
    class LobbyCallbacks : ILobbyCallbacks //Exists for ClientSide mod check in lobby join menus.
    {
        public static LoadBalancingClient MatchmakingLoadBalancingClient = null;
        public static LobbyCallbacks Instance;
        public MatchmakingTerminal ActiveTerminal;
        public TabsRibbon Tabs;
        public List<RoomInfo> RoomList = new List<RoomInfo>();

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
            foreach (RoomInfo roomInfo in roomList)
            {
                int i = 0;
                RoomInfo foundRoom = null;
                for(; i < RoomList.Count; i++)
                {
                    if (RoomList[i].Name == roomInfo.Name)
                    {
                        foundRoom = RoomList[i];
                        break;
                    }
                }

                if (foundRoom != null)
                {
                    if (roomInfo.RemovedFromList)
                    {
                        RoomList.Remove(foundRoom);
                    }
                    else
                    {
                        RoomList[i] = roomInfo;
                    }
                }
                else if (!roomInfo.RemovedFromList)
                {
                    RoomList.Add(roomInfo);
                }
            }
        }
    }
    [HarmonyPatch(typeof(MatchmakingHandler), "Awake")]
    class ClientGrabPatch
    {
        [HarmonyPostfix]
        static void GrabClient(LoadBalancingClient ___client)
        {
            LobbyCallbacks.MatchmakingLoadBalancingClient = ___client;
        }
    }

    [HarmonyPatch(typeof(MatchmakingTerminal), "PanelActiveChange")] //ActiveTerminal Somehow doesn't get updated. Reproduce?: join game via terminal, go back to lobby and join via different terminal?
    class TerminalEnablePatch
    {
        [HarmonyPostfix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "Irrelevant")]
        static void PanelChange(bool isActive, MatchmakingTerminal __instance, TabsRibbon ___tabs)
        {
            BepinPlugin.Log.LogInfo("Setting Active Terminal " + isActive.ToString());
            if (isActive)
            {
                LobbyCallbacks.Instance = new LobbyCallbacks();
                LobbyCallbacks.MatchmakingLoadBalancingClient.AddCallbackTarget(LobbyCallbacks.Instance);
                LobbyCallbacks.Instance.ActiveTerminal = __instance;
                LobbyCallbacks.Instance.Tabs = ___tabs;
            }
            else
            {
                LobbyCallbacks.Instance.Tabs = null;
                LobbyCallbacks.Instance.ActiveTerminal = null;
                LobbyCallbacks.Instance.RoomList = null;
                LobbyCallbacks.MatchmakingLoadBalancingClient.RemoveCallbackTarget(LobbyCallbacks.Instance);
                LobbyCallbacks.Instance = null;
            }
        }
    }
}
