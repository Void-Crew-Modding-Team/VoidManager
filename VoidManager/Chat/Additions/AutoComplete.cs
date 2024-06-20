using BepInEx;
using Gameplay.Chat;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI.Chat;
using UnityEngine;
using UnityEngine.UIElements;
using VoidManager.Chat.Router;
using VoidManager.Utilities;

namespace VoidManager.Chat.Additions
{
    internal class AutoComplete
    {
        private static readonly FieldInfo chatUIField = AccessTools.Field(typeof(TextChat), "_chatUI");
        private static readonly FieldInfo inputFieldField = AccessTools.Field(typeof(TextChatVE), "inputField");

        private static List<Argument> chatCommandsCache = null;
        private static List<Argument> ChatCommands { get => chatCommandsCache ??= GetChatCommands(); }

        private static List<string> playerNames = new();

        private static bool chatOpen = false;

        internal static void Tick(object sender, EventArgs e)
        {
            if (!chatOpen) return;

            if (UnityInput.Current.GetKeyDown(KeyCode.Tab))
            {
                TextChatVE chatUI = (TextChatVE)chatUIField.GetValue(TextChat.Instance);
                TextField inputField = (TextField)inputFieldField.GetValue(chatUI);
                string text = inputField.text;
                if (text[0] != '/' && text[0] != '!') return;

                text = Complete(text);

                inputField.value = text;
                inputField.cursorIndex = text.Length;
                inputField.selectIndex = text.Length;
            }
        }

        private static string Complete(string text)
        {
            List<Argument> arguments;
            if (text[0] == '/')
                arguments = ChatCommands;
            else
                arguments = PublicCommandHandler.publicCommands;
            string[] split = text.Substring(1).Split(' ');

            //Traverse the argument tree
            for (int i = 0; i < split.Length - 1; i++)
            {
                bool containsReplace(string name) {
                    if (name[0] != '%')
                    {
                        return name.Equals(split[i], StringComparison.CurrentCultureIgnoreCase);
                    }
                    else
                    {
                        return name switch
                        {
                            "%player_name" => playerNames.Contains(split[i], StringComparer.CurrentCultureIgnoreCase),
                            "%number" => double.TryParse(split[i], out _),
                            "%integer" => long.TryParse(split[i], out _),
                            _ => false,
                        };
                    }
                }
                //Argument argument = arguments.Find(argument => argument.names.Contains(split[i], StringComparer.CurrentCultureIgnoreCase) || (argument.playerName && playerNames.Contains(split[i], StringComparer.CurrentCultureIgnoreCase)));
                Argument argument = arguments.FirstOrDefault(argument => argument.names.Any(containsReplace));
                if (argument == null)
                {
                    Messaging.Notification($"No match found for \"{split[i]}\"");
                    return text;
                }
                else
                {
                    arguments = argument.arguments;
                }
            }

            string partial = split[split.Length - 1];
            List<string> matches = new();
            List<string> displayOnly = new();

            //Match the last argument
            foreach (Argument argument in arguments)
            {
                foreach (string argumentName in argument.names)
                {
                    if (argumentName[0] == '%')
                    {
                        switch(argumentName)
                        {
                            case "%player_name":
                                foreach (string playerName in playerNames)
                                {
                                    if (playerName.StartsWith(partial, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        matches.Add(playerName);
                                    }
                                }
                                goto default;
                            default:
                                displayOnly.Add(argumentName);
                                break;
                        }
                    }
                    else if (argumentName.StartsWith(partial, StringComparison.CurrentCultureIgnoreCase))
                    {
                        matches.Add(argumentName);
                    }
                }
            }

            if (matches.Count + displayOnly.Count == 0)
            {
                Messaging.Notification($"No partial match found for \"{partial}\"");
                return text;
            }
            else if (matches.Count == 1)
            {
                split[split.Length - 1] = matches[0];
                return text[0] + string.Join(" ", split);
            }
            else
            {
                Messaging.Notification(string.Join("\n", matches.Union(displayOnly)));

                if (matches.Count == 0) return text;

                string partialMatch = matches[0];
                foreach(string match in matches)
                {
                    if (match.Length < partialMatch.Length)
                        partialMatch = partialMatch.Substring(0, match.Length);
                    for (int i = 0; i < partialMatch.Length; i++)
                    {
                        if (match.ToLower()[i] != partialMatch.ToLower()[i])
                        {
                            partialMatch = partialMatch.Substring(0, i);
                            break;
                        }
                    }
                    if (partialMatch.Length == 0) break;
                }
                if (partialMatch.Length > 0)
                {
                    split[split.Length - 1] = partialMatch;
                    return text[0] + string.Join(" ", split);
                }
                else
                {
                    return text;
                }
            }
        }

        internal static void OnChatOpened(object sender, EventArgs e)
        {
            chatOpen = true;
        }

        internal static void OnChatClosed(object sender, EventArgs e)
        {
            chatOpen = false;
        }

        private static List<Argument> GetChatCommands()
        {
            List<Argument> commands = new();
            foreach (ChatCommand command in CommandHandler.GetCommands())
            {
                commands.Add(new Argument(command.CommandAliases(), command.Arguments()));
            }
            return commands;
        }

        internal static void RefreshPlayerList(object sender, EventArgs e)
        {
            List<string> players = new();
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                players.Add(player.NickName);
            }
            playerNames = players;
        }
    }
}
