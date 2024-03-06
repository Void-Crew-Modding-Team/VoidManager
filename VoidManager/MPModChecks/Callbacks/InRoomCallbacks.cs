using CG.GameLoopStateMachine.GameStates;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using System.Collections.Generic;
using ToolClasses;

namespace VoidManager.MPModChecks.Callbacks
{
    class RoomCallbacks : IInRoomCallbacks, IMatchmakingCallbacks, IOnEventCallback
    {
        public RoomCallbacks()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == MPModCheckManager.PlayerMPUserDataEventCode)
            {
                Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(photonEvent.Sender);

                if (sender.IsLocal)
                {
                    Plugin.Log.LogWarning("Recieved data from self. Ignoring data.");
                    return;
                }

                object[] data = (object[])photonEvent.CustomData;
                if ((bool)data[0])//Hashfull vs Hashless marker
                {
                    MPModCheckManager.Instance.AddNetworkedPeerMods(sender, MPModCheckManager.DeserializeHashfullMPUserData( (byte[])data[1] ));
                }
                else if (PhotonNetwork.IsMasterClient) //Data is hashless but recieving player is host. Data recieved should be hashfull when sent to host. Also no point turning down hashfull data from other clients when local is client.
                {
                    Plugin.Log.LogWarning($"Recieved hashless data from {sender.NickName}, but expecting hashfull data. Data will not be added.");
                    return;
                }
                else
                {
                    MPModCheckManager.Instance.AddNetworkedPeerMods(sender, MPModCheckManager.DeserializeHashlessMPUserData( (byte[])data[1] ));
                }
            }
        }

        public void OnJoinedRoom()
        {
            if(!MPModCheckManager.Instance.ModChecksClientside(PhotonNetwork.CurrentRoom.CustomProperties))
            {
                Plugin.Log.LogInfo("Disconnecting from Room");
                SteamMatchmaking.LeaveLobby(Singleton<SteamService>.Instance.GetCurrentLobbyID());
                PhotonNetwork.LeaveRoom(false);

                return;
            }
            //Sends to host twice. Should fixme
            MPModCheckManager.Instance.SendModlistToHost();
            MPModCheckManager.Instance.SendModListToOthers();

            //Add host mod list to local cache.
            if (!PhotonNetwork.IsMasterClient)
            {
                MPModCheckManager.Instance.AddNetworkedPeerMods(PhotonNetwork.MasterClient, MPModCheckManager.Instance.GetHostModList());
            }
        }

        public void OnLeftRoom()
        {
            MPModCheckManager.Instance.ClearAllNetworkedPeerMods();
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                MPModCheckManager.Instance.UpdateLobbyProperties();
            }
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PunSingleton<PhotonService>.Instance.StartCoroutine(MPModCheckManager.PlayerJoinedChecks(newPlayer)); //Plugin is not a valid monobehaviour.
            }
            else
            {
                MPModCheckManager.Instance.SendModlistToClient(newPlayer);
            }
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            MPModCheckManager.Instance.RemoveNetworkedPeerMods(otherPlayer);
        }


        //Not Utillized.
        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
        }
    }

    //Initialize Room Callbacks class.
    [HarmonyPatch(typeof(GSMainMenu), "OnEnter")]
    class InitPatch
    {
        static bool RoomCallbacksInitialized = false;

        [HarmonyPostfix]
        static void InitRoomCallbacks()
        {
            if (RoomCallbacksInitialized)
            {
                return;
            }
            RoomCallbacksInitialized = true;
            MPModCheckManager.RoomCallbacksClass = new RoomCallbacks();
        }
    }
}
