using HarmonyLib;
using System;
using System.Reflection;
using System.Diagnostics;
using BepInEx;
using UnityEngine;
using System.Linq;

namespace VoidManager.Mod
{
    /// <summary>
    /// Used by V-Man to signify a mod. Must have a unique harmonyID
    /// </summary>
    public abstract class VoidPlugin
    {
        internal FileVersionInfo VersionInfo;

        /// <summary>
        /// Version of mod.
        /// </summary>
        public virtual string Version
        {
            get
            {
                return VersionInfo?.FileVersion;
            }
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
        /// Short (one line) description of mod.
        /// </summary>
        public virtual string ShortDescription
        {
            get
            {
                return VersionInfo?.FileDescription;
            }
        }

        /// <summary>
        /// Long (mutli-line) description of mod.  Ideal for in-game readme or patch notes.
        /// </summary>
        public virtual string LongDescription
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Name of mod.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return VersionInfo?.ProductName;
            }
        }

        public virtual MultiplayerType MPType
        {
            get
            {
                return MultiplayerType.Client;
            }
        }
    }
}
