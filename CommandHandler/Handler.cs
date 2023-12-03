using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandHandler
{
    internal class Handler
    {
        private static Dictionary<string, ChatCommand> chatCommands = new Dictionary<string, ChatCommand>();
        public static int chatCommandCount => chatCommands.Count;

        /// <summary>
        /// Executes chat command if found from alias with arguments.
        /// </summary>
        /// <param name="alias">Potential chat command alias</param>
        /// <param name="arguments">Arguments to use with command</param>
        public static void ExecuteCommandFromAlias(string alias, string arguments)
        {
            alias = alias.ToLower();
            try
            {
                if (chatCommands.ContainsKey(alias)) chatCommands[alias].Execute(arguments);
            }
            catch
            {
                Plugin.Log.LogError($"[{MyPluginInfo.PLUGIN_NAME}] /{alias} {arguments} failed!");
            }
        }

        /// <summary>
        /// Iterates through the current Plugin files and searches for commands.
        /// </summary>
        public static void DiscoverPlugins()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                // Finds ChatCommand implementations from all the Assemblies in the same file location.
                var chatCommandInstances = types.Where(t => typeof(ChatCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var modType in chatCommandInstances)
                { // Iterates through each discovered ChatCommand
                    ChatCommand modInstance = (ChatCommand)Activator.CreateInstance(modType);
                    foreach (string commandAlias in Array.ConvertAll(modInstance.CommandAliases(), d => d.ToLower()))
                    {
                        if (chatCommands.ContainsKey(commandAlias))
                        {
                            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Found duplicate command alias {commandAlias}");
                            continue;
                        }
                        else
                        {
                            chatCommands.Add(commandAlias, modInstance);
                        }
                    }
                }
            }
            Plugin.Log.LogInfo($"[{MyPluginInfo.PLUGIN_NAME}] Added {Handler.chatCommandCount} chat commands");
        }
    }
}
