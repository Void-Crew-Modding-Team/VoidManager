using Gameplay.Chat;
using System.Reflection;
using UI.Ping;
using static System.Net.Mime.MediaTypeNames;

namespace VoidManager.Utilities
{
    public class Messaging
    {
        /// <summary>
        /// Inserts a line to text chat with reference to the executing assembly.
        /// </summary>
        /// <param name="message"></param>
        public static void Notification(string message)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            TextChat.Instance.AddLog(new Log($"{assembly.FullName.Split(',')[0]}", message));
        }

        /// <summary>
        /// Inserts a line to text chat.
        /// </summary>
        /// <param name="message"></param>
        public static void Echo(string message)
        {
            TextChat.Instance.AddLog(new Log($"", message));
        }
    }
}
