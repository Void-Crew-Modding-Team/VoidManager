using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            StartedAsHost = startedAsHost;
            IsMod_Session = isMod_Session;
            HostHasMod = hostHasMod;
        }

        public bool IsHost;
        public CallType CallType;
        public bool StartedAsHost;
        public bool IsMod_Session;
        public bool HostHasMod;
    }

    /// <summary>
    /// Reason for call of OnSessionChanged
    /// </summary>
    public enum CallType
    {
        Hosting,
        Joining,
        HostChange,
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
