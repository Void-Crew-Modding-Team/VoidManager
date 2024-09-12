using CG.Game;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using UI.Chat;

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
        /// Called by photon on player join.
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerEnteredRoom;

        internal void OnPlayerEnteredRoom(Player joiningPlayer)
        {
            PlayerEnteredRoom?.Invoke(this, new PlayerEventArgs() { player = joiningPlayer });
        }


        /// <summary>
        /// Called by photon on player leave.
        /// </summary>
        public event EventHandler<PlayerEventArgs> PlayerLeftRoom;

        internal void OnPlayerLeftRoom(Player leavingPlayer)
        {
            PlayerLeftRoom?.Invoke(this, new PlayerEventArgs() { player = leavingPlayer });
        }


        /// <summary>
        /// Called by photon on room join.
        /// </summary>
        public event EventHandler JoinedRoom;

        internal void OnJoinedRoom()
        {
            JoinedRoom?.Invoke(this, EventArgs.Empty);
            SessionChanged?.Invoke(this, new SessionChangedInput(false, CallType.Joining, PhotonNetwork.MasterClient.IsLocal, false, false));
        }


        /// <summary>
        /// Called by photon on room leave.
        /// </summary>
        public event EventHandler LeftRoom;

        internal void OnLeftRoom()
        {
            LeftRoom?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Called by photon on MasterClient switch.
        /// </summary>
        public event EventHandler<PlayerEventArgs> MasterClientSwitched;

        internal void OnMasterClientSwitched(Player newMasterClient)
        {
            MasterClientSwitched?.Invoke(this, new PlayerEventArgs() { player = newMasterClient });
            SessionChanged?.Invoke(this, new SessionChangedInput(newMasterClient.IsLocal, CallType.HostChange, false, false, false));
        }


        /// <summary>
        /// Called by VoidManager after client passed Mod Checks.
        /// </summary>
        public event EventHandler<PlayerEventArgs> HostVerifiedClient;

        internal void OnHostVerifiedClient(Player verifiedPlayer) //Called by ModChecksHostOnClientJoin and PlayerJoinedChecks
        {
            HostVerifiedClient?.Invoke(this, new PlayerEventArgs() { player = verifiedPlayer });
        }


        /// <summary>
        /// Called after a client modlist has been recieved by the MPModCheckManager instance.
        /// </summary>
        public event EventHandler<PlayerEventArgs> ClientModlistRecieved;

        internal void OnClientModlistRecieved(Player DataSender)
        {
            ClientModlistRecieved?.Invoke(this, new PlayerEventArgs() { player = DataSender });
        }


        /// <summary>
        /// Called after after loading a game session (reccurs multiple times in the same PhotonRoom).
        /// </summary>
        public event EventHandler HostStartSession;

        internal void OnHostStartSession()
        {
            HostStartSession?.Invoke(this, EventArgs.Empty);
            SessionChanged?.Invoke(this, new SessionChangedInput(true, CallType.Hosting, true, false, false));
        }


        /// <summary>
        ///  Called when: hosting a session, joining a session, on host change, on session escalation.
        /// </summary>
        public event EventHandler<SessionChangedInput> SessionChanged;

        /// <summary>
        /// Used to escalate sessions MPType
        /// </summary>
        internal void OnEscalateSession()
        {
            SessionChanged?.Invoke(this, new SessionChangedInput(PhotonNetwork.MasterClient.IsLocal, CallType.SessionEscalated, false, false, false));
        }


        [HarmonyPatch(typeof(GameSessionManager), "HostGameSession")]
        class HostStartSessionpatch
        {
            static void Postfix()
            {
                Instance.OnHostStartSession();
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
                Instance.LateUpdate.Invoke(Instance, EventArgs.Empty);
            }
        }
    }
}
