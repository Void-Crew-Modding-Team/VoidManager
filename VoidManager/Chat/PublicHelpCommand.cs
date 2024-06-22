using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoidManager.Chat.Router;
using VoidManager.Utilities;

namespace VoidManager.Chat
{
    class PublicHelpCommand : PublicCommand
    {
        public override string[] CommandAliases()
            => new string[] { "help" };

        public override string Description()
            => "Displays a list of available public commands";

        public override string[] UsageExamples()
            => new List<string>(base.UsageExamples()).Concat(new string[] { $"!{CommandAliases()[0]} help" }).ToArray();

        public override List<Argument> Arguments()
        {
            List<string> argumentList = new();

            //Required to avoid issues when PublicCommandHandler generates publicCommands
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (PublicCommand command in CommandHandler.GetPublicCommands())
                {
                    foreach (string alias in command.CommandAliases())
                    {
                        argumentList.Add(alias);
                    }
                }
                return new List<Argument>() { new Argument(argumentList.ToArray()) };
            }
            else
            {
                foreach (Argument argument in PublicCommandHandler.publicCommands)
                {
                    foreach (string alias in argument.names)
                    {
                        argumentList.Add(alias);
                    }
                }
                return new List<Argument>() { new Argument(argumentList.ToArray()) };
            }
        }

        public override void Execute(string arguments, int senderId)
        {
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                if (arguments[0] == '!')
                {
                    arguments = arguments.Substring(1);
                }
                BepinPlugin.Log.LogInfo(arguments);
                BepinPlugin.Log.LogInfo(arguments.Split(' ')[0]);
                PublicCommand cmd = CommandHandler.GetPublicCommand(arguments.Split(' ')[0].ToLower());
                StringBuilder stringBuilder = new();
                if (cmd != null)
                {
                    stringBuilder.AppendLine($"<color=green>!{cmd.CommandAliases()[0]}</color> - {cmd.Description()}");// <color=#ff6600ff>[{name}]</color>");
                    stringBuilder.AppendLine($"Aliases: !{string.Join($", !", cmd.CommandAliases())}");
                    stringBuilder.AppendLine($"Usage: {cmd.UsageExamples()[0]}");
                    for (int i = 1; i < cmd.UsageExamples().Length; i++)
                    {
                        stringBuilder.AppendLine($"       {cmd.UsageExamples()[i]}");
                    }
                }
                else
                {
                    stringBuilder.AppendLine($"Public Command !{arguments} not found");
                }
                Messaging.Echo(stringBuilder.ToString(), false);
            }
            else
            {
                IOrderedEnumerable<PublicCommand> commands = CommandHandler.GetPublicCommands();

                //Send multiple messages to avoid VivoxApiException: Message text too long
                Messaging.Echo("<color=green>Public Command List:</color>", false);
                foreach (PublicCommand command in commands)
                {
                    Messaging.Echo($"<color=#3f7fff>!{command.CommandAliases()[0]}</color> - {command.Description()}", false);

                }
                Messaging.Echo("Use <color=green>!help <command></color> for details about a specific public command", false);
            }
        }
    }
}
