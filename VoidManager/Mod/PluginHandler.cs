using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VoidManager.Mod
{
    internal class PluginHandler
    {
        public static Dictionary<string, BepInPlugin> ActiveMods { get; set; }
        public static Dictionary<string, BepInPlugin> HostAndClientMods { get; set; }

        /// <summary>
        /// Iterates through the current Plugin files and searches for commands.
        /// </summary>
        public static void DiscoverPlugins()
        {
            ActiveMods = new Dictionary<string, BepInPlugin>();
            HostAndClientMods = new Dictionary<string, BepInPlugin>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                // Finds ManagerPlugin implementations from all the Assemblies in the same file location.
                var bepInPluginInstances = types.Where(t => typeof(BepInPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (bepInPluginInstances.Any())
                {
                    BepInPlugin managerPlugin = (BepInPlugin)Activator.CreateInstance(bepInPluginInstances.First());
                    ActiveMods.Add(managerPlugin.GUID, managerPlugin);
                    if (MetadataHelper.TryGetMetaData(managerPlugin, out ManagerRestrict MngerRestAtt) && MngerRestAtt.MPType == MultiplayerType.All)
                        HostAndClientMods.Add(managerPlugin.GUID, managerPlugin);
                    Chat.Router.CommandHandler.DiscoverCommands(assembly, managerPlugin.Name);
                }
            }
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {ActiveMods.Count} Mods");
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Discovered {HostAndClientMods.Count} Host and Client Mods");
        }
    }
}
