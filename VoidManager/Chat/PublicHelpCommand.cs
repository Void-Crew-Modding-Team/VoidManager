using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
            => new List<string>(base.UsageExamples()).Concat(new string[] { $"!{CommandAliases()[0]} help", $"!{CommandAliases()[0]} 2" }).ToArray();

        /*public override string[][] Arguments()
            => new string[][] { new string[] { "%command", "%page_number" } };*/

        public override void Execute(string arguments, int senderId)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int page = 1;
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    if (!int.TryParse(arguments, out page))
                    {
                        if (arguments[0] == '!')
                        {
                            arguments = arguments.Substring(1);
                        }
                        PublicCommand cmd = Router.CommandHandler.GetPublicCommand(arguments.Split(' ')[0]);
                        StringBuilder stringBuilder = new StringBuilder();
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
                        return;
                    }
                }

                int commandsPerPage = 6;
                IOrderedEnumerable<PublicCommand> commands = Router.CommandHandler.GetPublicCommands();
                int pages = UnityEngine.Mathf.CeilToInt(commands.Count() / (float)commandsPerPage);
                page--; //Pages start from 1
                if (page < 0)
                {
                    page = 0;
                }

                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder2.AppendLine(pages == 1 && page == 0 ? "<color=green>Public Command List:</color> :" : $"<color=green>Public Command List:</color> Page {page + 1} : {pages}");
                for (int i = 0; i < commandsPerPage; i++)
                {
                    int index = i + page * commandsPerPage;
                    if (i + page * commandsPerPage >= commands.Count())
                        break;
                    PublicCommand command = commands.ElementAt(index);
                    stringBuilder2.AppendLine($"!{command.CommandAliases()[0]} - {command.Description()}");

                }
                stringBuilder2.AppendLine("Use <color=green>!help <command></color> for details about a specific public command");
                Messaging.Echo(stringBuilder2.ToString(), false);
            }
        }
    }
}
