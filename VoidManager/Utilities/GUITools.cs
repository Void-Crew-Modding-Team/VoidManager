﻿using Photon.Pun;
using System;
using UnityEngine;
using VoidManager.CustomGUI;
using static UnityEngine.GUILayout;

namespace VoidManager.Utilities
{
    /// <summary>
    /// Prefabs for IMGUI displays
    /// </summary>
    public class GUITools
    {
        /// <summary>
        /// GUIStyle to make buttons take as little space as possible.
        /// </summary>
        public static GUIStyle ButtonMinSizeStyle;

        internal static string keybindToChange = null;

        /// <summary>
        /// A Label, textfield, and apply button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="settingvalue"></param>
        /// <param name="ApplyFunc"></param>
        public static void SettingGroup(string label, ref string settingvalue, Action ApplyFunc)
        {
            Label(label);
            BeginHorizontal();
            settingvalue = TextField(settingvalue);
            if (PhotonNetwork.IsMasterClient && Button("Apply", ButtonMinSizeStyle)) //Block apply button, but clients can still read current lobby settings
            {
                ApplyFunc?.Invoke();
            }
            EndHorizontal();
        }

        /// <summary>
        /// Creates a button that when pressed allows the user to enter a new keybind. Returns true when a keybind is set.
        /// </summary>
        /// <param name="buttonName"></param>
        /// <param name="keybind"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool ChangeKeybindButton(string buttonName, ref KeyCode keybind)
        {
            if (string.IsNullOrEmpty(buttonName)) throw new ArgumentException("buttonName must not be null or empty");

            bool keybindChanged = false;
            bool changeKeybind = keybindToChange == buttonName;
            if (changeKeybind)
            {
                Event e = Event.current;
                if (e.isKey)
                {
                    if (e.keyCode == KeyCode.Escape)
                    {
                        keybind = KeyCode.None;
                        keybindChanged = true;
                        keybindToChange = null;
                    }
                    else
                    {
                        keybind = e.keyCode;
                        keybindChanged = true;
                        keybindToChange = null;
                    }
                }
            }

            if (Button(changeKeybind ? $"{buttonName}: ..... Press ESC to remove" : $"{buttonName}: ({keybind})"))
            {
                keybindToChange = buttonName;
            }
            return keybindChanged;
        }

        /// <summary>
        /// Draws a button with selected highlight when 'selected' is true.
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="selected">Selected Highlight</param>
        /// <returns></returns>
        public static bool DrawButtonSelected(string text, bool selected)
        {
            if (selected)
            {
                bool returnvalue = Button(text, GUIMain._SelectedButtonStyle);
                return returnvalue;
            }
            else
            {
                return Button(text);
            }
        }
    }
}
