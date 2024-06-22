using BepInEx;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using VoidManager.Chat.Router;
using VoidManager.Utilities;

namespace VoidManager.Chat
{
    internal class PublicListModsCommand : PublicCommand
    {
        private List<string> hostMods = null;

        public override string[] CommandAliases()
            => new string[] { "listMods" };

        public override string Description()
            => "Displays a list of current mods in use";

        public override void Execute(string arguments, int SenderID)
        {
            if (hostMods == null) FindHostMods();

            //Send multiple messages to avoid VivoxApiException: Message text too long
            foreach (string modString in hostMods)
            {
                Messaging.Echo(modString, false);
            }
        }

        private void FindHostMods()
        {
            hostMods = new();

            List<PluginInfo> allMods = PluginHandler.ActiveBepinPlugins.Values.ToList();
            allMods.Sort((a, b) => a.Metadata.Name.CompareTo(b.Metadata.Name));

            foreach (PluginInfo pluginInfo in allMods)
            {
                //Find voidPlugin for pluginInfo, skip if it doesn't exist
                if (!PluginHandler.ActiveVoidPlugins.TryGetValue(pluginInfo.Metadata.GUID, out VoidPlugin voidPlugin) &&
                    !PluginHandler.GeneratedVoidPlugins.TryGetValue(pluginInfo.Metadata.GUID, out voidPlugin)) continue;

                //Ignore hidden and client mods
                if (voidPlugin.MPType <= MPModChecks.MultiplayerType.Client) continue;

                hostMods.Add($"<color=#ff7f00>{pluginInfo.Metadata.Name}</color> - {voidPlugin.Description}");
            }
        }
    }
}
