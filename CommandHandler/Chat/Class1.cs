using CommandHandler.Chat.Router;
using CommandHandler.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandHandler.Chat
{
    internal class Class1 : PublicCommand
    {
        public override string[] CommandAliases()
            => new string[] { "echo", "e" };

        public override string Description()
            => "Repeats the input text back through the chat box.";

        public override string[] UsageExamples()
            => new string[] { $"/{CommandAliases()[0]} <text>" };

        public override void Execute(string arguments, int player)
        {
            Messaging.Echo($"Echo: {arguments}");
        }
    }
}
