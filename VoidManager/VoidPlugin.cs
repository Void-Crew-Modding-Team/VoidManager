using BepInEx;
using System.Diagnostics;
using VoidManager.MPModChecks;

namespace VoidManager
{
    /// <summary>
    /// Used by V-Man to signify a mod. Must have a unique harmonyID
    /// </summary>
    public abstract class VoidPlugin
    {
        internal FileVersionInfo VersionInfo;
        internal byte[] ModHash;

        /// <summary>
        /// Automatically assigned by VoidManager
        /// </summary>
        internal PluginInfo BepinPlugin;

        /// <summary>
        /// Automatically assigned by VoidManager
        /// </summary>
        public PluginInfo MyBepinPlugin
        {
            get => BepinPlugin;
        }

        /// <summary>
        /// Author(s) of mod.
        /// </summary>
        public virtual string Author
        {
            get
            {
                return VersionInfo?.CompanyName;
            }
        }

        /// <summary>
        /// description of mod. Ideal for in-game readme or patch notes.
        /// </summary>
        public virtual string Description
        {
            get
            {
                return VersionInfo?.FileDescription;
            }
        }

        /// <summary>
        /// Mod's multiplayer requirements. Use MPModChecks.MultiplayerType.<br/>
        /// Hidden: Hidden from mod lists<br/>
        /// Client: No requirement<br/>
        /// Unspecified: default. Hosts can toggle between trusting these mods or requiring installation on host.<br/>
        /// All: All players must have the mod installed
        /// </summary>
        public virtual MultiplayerType MPType
        {
            get
            {
                return MultiplayerType.Unspecified;
            }
        }
    }
}
