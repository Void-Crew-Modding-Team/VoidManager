using Client.Utils;
using HarmonyLib;
using Photon.Realtime;
using System.Collections.Generic;
using UI.Matchmaking;

namespace VoidManager.Callbacks
{
    class LobbyCallbacks : ILobbyCallbacks //Exists for ClientSide mod check in lobby join menus.
    {
        public static LoadBalancingClient MatchmakingLoadBalancingClient = null;
        public static LobbyCallbacks Instance;
        public MatchmakingTerminal ActiveTerminal;
        public List<RoomInfo> RoomList;

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
            BepinPlugin.Log.LogInfo("Copying room");
            RoomList = roomList.DeepCopy();
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

    [HarmonyPatch(typeof(MatchmakingTerminal), "PanelActiveChange")]
    class TerminalEnablePatch
    {
        [HarmonyPostfix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "Irrelevant")]
        static void PanelChange(bool isActive, MatchmakingTerminal __instance)
        {
            BepinPlugin.Log.LogInfo("Setting Active Terminal " + isActive.ToString());
            if (isActive)
            {
                LobbyCallbacks.Instance = new LobbyCallbacks();
                LobbyCallbacks.MatchmakingLoadBalancingClient.AddCallbackTarget(LobbyCallbacks.Instance);
                LobbyCallbacks.Instance.ActiveTerminal = __instance;
            }
            else
            {
                LobbyCallbacks.Instance.ActiveTerminal = null;
                LobbyCallbacks.Instance.RoomList = null;
                LobbyCallbacks.MatchmakingLoadBalancingClient.RemoveCallbackTarget(LobbyCallbacks.Instance);
                LobbyCallbacks.Instance = null;
            }
        }
    }
}
