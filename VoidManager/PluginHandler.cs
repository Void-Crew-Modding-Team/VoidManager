using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using VoidManager.Chat.Router;
using VoidManager.ModMessages;

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
                    Chat.Router.CommandHandler.DiscoverPublicCommands(assembly, BepinPlugin.Metadata.Name);
                    ModMessageHandler.DiscoverModMessages(assembly, BepinPlugin);
                    continue;
                }
                var voidPluginInstances = assembly.GetTypes().Where(t => typeof(VoidPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (voidPluginInstances.Any())
                {
                    VoidPlugin voidPlugin = (VoidPlugin)Activator.CreateInstance(voidPluginInstances.First());
                    Chat.Router.CommandHandler.DiscoverCommands(assembly, BepinPlugin.Metadata.Name);
                    Chat.Router.CommandHandler.DiscoverPublicCommands(assembly, BepinPlugin.Metadata.Name);
                    ModMessageHandler.DiscoverModMessages(assembly, BepinPlugin);
                    ActiveVoidPlugins.Add(BepinPlugin.Metadata.GUID, voidPlugin);
                    voidPlugin.VersionInfo = FileVersionInfo.GetVersionInfo(BepinPlugin.Location);
                    voidPlugin.ModHash = GetFileHash(BepinPlugin.Location);
                    voidPlugin.BepinPlugin = BepinPlugin;
                }
            }
            Plugin.Log.LogInfo($"Created {CommandHandler.chatCommandCount} local command(s) and {CommandHandler.publicCommandCount} public command(s)");
            Plugin.Log.LogInfo($"Created {ModMessageHandler.modMessageHandlers.Count()} mod message(s)");
            Plugin.Log.LogInfo($"Discovered {ActiveVoidPlugins.Count} VoidManager plugin(s) from {ActiveBepinPlugins.Count} mod(s)");
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
