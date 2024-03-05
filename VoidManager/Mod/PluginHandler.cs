using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VoidManager.Mod
{
    internal class PluginHandler
    {
        public static Dictionary<string, PluginInfo> ActiveMods { get => Chainloader.PluginInfos; }
        public static Dictionary<string, VoidPlugin> ActivePlugins { get; private set; }

        /// <summary>
        /// Iterates through the current Plugin files and searches for commands.
        /// </summary>
        public static void DiscoverPlugins()
        {
            ActivePlugins = new Dictionary<string, VoidPlugin>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (PluginInfo plugin in Chainloader.PluginInfos.Values)
            {
                Assembly assembly = plugin.Instance.GetType().Assembly;
                // Finds VoidPlugin class.
                var voidPluginInstances = assembly.GetTypes().Where(t => typeof(VoidPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (voidPluginInstances.Any())
                {
                    VoidPlugin voidPlugin = (VoidPlugin)Activator.CreateInstance(voidPluginInstances.First());
                    Chat.Router.CommandHandler.DiscoverCommands(assembly, voidPlugin.Name);
                    ActivePlugins.Add(plugin.Metadata.GUID, voidPlugin);
                }
            }
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActiveMods.Count} Mods");
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActivePlugins.Count} VoidManager Plugins");
        }
    }
}
