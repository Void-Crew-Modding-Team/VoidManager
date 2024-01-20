using System;
using System.Collections.Generic;
using System.Linq;
using VoidManager.Chat.Router;

namespace VoidManager.ModMessage
{
    public class ModMessageHandler
    {
        public static Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();
        /// <summary>
        /// Iterates through the current Mods and searches for ModMessages.
        /// </summary>
        public static void DiscoverModMessages()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (VoidCrewMod mod in Plugin.activeMods.Values)
            {
                var types = mod.GetType().Assembly.GetTypes();

                // Finds ModMessage implementations from all the Assemblies in the same file location.
                var modMessageInstances = types.Where(t => typeof(ModMessage).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var modType in modMessageInstances)
                { // Iterates through each discovered ModMessage
                    ModMessage modMessageHandler = (ModMessage)Activator.CreateInstance(modType);
                    modMessageHandlers.Add(mod.HarmonyIdentifier() + "#" + modMessageHandler.GetIdentifier(), modMessageHandler);
                }
            }
        }
    }
}
