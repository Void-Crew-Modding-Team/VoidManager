using BepInEx;
using Gameplay.Chat;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UI.Chat;
using UnityEngine;
using UnityEngine.UIElements;

namespace VoidManager.Chat.Additions
{
    internal class ChatHistory
    {
        private static readonly FieldInfo chatUIField = AccessTools.Field(typeof(TextChat), "_chatUI");
        private static readonly FieldInfo inputFieldField = AccessTools.Field(typeof(TextChatVE), "inputField");

        private const int chatHistoryMaxLength = 100;
        private static readonly List<string> chatHistory = new();
        private static int historyIndex = -1;

        private static bool chatOpen = false;

        internal static void Tick(object sender, EventArgs e)
        {
            if (!chatOpen) return;

            bool changed = false;
            if (UnityInput.Current.GetKeyDown(KeyCode.UpArrow))
            {
                historyIndex++;
                if (historyIndex >= chatHistory.Count) historyIndex = chatHistory.Count - 1;
                changed = true;
            }
            if (UnityInput.Current.GetKeyDown(KeyCode.DownArrow))
            {
                historyIndex--;
                if (historyIndex < -1) historyIndex = -1;
                changed = true;
            }

            if (changed)
            {
                TextChatVE chatUI = (TextChatVE)chatUIField.GetValue(TextChat.Instance);
                TextField inputField = (TextField)inputFieldField.GetValue(chatUI);

                if (historyIndex >= 0)
                {
                    inputField.value = chatHistory[historyIndex];
                    inputField.cursorIndex = inputField.text.Length;
                    inputField.selectIndex = inputField.text.Length;
                }
                else
                {
                    inputField.value = "";
                }
            }
        }

        internal static void AddToHistory(string message)
        {
            if (chatHistory.Contains(message))
            {
                chatHistory.Remove(message);
            }

            if (chatHistory.Count >= chatHistoryMaxLength)
            {
                chatHistory.RemoveAt(chatHistoryMaxLength - 1);
            }

            chatHistory.Insert(0, message);
        }

        internal static void OnChatOpened(object sender, EventArgs e)
        {
            chatOpen = true;
        }

        internal static void OnChatClosed(object sender, EventArgs e)
        {
            chatOpen = false;
            historyIndex = -1;
        }
    }
}
