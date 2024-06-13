using Photon.Pun;
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
        /// Creates a button that when pressed allows the user to enter a new keybind.
        /// </summary>
        /// <param name="buttonName"></param>
        /// <param name="keybind"></param>
        /// <returns>true when a keybind is set, false otherwise</returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool DrawChangeKeybindButton(string buttonName, ref KeyCode keybind)
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

        /// <summary>
        /// Creates a checkbox with a label to the right
        /// </summary>
        /// <param name="label"></param>
        /// <param name="isOn"></param>
        /// <returns>true if the value is changed, false otherwise</returns>
        public static bool DrawCheckbox(string label, ref bool isOn)
        {
            bool result = Toggle(isOn, label);
            if (result != isOn)
            {
                isOn = result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a box with a label, sliders to set the color, a color preview, and the rgba values
        /// </summary>
        /// <param name="rect">The location and size of the box. (8, 58, 480, 160) is a good start</param>
        /// <param name="label"></param>
        /// <param name="color"></param>
        /// <param name="showAlpha">Should the alpha channel be shown</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static void DrawColorPicker(Rect rect, string label, ref Color color, bool showAlpha = true, float min = 0, float max = 20)
        {
            BeginArea(rect, "", "Box");
            BeginHorizontal();
            Label(label);
            EndHorizontal();
            BeginHorizontal();
            BeginVertical("Box");

            BeginHorizontal();
            Label("R", Width(10));
            float r = HorizontalSlider(color.r, min, max);
            EndHorizontal();

            BeginHorizontal();
            Label("G", Width(10));
            float g = HorizontalSlider(color.g, min, max);
            EndHorizontal();

            BeginHorizontal();
            Label("B", Width(10));
            float b = HorizontalSlider(color.b, min, max);
            EndHorizontal();

            float a = color.a;
            if (showAlpha)
            {
                BeginHorizontal();
                Label("A", Width(10));
                a = HorizontalSlider(color.a, min, Mathf.Min(max, 1));
                EndHorizontal();
            }

            EndVertical();
            BeginVertical("Box", new GUILayoutOption[] { Width(44), Height(44) });
            Color temp = GUI.color;
            float scale = Mathf.Max(color.maxColorComponent, 1);
            GUI.color = new Color(color.r / scale, color.g / scale, color.b / scale, showAlpha ? color.a : 1);
            Label(new Texture2D(60, 40));
            GUI.color = temp;
            EndVertical();
            EndHorizontal();
            Label($"{color.r:0.00}, {color.g:0.00}, {color.b:0.00}" + (showAlpha ? $", {color.a:0.00}" : ""));
            EndArea();
        }
    }
}
