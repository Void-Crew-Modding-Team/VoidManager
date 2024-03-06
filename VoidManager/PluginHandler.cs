using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace VoidManager
{
    static class PluginHandler
    {
        public static Dictionary<string, PluginInfo> ActiveBepinPlugins { get => Chainloader.PluginInfos; }
        public static Dictionary<string, VoidPlugin> ActiveVoidPlugins { get; private set; }

        /// <summary>
        /// Iterates through the current Plugin files and searches for commands.
        /// </summary>
        public static void DiscoverPlugins()
        {
            ActiveVoidPlugins = new Dictionary<string, VoidPlugin>();
            foreach (PluginInfo BepinPlugin in Chainloader.PluginInfos.Values)
            {
                Assembly assembly = BepinPlugin.Instance.GetType().Assembly;
                // Finds VoidPlugin class.
                var voidPluginInstances = assembly.GetTypes().Where(t => typeof(VoidPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (voidPluginInstances.Any())
                {
                    VoidPlugin voidPlugin = (VoidPlugin)Activator.CreateInstance(voidPluginInstances.First());
                    Chat.Router.CommandHandler.DiscoverCommands(assembly, voidPlugin.Name);
                    ActiveVoidPlugins.Add(BepinPlugin.Metadata.GUID, voidPlugin);
                    voidPlugin.VersionInfo = FileVersionInfo.GetVersionInfo(BepinPlugin.Location);
                    voidPlugin.BepinPlugin = BepinPlugin;
                }
            }
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActiveBepinPlugins.Count} Mods");
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActiveVoidPlugins.Count} VoidManager Plugins");
        }
    }
}
