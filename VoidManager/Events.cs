using HarmonyLib;
using Photon.Realtime;
using System;

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
        }


        /// <summary>
        /// Called by photon on room leave.
        /// </summary>
        public event EventHandler LeftRoom;

        internal void CallOnLeftRoom()
        {
            LeftRoom?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Called by photon on MasterClient switch.
        /// </summary>
        public event EventHandler<PlayerEventArgs> MasterClientSwitched;

        internal void CallOnMasterClientSwitched(Player newMasterClient)
        {
            MasterClientSwitched?.Invoke(this, new PlayerEventArgs() { player = newMasterClient });
        }


        /// <summary>
        /// Called by VoidManager after client passed Mod Checks.
        /// </summary>
        public event EventHandler<PlayerEventArgs> HostVerifiedClient;

        internal void CallHostOnClientVerified(Player verifiedPlayer) //Called by ModChecksHostOnClientJoin and PlayerJoinedChecks
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
        }

        [HarmonyPatch(typeof(GameSessionManager), "HostGameSession")]
        class HostStartSessionpatch
        {
            static void Postfix()
            {
                Instance.OnHostStartSession();
            }
        }
    }
}
