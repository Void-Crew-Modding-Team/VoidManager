namespace VoidManager.MPModChecks
{
    /// <summary>
    /// MultiplayerType Enum
    /// </summary>
    public enum MultiplayerType : byte
    {
        /// <summary>
        /// For debug use, will not show to clients
        /// </summary>
        Hidden,

        /// <summary>
        /// Client-Side
        /// </summary>
        Client = 3,

        /// <summary>
        /// Client-Side, but displays as a host mod.
        /// </summary>
        Host = 4,

        /// <summary>
        /// mod has not had it's multiplayer operations specified for VoidManager.
        ///- If the host has VoidManager and this mod, Connection will be allowed.
        ///- If the host has VoidManager but not this mod, they can optionally trust Unspecified Mods.
        ///- If the host does not have VoidManager, Connection will be disallowed.
        ///- If the local client is hosting, vanilla clients will be allowed to join the session.
        /// </summary>
        Unspecified = 6,

        //HostMustHave,

        /// <summary>
        /// All players must have the same mod installed for connection.
        /// </summary>
        All = 10
    }
}
