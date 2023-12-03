using CG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandHandler.Utilities
{
    public class Logger
    {
        /// <summary>
        /// Different Log Methods
        /// </summary>
        public enum LogType
        {
            GameLog,
            InfoLog,
            MessageLog,
            WarningLog,
            FatalLog
        }

        /// <summary>
        /// Creates a log based on the supplied LogType
        /// </summary>
        /// <param name="message">Message to be logged</param>
        /// <param name="logType">Logging method to be used</param>
        public static void Info(string message, LogType logType = LogType.InfoLog)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            string result = $"{FormatLog(assembly, message)}";
            switch (logType )
            {
                case LogType.GameLog: Debug.Log(result); break;
                case LogType.InfoLog: Plugin.Log.LogInfo(result); break;
                case LogType.MessageLog: Plugin.Log.LogMessage(result); break;
                case LogType.WarningLog: Plugin.Log.LogWarning(result); break;
                case LogType.FatalLog: Plugin.Log.LogFatal(result); break;
            }
        }

        /// <summary>
        /// Formats the log response into '[time] [modname] [message]'
        /// </summary>
        /// <param name="assembly">The assembly calling the original method</param>
        /// <param name="message">The message to format</param>
        /// <returns>Formatted message [time] [modname] [message] </returns>
        private static string FormatLog(Assembly assembly, string message)
        {
            /*Type pluginType = assembly.GetType("MyPluginInfo");
            List<FieldInfo> field = pluginType.GetFields().ToList();
            */
            string result = $"[{DateTime.Now.ToString("HH:mm:ss")}]";
            /*if (field != null && field.IsLiteral && !field.IsInitOnly)
            {
                object obj = field.GetValue(null);
                result += $" {obj}";
            }
            */
            result += $" [{assembly.FullName.Split(',')[0]}]";
            return $"{result} {message}";
        }
    }
}
