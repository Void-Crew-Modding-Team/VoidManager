using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using VoidManager.ModMessages;

namespace VoidManager.Chat.Router
{
    internal class PublicCommandHandler : ModMessage
    {
        private static List<Argument> localPublicCommandsCache = null;
        private static List<Argument> LocalPublicCommands { get => localPublicCommandsCache ??= GetLocalPublicCommands(); }

        internal static List<Argument> publicCommands = new();

        public override void Handle(object[] objects, Player sender)
        {
            if (objects.Length == 0)
            {
                BepinPlugin.Log.LogInfo($"Received message from {sender.NickName} with no version");
                return;
            }
            if (objects[0] as string != MyPluginInfo.PLUGIN_VERSION)
            {
                BepinPlugin.Log.LogInfo($"Received message from {sender.NickName} with version {objects[0] as string}, expected version {MyPluginInfo.PLUGIN_VERSION}");
                return;
            }

            if (objects.Length == 1)
            {
                Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(PublicCommandHandler)), sender, new object[] { MyPluginInfo.PLUGIN_VERSION, Argument.ToByteArray(publicCommands) });
            }
            else
            {
                publicCommands = Argument.FromByteArray((byte[]) objects[1]);
            }
        }

        public static void RequestPublicCommands()
        {
            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(PublicCommandHandler)), PhotonNetwork.MasterClient, new object[] { MyPluginInfo.PLUGIN_VERSION });
        }

        private static List<Argument> GetLocalPublicCommands()
        {
            List<Argument> commands = new();
            foreach (PublicCommand command in CommandHandler.GetPublicCommands())
            {
                commands.Add(new Argument(command.CommandAliases(), command.Arguments()));
            }
            return commands;
        }

        internal static void RefreshPublicCommandCache(object sender, EventArgs e)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                publicCommands = LocalPublicCommands;
            }
            else
            {
                publicCommands = new();
                RequestPublicCommands();
            }
        }
    }
}
