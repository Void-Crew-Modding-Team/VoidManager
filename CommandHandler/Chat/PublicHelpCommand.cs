using System.Collections.Generic;
using System.Linq;
using VoidManager.Chat.Router;

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
            return;
            /*if (PhotonNetwork.IsMasterClient)
            { 
                IOrderedEnumerable<PublicCommand> publicCommands = Router.CommandHandler.GetPublicCommands();

                if (publicCommands.Count() <= 1)
                {
                    return;
                }

                int page = 1;
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    if (!int.TryParse(arguments, out page))
                    {
                        if (arguments[0] == '!')
                        {
                            arguments = arguments.Substring(1);
                        }
                        PublicCommand cmd = Router.CommandHandler.GetPublicCommand(arguments);
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
                            stringBuilder.AppendLine($"Command !{arguments} not found");
                        }
                        Messaging.Echo(stringBuilder.ToString(), senderId);
                        return;
                    }
                }

                int commandsPerPage = 13 /*(PLXMLOptionsIO.Instance.CurrentOptions.GetStringValueAsInt("ChatNumLines") * 5 + 10) - 2*/; //Minimum value
            /*int pages = Mathf.CeilToInt(publicCommands.Count() / (float)commandsPerPage);

            page--; //Pages start from 1
            if (page < 0)
            {
                page = 0;
            }

            StringBuilder stringBuilder1 = new StringBuilder();
            string header = pages == 1 && page == 0 ? $"<color=green>Available Commands:</color>" : $"<color=green>Available Commands:</color> Page {page + 1} : {pages}";
            stringBuilder1.AppendLine(header);
            for (int i = 0; i < commandsPerPage; i++)
            {
                int index = i + page * commandsPerPage;
                if (i + page * commandsPerPage >= publicCommands.Count())
                    break;
                PublicCommand command = publicCommands.ElementAt(index);
                stringBuilder1.AppendLine($"!{command.CommandAliases()[0]} - {command.Description()}");

            }
            stringBuilder1.AppendLine("Use <color=green>!help <command></color> for details about a specific command");
            Messaging.Echo(stringBuilder1.ToString(), senderId);
        }*/
        }
    }
}
