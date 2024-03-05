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
        protected Harmony harmony;
        internal FileVersionInfo VersionInfo;
        public VoidPlugin()
        {
            Assembly asm = GetType().Assembly;
            var bepInPluginInstances = asm.GetTypes().Where(t => typeof(BepInPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            if (bepInPluginInstances.Any())
            {
                BepInPlugin managerPlugin = (BepInPlugin)Activator.CreateInstance(bepInPluginInstances.First());
                harmony = new Harmony(managerPlugin.GUID);
                harmony.PatchAll(asm);
            }
            else
            {
                Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Failed to patch Harmony for {Name}");
            }
        }

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
