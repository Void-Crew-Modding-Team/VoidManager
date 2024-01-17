using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoidManager.Mod
{
    internal class PluginHandler
    {
        public static Dictionary<string, BepInPlugin> ActiveMods { get; set; }
        public static Dictionary<string, ManagerPlugin> HostAndClientMods { get; set; }

        /// <summary>
        /// Iterates through the current Plugin files and searches for commands.
        /// </summary>
        public static void DiscoverPlugins()
        {
            ActiveMods = new Dictionary<string, BepInPlugin>();
            HostAndClientMods = new Dictionary<string, ManagerPlugin>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                // Finds ManagerPlugin implementations from all the Assemblies in the same file location.
                var managerPluginInstances = types.Where(t => typeof(ManagerPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (managerPluginInstances.Any()) 
                {
                    ManagerPlugin managerPlugin = (ManagerPlugin)Activator.CreateInstance(managerPluginInstances.First());
                    ActiveMods.Add(managerPlugin.GUID, managerPlugin);
                    if (managerPlugin.AllClientsRequireMod) HostAndClientMods.Add(managerPlugin.GUID, managerPlugin);
                    Chat.Router.CommandHandler.DiscoverCommands(assembly, managerPlugin.Name);
                }
                else
                {
                    var bepInPluginInstances = types.Where(t => typeof(BepInPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    if (bepInPluginInstances.Any())
                    {
                        BepInPlugin managerPlugin = (BepInPlugin)Activator.CreateInstance(bepInPluginInstances.First());
                        ActiveMods.Add(managerPlugin.GUID, managerPlugin);
                        Chat.Router.CommandHandler.DiscoverCommands(assembly, managerPlugin.Name);
                    }
                }
                
            }
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActiveMods.Count} Mods");
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {HostAndClientMods.Count} Host and Client Mods");
        }
    }
}
