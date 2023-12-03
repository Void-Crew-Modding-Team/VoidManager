using HarmonyLib;

namespace CommandHandler
{
    public abstract class ChatCommand
    {
        public abstract string[] CommandAliases();
        public abstract string Description();
        public abstract void Execute(string arguments);
    }
}
