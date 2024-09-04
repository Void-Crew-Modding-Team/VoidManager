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
        /// Unknown/Unmanaged. This mod's multiplayer effectiveness is ignored by VoidManager. Intended for non-VoidManager mods.
        /// </summary>
        Unmanaged = 4,

        /// <summary>
        /// Client-Side. Minimum session type: Mod_Local
        /// </summary>
        Client = 8,

        /// <summary>
        /// Client-Side, displays as a host mod. Minimum session type: Mod_Local
        /// </summary>
        Host = 14,

        /// <summary>
        /// Can only join/host Mod_Session, but doesn't require other players to install the mod. Mod_Session
        /// </summary>
        Session = 20,

        /// <summary>
        /// All players must have the same mod installed for connection. Mod_Session
        /// </summary>
        All = 30
    }
}
