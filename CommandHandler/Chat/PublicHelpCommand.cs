using CommandHandler.Chat.Router;
using CommandHandler.Utilities;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommandHandler.Chat
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

        public override void Execute(string arguments, int SenderID)
        {
            return;
            /*if (PhotonNetwork.IsMasterClient)
            { 
                IOrderedEnumerable<PublicCommand> publicCommands = Router.CommandHandler.GetPublicCommands();

                if (publicCommands.Count() <= 1)
                {
                    return;
                }

                PLPlayer sender = PLServer.Instance.GetPlayerFromPlayerID(SenderID);
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
                        if (cmd != null)
                        {
                            Messaging.Echo(sender, $"[&%~[C3 !{cmd.CommandAliases()[0]} ]&%~] - {cmd.Description()}");// <color=#ff6600ff>[{name}]</color>");
                            Messaging.Echo(sender, $"Aliases: !{string.Join($", !", cmd.CommandAliases())}");
                            Messaging.Echo(sender, $"Usage: {cmd.UsageExamples()[0]}");
                            for (int i = 1; i < cmd.UsageExamples().Length; i++)
                            {
                                Messaging.Echo(sender, $"       {cmd.UsageExamples()[i]}");
                            }
                        }
                        else
                        {
                            Messaging.Echo(sender, $"Command !{arguments} not found");
                        }
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

                string header = pages == 1 && page == 0 ? $"[&%~[C3 Available Commands: ]&%~]" : $"[&%~[C3 Available Commands: ]&%~] Page {page + 1} : {pages}";
                Messaging.Echo(sender, header);
                for (int i = 0; i < commandsPerPage; i++)
                {
                    int index = i + page * commandsPerPage;
                    if (i + page * commandsPerPage >= publicCommands.Count())
                        break;
                    PublicCommand command = publicCommands.ElementAt(index).Item1;
                    Messaging.Echo(sender, $"!{command.CommandAliases()[0]} - {command.Description()}");

                }
                Messaging.Echo(sender, "Use [&%~[C2 !help <command> ]&%~] for details about a specific command");
            }*/
        }
    }
}
