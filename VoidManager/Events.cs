﻿using CG.Game;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using UI.Chat;
using VoidManager.CustomGUI;
using VoidManager.LobbyPlayerList;
using VoidManager.MPModChecks;
using VoidManager.Progression;

namespace VoidManager
{
    /// <summary>
    /// Provides EventHandlers for subscription.
    /// </summary>
    public class Events
    {
        /// <summary>
        /// The current Events Instance.
        /// </summary>
        public static Events Instance { get; internal set; }

        /// <summary>
        /// Used by VoidManager.Events to pass the Photon Player as an argument
        /// </summary>
        public class PlayerEventArgs : EventArgs
        {
            /// <summary>
            /// player argument
            /// </summary>
            public Player player;
        }

        /// <summary>
        /// Used by VoidManager.Events to pass the Photon Player and changed properties as an argument
        /// </summary>
        public class PlayerPropertiesEventArgs : EventArgs
        {
            /// <summary>
            /// player argument
            /// </summary>
            public Player player;

            /// <summary>
            /// Changed Properties
            /// </summary>
            public Hashtable changedProperties;
        }

        /// <summary>
        /// Used by VoidManager.Events to pass the changed properties as an argument
        /// </summary>
        public class RoomPropertiesEventArgs : EventArgs
        {
            /// <summary>
            /// Changed Properties
            /// </summary>
            public Hashtable changedProperties;
        }


        /// <summary>
        /// Called by photon on player join.
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerEnteredRoom;

