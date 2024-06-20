using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoidManager.Chat.Router;
using VoidManager.Utilities;

namespace VoidManager.Chat
{
    class HelpCommand : ChatCommand
    {
        private List<Argument> cachedArguments;

        public override string[] CommandAliases()
            => new string[] { "help", "?" };

        public override string Description()
            => "Displays help text for a command, or the list of commands if none specified.";

        public override string[] UsageExamples()
            => new List<string>(base.UsageExamples()).Concat(new string[] { $"/{CommandAliases()[0]} help" }).ToArray();

        public override List<Argument> Arguments()
        {
            if (cachedArguments != null)
            {
                return cachedArguments;
            }
            else
            {
                List<string> argumentList = new();
                foreach (ChatCommand command in CommandHandler.GetCommands())
                {
                    foreach (string alias in command.CommandAliases())
                    {
                        argumentList.Add(alias);
                    }
                }
                cachedArguments = new List<Argument>() { new Argument(argumentList.ToArray()) };

                return cachedArguments;
            }
        }

        public override void Execute(string arguments)
        {
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                if (arguments[0] == '/')
                {
                    arguments = arguments.Substring(1);
                }
                ChatCommand cmd = CommandHandler.GetCommand(arguments.Split(' ')[0].ToLower());
                StringBuilder stringBuilder = new();
                if (cmd != null)
                {
                    stringBuilder.AppendLine($"<color=green>/{cmd.CommandAliases()[0]}</color> - {cmd.Description()}");// <color=#ff6600ff>[{name}]</color>");
                    stringBuilder.AppendLine($"Aliases: /{string.Join($", /", cmd.CommandAliases())}");
                    stringBuilder.AppendLine($"Usage: {cmd.UsageExamples()[0]}");
                    for (int i = 1; i < cmd.UsageExamples().Length; i++)
                    {
                        stringBuilder.AppendLine($"       {cmd.UsageExamples()[i]}");
                    }
                }
                else
                {
                    stringBuilder.AppendLine($"Command /{arguments} not found");
                }
                Messaging.Echo(stringBuilder.ToString());
            }
            else
            {
                IOrderedEnumerable<ChatCommand> commands = CommandHandler.GetCommands();

                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("<color=green>Command List:</color> :");
                foreach (ChatCommand command in commands)
                {
                    stringBuilder.AppendLine($"<color=#3f7fff>/{command.CommandAliases()[0]}</color> - {command.Description()}");

                }
                stringBuilder.AppendLine("Use <color=green>/help <command></color> for details about a specific command");
                Messaging.Echo(stringBuilder.ToString());
            }
        }
    }
}
