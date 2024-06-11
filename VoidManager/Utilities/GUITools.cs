using Photon.Pun;
using System;
using UnityEngine;
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

        internal static string keybindToChange = string.Empty;

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
                        keybindToChange = string.Empty;
                    }
                    else
                    {
                        keybind = e.keyCode;
                        keybindChanged = true;
                        keybindToChange = string.Empty;
                    }
                }
            }

            if (Button(changeKeybind ? $"{buttonName}: ..... Press ESC to remove" : $"{buttonName}: ({keybind})"))
            {
                keybindToChange = buttonName;
            }
            return keybindChanged;
        }
    }
}
