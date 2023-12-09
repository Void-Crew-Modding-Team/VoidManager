using CommandHandler.Chat.Router;
using CommandHandler.Utilities;

namespace CommandHandler.Chat
{
    internal class EchoCommand : ChatCommand
    {
        public override string[] CommandAliases()
            => new string[] { "echo", "e" };

        public override string Description()
            => "Repeats the input text back through the chat box.";

        public override string[] UsageExamples()
            => new string[] { $"/{CommandAliases()[0]} <text>" };

        public override void Execute(string arguments)
        {
            Messaging.Echo($"Echo: {arguments}");
        }
    }
}
