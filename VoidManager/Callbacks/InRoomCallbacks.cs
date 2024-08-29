using CG.GameLoopStateMachine;
using CG.GameLoopStateMachine.GameStates;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolClasses;
using VoidManager.ModMessages;
using VoidManager.MPModChecks;

namespace VoidManager.Callbacks
{
    class InRoomCallbacks : IInRoomCallbacks, IMatchmakingCallbacks, IOnEventCallback
    {
        public InRoomCallbacks()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        internal const byte PlayerMPUserDataEventCode = 99;
        internal const byte ModMessageEventCode = 98;
        internal const byte KickMessageEventCode = 97;
        internal const string RoomModsPropertyKey = "Mods";
        internal const string OfficalModdedPropertyKey = "R_Mod";

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == PlayerMPUserDataEventCode)//MPModChecksEvents
            {
                Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(photonEvent.Sender);

                if (sender.IsLocal)
                {
                    BepinPlugin.Log.LogWarning("Recieved data from self. Ignoring data.");
                    return;
                }

                object[] data = (object[])photonEvent.CustomData;
                if ((bool)data[0])//Hashfull vs Hashless marker
                {
                    MPModCheckManager.Instance.AddNetworkedPeerMods(sender, MPModCheckManager.DeserializeHashfullMPUserData((byte[])data[1]));
                }
                else if (PhotonNetwork.IsMasterClient) //Data is hashless but recieving player is host. Data recieved should be hashfull when sent to host. Also no point turning down hashfull data from other clients when local is client.
                {
                    BepinPlugin.Log.LogWarning($"Recieved hashless data from {sender.NickName}, but expecting hashfull data. Data will not be added.");
                    return;
                }
                else
                {
                    MPModCheckManager.Instance.AddNetworkedPeerMods(sender, MPModCheckManager.DeserializeHashlessMPUserData((byte[])data[1]));
                }
            }
            else if (photonEvent.Code == ModMessageEventCode)//ModMessagesEvents
            {
                Player Sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(photonEvent.Sender);
                object[] args = (object[])photonEvent.CustomData;

                //Fail in event args cannot possibly work for a proper ModMessage
                if (args == null || args.Length < 2)
                {
                    BepinPlugin.Log.LogInfo($"Recieved Invalid ModMessage from {Sender.NickName ?? "N/A"}");
                    return;
                }


                if (ModMessageHandler.modMessageHandlers.TryGetValue($"{args[0]}#{args[1]}", out ModMessage modMessage))
                {
                    modMessage.Handle(args.Length > 2 ? args.Skip(2).ToArray() : null, Sender);
                    return;
                }

                //Fail in event targetted ModMessage was not found.
                BepinPlugin.Log.LogInfo($"Recieved Unrecognised ModMessage ({args[0] ?? "N/A"}#{args[1] ?? "N/A"}) from {Sender.NickName ?? "N/A"}");
            }
            else if (photonEvent.Code == KickMessageEventCode)
            {
                BepinPlugin.Log.LogMessage("Recieved Kick Message.");
                try
                {
                    Player sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);//Not sure why others are using Networking client. If this ends up working right, the others should be replaced. TestMe
                    if (!sender.IsMasterClient)
                    {
                        BepinPlugin.Log.LogInfo("Recieved Kick Message from non-host. Sender: " + sender.NickName);
                        return;
                    }
                    object[] EventData = (object[])photonEvent.CustomData;
                    if (EventData.Length >= 2)
                    {
                        KickMessagePatches.KickTitle = EventData[0].ToString();
                        KickMessagePatches.KickMessage = EventData[1].ToString();
                    }
                }
                catch (Exception ex)
                {
                    BepinPlugin.Log.LogError("Recieved Photon Event with InfoMessage code, but could not parse data.\n" + ex);
                }
            }
            else if (photonEvent.Code == 203)
            {
                if (Singleton<GameStateMachine>.Instance.CurrentState is GSSpawn)//fixes bug with vanilla getting kicked too early. Treated like a normal photon disconnect, but the error code will be the input value.
                {
                    BepinPlugin.Log.LogInfo("Kicked while in GSSpawn State.");
                    Singleton<GameStateMachine>.Instance.ChangeState<GSPhotonDisconnected>();
                }
            }
        }

        public void OnJoinedRoom()
        {
            if (!MPModCheckManager.Instance.ModChecksClientside(PhotonNetwork.CurrentRoom.CustomProperties))
            {
                BepinPlugin.Log.LogInfo("Disconnecting from Room");
                GameStateMachine.Instance.ChangeState<GSPhotonDisconnected>();
                return;
            }
            //Sends to host twice. Should fixme
            MPModCheckManager.Instance.SendModlistToHost();
            MPModCheckManager.Instance.SendModListToOthers();

            //Add host mod list to local cache.
            if (!PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomModsPropertyKey))
            {
                MPModCheckManager.Instance.AddNetworkedPeerMods(PhotonNetwork.MasterClient, MPModCheckManager.Instance.GetHostModList());
            }

            //Above controls whether a game is joined, so it is better to let it run first.
            Events.Instance.OnJoinedRoom();
        }

        public void OnLeftRoom()
        {
            MPModCheckManager.Instance.ClearAllNetworkedPeerMods();
            Events.Instance.CallOnLeftRoom();
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                MPModCheckManager.Instance.UpdateLobbyProperties();
            }

            Events.Instance.CallOnMasterClientSwitched(newMasterClient);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            MPModCheckManager.Instance.PlayerJoined(newPlayer);
            Events.Instance.OnPlayerEnteredRoom(newPlayer);
        }

        public void OnPlayerLeftRoom(Player leavingPlayer)
        {
            Events.Instance.OnPlayerLeftRoom(leavingPlayer);
            MPModCheckManager.Instance.RemoveNetworkedPeerMods(leavingPlayer);
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
}
