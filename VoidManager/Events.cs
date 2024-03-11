using Photon.Realtime;

namespace VoidManager
{
    public class Events
    {
        /// <summary>
        /// creates VoidManager.Events Instance.
        /// </summary>
        internal Events()
        {
            Instance = this;
        }

        /// <summary>
        /// The current Events Instance.
        /// </summary>
        public static Events Instance;

        /// <summary>
        /// Used by OnPlayerEnteredRoomEvent
        /// </summary>
        public delegate void OnPlayerEnteredRoomDelegate(Player joiningPlayer);

        /// <summary>
        /// Called by photon on player join.
        /// </summary>
        public event OnPlayerEnteredRoomDelegate OnPlayerEnteredRoomEvent;

        internal void CallOnPlayerEnteredRoomEvent(Player joiningPlayer)
        {
            OnPlayerEnteredRoomEvent?.Invoke(joiningPlayer);
        }


        /// <summary>
        /// Used by OnPlayerLeftRoomEvent
        /// </summary>
        public delegate void OnPlayerLeftRoomDelegate(Player leavingPlayer);

        /// <summary>
        /// Called by photon on player leave.
        /// </summary>
        public event OnPlayerLeftRoomDelegate OnPlayerLeftRoomEvent;

        internal void CallOnPlayerLeftRoomEvent(Player leavingPlayer)
        {
            OnPlayerLeftRoomEvent?.Invoke(leavingPlayer);
        }


        /// <summary>
        /// Used by OnJoinedRoomEvent
        /// </summary>
        public delegate void OnJoinedRoomDelegate();

        /// <summary>
        /// Called by photon on room join.
        /// </summary>
        public event OnJoinedRoomDelegate OnJoinedRoomEvent;

        internal void CallOnJoinedRoomEvent()
        {
            OnJoinedRoomEvent?.Invoke();
        }


        /// <summary>
        /// Used by OnLeftRoomEvent
        /// </summary>
        public delegate void OnLeftRoomDelegate();

        /// <summary>
        /// Called by photon on room leave.
        /// </summary>
        public event OnLeftRoomDelegate OnLeftRoomEvent;

        internal void CallOnLeftRoomEvent()
        {
            OnLeftRoomEvent?.Invoke();
        }


        /// <summary>
        /// Used by OnMasterClientSwitchedEvent
        /// </summary>
        public delegate void OnMasterClientSwitchedDelegate(Player newMasterClient);

        /// <summary>
        /// Called by photon on MasterClient switch.
        /// </summary>
        public event OnMasterClientSwitchedDelegate OnMasterClientSwitchedEvent;

        internal void CallOnMasterClientSwitchedEvent(Player newMasterClient)
        {
            OnMasterClientSwitchedEvent?.Invoke(newMasterClient);
        }


        /// <summary>
        /// Used by HostOnClientVerifiedEvent
        /// </summary>
        public delegate void HostOnClientVerifiedDelegate(Player verifiedPlayer);

        /// <summary>
        /// Called by VoidManager after client passed Mod Checks.
        /// </summary>
        public event HostOnClientVerifiedDelegate HostOnClientVerifiedEvent;

        internal void CallHostOnClientVerifiedEvent(Player verifiedPlayer) //Called by ModChecksHostOnClientJoin and PlayerJoinedChecks
        {
            HostOnClientVerifiedEvent?.Invoke(verifiedPlayer);
        }


        /// <summary>
        /// Used by ClientModlistRecievedEvent
        /// </summary>
        /// <param name="DataSender"></param>
        public delegate void ClientModlistRecievedDelegate(Player DataSender);

        /// <summary>
        /// Called after a client modlist has been recieved by the MPModCheckManager instance.
        /// </summary>
        public event ClientModlistRecievedDelegate ClientModlistRecievedEvent;

        internal void CallClientModlistRecievedEvent(Player DataSender)
        {
            ClientModlistRecievedEvent?.Invoke(DataSender);
        }
    }
}
