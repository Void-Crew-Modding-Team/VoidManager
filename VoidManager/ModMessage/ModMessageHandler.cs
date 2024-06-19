using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoidManager.ModMessages
{
    /// <summary>
    /// Handles RPCs between modded clients.
    /// </summary>
    internal class ModMessageHandler
    {
        internal static Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();

        /// <summary>
        /// Scans the given assembly for ModMessage classes.
        /// </summary>
        internal static void DiscoverModMessages(System.Reflection.Assembly assembly, PluginInfo bepinPlugin)
        {
            Type[] types = assembly.GetTypes();

            // Finds ModMessage implementations from all the Assemblies in the same file location.
            IEnumerable<Type> modMessageInstances = types.Where(t => typeof(ModMessage).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            int modMessageCount = 0;
            foreach (Type modType in modMessageInstances)
            { // Iterates through each discovered ModMessage
                ModMessage modMessageHandler = (ModMessage)Activator.CreateInstance(modType);
                modMessageHandlers.Add(bepinPlugin.Metadata.GUID + "#" + modMessageHandler.GetIdentifier(), modMessageHandler);
                modMessageCount++;
            }
            if (modMessageCount != 0)
            {
                BepinPlugin.Log.LogInfo($"[{bepinPlugin.Metadata.Name}] Detected {modMessageCount} mod messages");
            }
        }
    }
}
