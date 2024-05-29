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
using VoidManager.MPModChecks;

namespace VoidManager
{
    static class PluginHandler
    {
        public static Dictionary<string, PluginInfo> ActiveBepinPlugins { get => Chainloader.PluginInfos; }
        public static Dictionary<string, VoidPlugin> ActiveVoidPlugins { get; private set; }
        public static Dictionary<string, VoidPlugin> GeneratedVoidPlugins { get; private set; }

        /// <summary>
        /// Iterates through the current plugin files and searches for VoidPlugins.
        /// </summary>
        internal static void DiscoverPlugins()
        {
            VoidPlugin voidPlugin;

            BepinPlugin.Bindings.LoadModListOverride();
            var OverridenMods = BepinPlugin.Bindings.ModOverrideDictionary;

            ActiveVoidPlugins = new();
            GeneratedVoidPlugins = new();
            foreach (PluginInfo CurrentBepinPlugin in Chainloader.PluginInfos.Values)
            {
                Assembly assembly = CurrentBepinPlugin.Instance.GetType().Assembly;
                string BPluginGUID = CurrentBepinPlugin.Metadata.GUID;
                string BPluginName = CurrentBepinPlugin.Metadata.Name;
                // Finds VoidPlugin class.
                if (BPluginGUID == MyPluginInfo.PLUGIN_GUID)
                {
                    CommandHandler.DiscoverCommands(assembly, BPluginName);
                    CommandHandler.DiscoverPublicCommands(assembly, BPluginName);
                    ModMessageHandler.DiscoverModMessages(assembly, CurrentBepinPlugin);
                    continue;
                }
                var voidPluginInstances = assembly.GetTypes().Where(t => typeof(VoidPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (voidPluginInstances.Any())
                {
                    voidPlugin = (VoidPlugin)Activator.CreateInstance(voidPluginInstances.First());
                    voidPlugin.VersionInfo = FileVersionInfo.GetVersionInfo(CurrentBepinPlugin.Location);
                    voidPlugin.ModHash = GetFileHash(CurrentBepinPlugin.Location);
                    voidPlugin.BepinPlugin = CurrentBepinPlugin;
                    CommandHandler.DiscoverCommands(assembly, BPluginName);
                    CommandHandler.DiscoverPublicCommands(assembly, BPluginName);
                    ModMessageHandler.DiscoverModMessages(assembly, CurrentBepinPlugin);
                    CustomGUI.GUIMain.Instance.DiscoverGUIMenus(assembly, voidPlugin);
                    ActiveVoidPlugins.Add(CurrentBepinPlugin.Metadata.GUID, voidPlugin);
                }
                else
                {
                    MultiplayerType MPType;
                    if (OverridenMods.TryGetValue(BPluginGUID, out MPType) || OverridenMods.TryGetValue(BPluginName, out MPType)) //Overrides unspecified type mods with input from config.
                    {
                        voidPlugin = new DefaultVoidPlugin(MPType);
                        BepinPlugin.Log.LogInfo($"Discovered MPType override for {BPluginName}. Setting MPType to {MPType}");
                    }
                    else
                    {
                        voidPlugin = new DefaultVoidPlugin(MultiplayerType.Unspecified);
                    }

                    voidPlugin.VersionInfo = FileVersionInfo.GetVersionInfo(CurrentBepinPlugin.Location);
                    voidPlugin.ModHash = GetFileHash(CurrentBepinPlugin.Location);
                    voidPlugin.BepinPlugin = CurrentBepinPlugin;
                    CustomGUI.GUIMain.Instance.DiscoverNonVManMod(voidPlugin);
                    GeneratedVoidPlugins.Add(BPluginGUID, voidPlugin);
                }
            }
            CustomGUI.GUIMain.Instance.mods.Sort((plugin1, plugin2) => plugin1.BepinPlugin.MetaData.Name.CompareTo(plugin2.BepinPlugin.MetaData.Name));
            BepinPlugin.Log.LogInfo($"Loaded {CommandHandler.chatCommandCount} local command(s) and {CommandHandler.publicCommandCount} public command(s)");
            BepinPlugin.Log.LogInfo($"Loaded {ModMessageHandler.modMessageHandlers.Count()} mod message(s)");
            BepinPlugin.Log.LogInfo($"Discovered {ActiveVoidPlugins.Count} VoidManager plugin(s) from {ActiveBepinPlugins.Count - 1} mod(s)");
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