        internal void OnPlayerEnteredRoom(Player joiningPlayer)
        {
            NetworkedPeerManager.CheckPlayerModsFromProperties(joiningPlayer);
            ProgressionHandler.OnPlayerJoin(joiningPlayer);
            MPModCheckManager.Instance.PlayerJoined(joiningPlayer);
            LobbyPlayerListManager.Instance.UpdateLobbyPlayers();

            try
            {
                PlayerEnteredRoom?.Invoke(this, new PlayerEventArgs() { player = joiningPlayer });
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
        }


        /// <summary>
        /// Called by photon on player leave.
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerLeftRoom;

        internal void OnPlayerLeftRoom(Player leavingPlayer)
        {
            if (GUIMain.Instance.selectedPlayer == leavingPlayer)
            {
                GUIMain.Instance.SelectPlayer(null);
            }

            LobbyPlayerListManager.Instance.UpdateLobbyPlayers();

            try
            {
                PlayerLeftRoom?.Invoke(this, new PlayerEventArgs() { player = leavingPlayer });
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }

            NetworkedPeerManager.Instance.PlayerLeftRoom(leavingPlayer);
        }


        /// <summary>
        /// Called by photon on room join. Occurs once in a single photon room.
        /// </summary>
        public event EventHandler JoinedRoom;

        internal void OnJoinedRoom()
        {
            MPModCheckManager.Instance.JoinedRoom();
            PluginHandler.SessionWasEscalated = false;

            //Above controls whether a game is joined, so it is better to let it run first.
            try
            {
                JoinedRoom?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
        }


        /// <summary>
        /// Called by photon on room leave.
        /// </summary>
        public event EventHandler LeftRoom;

        internal void OnLeftRoom()
        {
            NetworkedPeerManager.Instance.LeftRoom();

            try
            {
                LeftRoom?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
        }


        /// <summary>
        /// Called by photon on MasterClient switch.
        /// </summary>
        public event EventHandler<PlayerEventArgs> MasterClientSwitched;

        internal void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
                MPModCheckManager.Instance.UpdateLobbyProperties();

            ProgressionHandler.OnHostChange(newMasterClient);
            bool IsMasterClient = newMasterClient.IsLocal;
            try
            {
                MasterClientSwitched?.Invoke(this, new PlayerEventArgs() { player = newMasterClient });
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
            PluginHandler.InternalSessionChanged(CallType.HostChange, ((IsMasterClient && ModdingUtils.SessionModdingType == ModdingType.mod_session) || MPModCheckManager.IsMod_Session()), IsMasterClient, newMasterClient);
        }


        /// <summary>
        /// Called by VoidManager after client passed Mod Checks.
        /// </summary>
        public event EventHandler<PlayerEventArgs> HostVerifiedClient;

        internal void OnHostVerifiedClient(Player verifiedPlayer) //Called by ModChecksHostOnClientJoin and PlayerJoinedChecks
        {
            try
            {
                HostVerifiedClient?.Invoke(this, new PlayerEventArgs() { player = verifiedPlayer });
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
        }


        /// <summary>
        /// Called after a client modlist has been recieved by the MPModCheckManager instance.
        /// </summary>
        public event EventHandler<PlayerEventArgs> ClientModlistRecieved;

        internal void OnClientModlistRecieved(Player DataSender)
        {
            try
            {
                ClientModlistRecieved?.Invoke(this, new PlayerEventArgs() { player = DataSender });
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
        }


        /// <summary>
        /// Called after after loading a hosted game session (reccurs multiple times in the same PhotonRoom).
        /// </summary>
        public event EventHandler HostStartSession;

        internal void OnHostStartSession()
        {
            try
            {
                HostStartSession?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
            PluginHandler.InternalSessionChanged(CallType.HostStartSession, MPModCheckManager.IsMod_Session(), true);
        }

        [HarmonyPatch(typeof(GameSessionManager), "HostGameSession")]
        class HostStartSessionPatch
        {
            static void Postfix()
            {
                Instance.OnHostStartSession();
            }
        }


        /// <summary>
        /// Called by host when creating a photon room. (Recurs once, may fail to execute if host drops)
        /// </summary>
        public event EventHandler HostCreateRoom;

        internal void OnHostCreateRoom()
        {
            try
            {
                HostCreateRoom?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }

            PluginHandler.CreatedRoomAsHost = true;
            PluginHandler.InternalSessionChanged(CallType.HostCreateRoom, ModdingUtils.SessionModdingType == ModdingType.mod_session, true);
        }


        /// <summary>
        /// Called after after loading a joined game session (reccurs multiple times in the same PhotonRoom).
        /// </summary>
        public event EventHandler JoinedSession;

        internal void OnJoinedSession()
        {
            try
            {
                JoinedSession?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }

            PluginHandler.CreatedRoomAsHost = false;
            PluginHandler.InternalSessionChanged(CallType.Joining, MPModCheckManager.IsMod_Session(), false, PhotonNetwork.MasterClient);
        }

        [HarmonyPatch(typeof(GameSessionManager), "JoinGameSession")]
        class JoinSessionPatch
        {
            static void Postfix()
            {
                Instance.OnJoinedSession();
            }
        }


        /// <summary>
        /// Called on player properties update.
        /// </summary>
        public event EventHandler<PlayerPropertiesEventArgs> PlayerPropertiesUpdate;

        internal void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // Run on properties update to keep up with player level ups. May not be updated when local player levels up.
            if (changedProps.ContainsKey("RP_PR") || changedProps.ContainsKey("RP_FR"))
            {
                LobbyPlayerListManager.Instance.UpdateLobbyPlayers();
            }

            try
            {
                PlayerPropertiesUpdate?.Invoke(this, new PlayerPropertiesEventArgs() { player = targetPlayer, changedProperties = changedProps });
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
        }


        /// <summary>
        /// Called on room properties update.
        /// </summary>
        public event EventHandler<RoomPropertiesEventArgs> RoomPropertiesUpdate;

        internal void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            try
            {
                RoomPropertiesUpdate?.Invoke(this, new RoomPropertiesEventArgs() { changedProperties = propertiesThatChanged });
            }
            catch (Exception e)
            {
                BepinPlugin.Log.LogError(e);
            }
        }


        /// <summary>
        /// Called when the player opens the chat window ("Enter" by default)
        /// </summary>
        public event EventHandler ChatWindowOpened;

        /// <summary>
        /// Called when the player closes the chat window, by sending a message or cancelling
        /// </summary>
        public event EventHandler ChatWindowClosed;

        [HarmonyPatch(typeof(TextChatVE))]
        class TextChatVEPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ShowInput")]
            static void ShowChatWindow()
            {
                Instance.ChatWindowOpened.Invoke(Instance, EventArgs.Empty);
            }

            [HarmonyPostfix]
            [HarmonyPatch("HideInput")]
            static void HideChatWindow()
            {
                Instance.ChatWindowClosed.Invoke(Instance, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Runs once per frame
        /// </summary>
        public event EventHandler LateUpdate;

        [HarmonyPatch(typeof(ClientGame), "LateUpdate")]
        static class ClientGamePatch
        {
            static void Postfix()
            {
                try
                {
                    Instance.LateUpdate?.Invoke(Instance, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    BepinPlugin.Log.LogError(e);
                }
            }
        }
    }
}
