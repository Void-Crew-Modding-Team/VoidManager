using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace VoidManager
{
    static class PluginHandler
    {
        public static Dictionary<string, PluginInfo> ActiveBepinPlugins { get => Chainloader.PluginInfos; }
        public static Dictionary<string, VoidPlugin> ActiveVoidPlugins { get; private set; }

        /// <summary>
        /// Iterates through the current Plugin files and searches for commands.
        /// </summary>
        internal static void DiscoverPlugins()
        {
            ActiveVoidPlugins = new Dictionary<string, VoidPlugin>();
            foreach (PluginInfo BepinPlugin in Chainloader.PluginInfos.Values)
            {
                Assembly assembly = BepinPlugin.Instance.GetType().Assembly;
                // Finds VoidPlugin class.
                if(BepinPlugin.Metadata.GUID == MyPluginInfo.PLUGIN_GUID)
                {
                    Chat.Router.CommandHandler.DiscoverCommands(assembly, BepinPlugin.Metadata.Name);
                    continue;
                }
                var voidPluginInstances = assembly.GetTypes().Where(t => typeof(VoidPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (voidPluginInstances.Any())
                {
                    VoidPlugin voidPlugin = (VoidPlugin)Activator.CreateInstance(voidPluginInstances.First());
                    Chat.Router.CommandHandler.DiscoverCommands(assembly, BepinPlugin.Metadata.Name);
                    ActiveVoidPlugins.Add(BepinPlugin.Metadata.GUID, voidPlugin);
                    voidPlugin.VersionInfo = FileVersionInfo.GetVersionInfo(BepinPlugin.Location);
                    voidPlugin.ModHash = GetFileHash(BepinPlugin.Location);
                    voidPlugin.BepinPlugin = BepinPlugin;
                }
            }
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActiveBepinPlugins.Count} Mods");
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActiveVoidPlugins.Count} VoidManager Plugins");
        }

        public static byte[] GetFileHash(string fileLocation)
        {
            using (SHA256 Hasher = SHA256.Create())
            {
                return Hasher.ComputeHash(File.ReadAllBytes(fileLocation));
            }
        }
    }
}
