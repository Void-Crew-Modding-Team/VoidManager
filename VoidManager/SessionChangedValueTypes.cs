#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace VoidManager
{
    /// <summary>
    /// Input values for virtual method OnSessionChanged
    /// </summary>
    public struct SessionChangedInput
    {
        public SessionChangedInput(bool isHost, CallType callType, bool startedAsHost, bool isMod_Session, bool hostHasMod)
        {
            IsHost = isHost;
            CallType = callType;
            CreatedRoomAsHost = startedAsHost;
            IsMod_Session = isMod_Session;
            HostHasMod = hostHasMod;
        }

        public bool IsHost;

        /// <summary>
        /// Reason for call of OnSessionChanged
        /// </summary>
        public CallType CallType;

        /// <summary>
        /// Current PhotonRoom was created as host.
        /// </summary>
        public bool CreatedRoomAsHost;

        /// <summary>
        /// Current Session/Run was started as host.
        /// </summary>
        public bool StartedSessionAsHost;
        public bool IsMod_Session;

        /// <summary>
        /// Host has mod, based on GUID without version/hash checking.
        /// </summary>
        public bool HostHasMod;
    }

    /// <summary>
    /// Reason for call of OnSessionChanged
    /// </summary>
    public enum CallType
    {
        /// <summary>
        /// On PhotonRoom Creation. Might not run if the host drops.
        /// </summary>
        HostCreateRoom,

        /// <summary>
        /// On Load into hub or ship as MasterClient.
        /// </summary>
        HostStartSession,

        /// <summary>
        /// On Load into hub or ship as client.
        /// </summary>
        Joining,

        /// <summary>
        /// On Host Change.
        /// </summary>
        HostChange,

        /// <summary>
        /// On Session escalation from Mod_Local to Mod_Session.
        /// </summary>
        SessionEscalated
    }

    /// <summary>
    /// Return values for virtual method OnSessionChanged
    /// </summary>
    public struct SessionChangedReturn
    {
        public bool SetMod_Session;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
