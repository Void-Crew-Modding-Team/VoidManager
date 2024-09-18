using ExitGames.Client.Photon;
using Gameplay.NPC.AI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using VoidManager.ModMessages;
using VoidManager.MPModChecks;
using VoidManager.MPModChecks.Patches;

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
        internal const byte SessionEscalationEventCode = 96;
        internal const string RoomModsPropertyKey = "Mods";
        internal const string OfficalModdedPropertyKey = "R_Mod";
        internal const string OfficalRoomNamePropertyKey = "R_Na";
        internal const string RoomPlayerListPropertyKey = "plyrs";

        public void OnEvent(EventData photonEvent)
        {
            //Don't run code outside given cases, as this event is called by everything networked.
            switch(photonEvent.Code)
            {
                case PlayerMPUserDataEventCode: //MPModChecksEvents
                    {
                        Player Sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
                        if (Sender.IsLocal)
                        {
                            BepinPlugin.Log.LogWarning("Recieved data from self. Ignoring data.");
                            return;
                        }

                        object[] data = (object[])photonEvent.CustomData;
                        if ((bool)data[0])//Hashfull vs Hashless marker
                        {
                            NetworkedPeerManager.Instance.AddNetworkedPeerMods(Sender, NetworkedPeerManager.DeserializeHashfullMPUserData((byte[])data[1]));
                        }
                        else if (PhotonNetwork.IsMasterClient) //Data is hashless but recieving player is host. Data recieved should be hashfull when sent to host. Also no point turning down hashfull data from other clients when local is client.
                        {
                            BepinPlugin.Log.LogWarning($"Recieved hashless data from {Sender.NickName}, but expecting hashfull data. Data will not be added.");
                            return;
                        }
                        else
                        {
                            NetworkedPeerManager.Instance.AddNetworkedPeerMods(Sender, NetworkedPeerManager.DeserializeHashlessMPUserData((byte[])data[1]));
                        }
                        return;
                    }
                case ModMessageEventCode: //ModMessagesEvents
                    {
                        Player Sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
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
                        return;
                    }
                case KickMessageEventCode:
                    {
                        Player Sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
                        BepinPlugin.Log.LogMessage("Recieved Kick Message.");
                        try
                        {
                            if (!Sender.IsMasterClient)
                            {
                                BepinPlugin.Log.LogInfo("Recieved Kick Message from non-host. Sender: " + Sender.NickName);
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
                        return;
                    }
                case SessionEscalationEventCode:
                    {
                        Player Sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender);
                        if(!Sender.IsMasterClient)
                        {
                            BepinPlugin.Log.LogInfo("Recieved Escalation event from non-host. Sender: " + Sender.NickName);
                            return;
                        }
                        PluginHandler.InternalEscalateSession();
                        return;
                    }
            }
        }

        public void OnJoinedRoom()
        {
            Events.Instance.OnJoinedRoom();
        }

        public void OnLeftRoom()
        {
            Events.Instance.OnLeftRoom();
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            Events.Instance.OnMasterClientSwitched(newMasterClient);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Events.Instance.OnPlayerEnteredRoom(newPlayer);
        }

        public void OnPlayerLeftRoom(Player leavingPlayer)
        {
            Events.Instance.OnPlayerLeftRoom(leavingPlayer);
        }

        public void OnCreatedRoom()
        {
            Events.Instance.OnHostCreateRoom();
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

        public void OnCreateRoomFailed(short returnCode, string message)
        {
        }
    }
}
