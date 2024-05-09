using VoidManager.MPModChecks;

namespace VoidManager
{
    internal class DefaultVoidPlugin : VoidPlugin
    {
        internal DefaultVoidPlugin(MultiplayerType inputMPType)
        {
            m_MPType = inputMPType;
        }

        internal MultiplayerType m_MPType;

        public override string Author => string.Empty;

        public override string Description => "Info auto-filled";

        public override MultiplayerType MPType => m_MPType;
    }
}
