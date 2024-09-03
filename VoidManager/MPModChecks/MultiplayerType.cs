namespace VoidManager.MPModChecks
{
    /// <summary>
    /// MultiplayerType Enum
    /// </summary>
    public enum MultiplayerType : byte
    {
        /// <summary>
        /// For debug use, will not show to clients.
        /// </summary>
        Hidden,

        /// <summary>
        /// Client-Side. Minimum session type: Mod_Local
        /// </summary>
        Client = 4,

        /// <summary>
        /// Client-Side, displays as a host mod. Minimum session type: Mod_Local
        /// </summary>
        Host = 8,

        /// <summary>
        /// Can only join Mod_Session, but doesn't require other players to install the mod. Mod_Session
        /// </summary>
        Session = 12,

        /// <summary>
        /// All players must have the same mod installed for connection. Mod_Session
        /// </summary>
        All = 20
    }
}
