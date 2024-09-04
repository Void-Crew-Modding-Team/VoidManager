using static VoidManager.MPModChecks.MPModCheckManager;

namespace VoidManager.MPModChecks
{
    /// <summary>
    /// Contains data for a given mod pair when being checked.
    /// </summary>
    public struct CheckConditions
    {
        /// <summary>
        /// Contains data for a given mod pair when being checked.
        /// </summary>
        public CheckConditions()
        {
        }

        /// <summary>
        /// The mod belonging to the ChecKConditions instance.
        /// </summary>
        public MPModDataBlock Mod;

        /// <summary>
        /// If host is calling, true; If Client is calling, false.
        /// </summary>
        public bool HostCheck = false;

        /// <summary>
        /// If Client, Host, or both players have this mod installed.
        /// </summary>
        public PlayersWithMod PlayersWithMod;

        /// <summary>
        /// Version string of client's mod.
        /// </summary>
        public string ClientModVersion = string.Empty;

        /// <summary>
        /// Version string of host's mod.
        /// </summary>
        public string HostModVersion = string.Empty;

        /// <summary>
        /// Current session is Mod_Session.
        /// </summary>
        public bool IsMod_Session;

        /// <summary>
        /// If is HostCheck and sha256 hash sent by client matches host's hash.
        /// </summary>
        public bool HashesMatch = false;
    }
}
