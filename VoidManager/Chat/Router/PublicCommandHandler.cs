using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using VoidManager.ModMessages;

namespace VoidManager.Chat.Router
{
    internal class PublicCommandHandler : ModMessage
    {
        private const int version = 1;
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
            if (version != (int)objects[0])
            {
                BepinPlugin.Log.LogInfo($"Received message from {sender.NickName} with version {objects[0] as string}, expected version {version}");
                return;
            }

            if (objects.Length == 1)
            {
                Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(PublicCommandHandler)), sender, new object[] { version, Argument.ToByteArray(publicCommands) });
            }
            else
            {
                publicCommands = Argument.FromByteArray((byte[]) objects[1]);
            }
        }

        public static void RequestPublicCommands()
        {
            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(PublicCommandHandler)), PhotonNetwork.MasterClient, new object[] { version });
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
