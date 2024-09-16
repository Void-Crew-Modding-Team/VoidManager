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
        /// Description of mod. Ideal for in-game readme or patch notes.
        /// </summary>
        public virtual string Description
        {
            get
            {
                return VersionInfo?.FileDescription;
            }
        }

        /// <summary>
        /// Thunderstore ID. In a link (https://thunderstore.io/c/void-crew/p/VoidCrewModdingTeam/VoidManager/), the section equivelant to 'VoidCrewModdingTeam/VoidManager')
        /// </summary>
        public virtual string ThunderstoreID
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Mod's multiplayer requirements. Use MPModChecks.MultiplayerType. Defaults as Session<br/>
        /// </summary>
        public virtual MultiplayerType MPType
        {
            get
            {
                return MultiplayerType.Session;
            }
        }

        /// <summary>
        /// Called by VoidManager on Host Create Room, Host Session, Join Session, Host Change, Session Escalation. May run multiple times if a mod escalates to Mod_Session.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual SessionChangedReturn OnSessionChange(SessionChangedInput input)
        {
            return new SessionChangedReturn() { SetMod_Session = false };
        }
    }
}
