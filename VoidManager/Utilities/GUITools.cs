using BepInEx.Configuration;
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
        /// <param name="buttonName">Must not be null or empty. Should be unique.</param>
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
        /// Creates a button that when pressed allows the user to enter a new keybind.
        /// </summary>
        /// <param name="buttonName">Must not be null or empty. Should be unique.</param>
        /// <param name="keybindConfig"></param>
        /// <returns>true when a keybind is set, false otherwise</returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool DrawChangeKeybindButton(string buttonName, ref ConfigEntry<KeyCode> keybindConfig)
        {
            KeyCode temp = keybindConfig.Value;
            bool result = DrawChangeKeybindButton(buttonName, ref temp);
            if (result)
            {
                keybindConfig.Value = temp;
            }
            return result;
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
        /// Creates a normal Toggle with button-like behaviour.
        /// </summary>
        /// <param name="label">Text</param>
        /// <param name="isOn">Ref to a bool which tracks current value.</param>
        /// <returns>true on value change, false otherwise</returns>
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
        /// Creates a normal Toggle with button-like behaviour.
        /// </summary>
        /// <param name="label">Text</param>
        /// <param name="config">Ref to a bool config entry which tracks current value.</param>
        /// <returns>true on value change, false otherwise</returns>
        public static bool DrawCheckbox(string label, ref ConfigEntry<bool> config)
        {
            bool result = Toggle(config.Value, label);
            if (result != config.Value)
            {
                config.Value = result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a label, text field, apply button, and reset button<br/>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value">The value currently in the text field</param>
        /// <param name="defaultValue">The value after the reset button is pressed</param>
        /// <param name="minWidth">minimum width of the input text field</param>
        /// <returns>true when the apply or reset button is pressed, false otherwise</returns>
        public static bool DrawTextField(string label, ref string value, string defaultValue, float minWidth = 80)
        {
            bool changed = false;
            BeginHorizontal();
            Label($"{label}: ");
            value = TextField(value, MinWidth(minWidth));
            FlexibleSpace();
            if (Button("Apply"))
            {
                changed = true;
            }
            if (Button("Reset"))
            {
                value = defaultValue;
                changed = true;
            }
            EndHorizontal();
            return changed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="label"></param>
        /// <param name="color"></param>
        /// <param name="showAlpha"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>true on value change, false otherwise</returns>
        public static bool DrawColorPicker(Rect rect, string label, ref Color color, bool showAlpha = true, float min = 0, float max = 20)
        {
            bool changed = false;
            float tempComponent;

            BeginArea(rect, "", "Box");
            BeginHorizontal();
            Label(label);
            EndHorizontal();
            BeginHorizontal();
            BeginVertical("Box");

            BeginHorizontal();
            Label("R", Width(10));
            tempComponent = HorizontalSlider(color.r, min, max);
            if (tempComponent != color.r)
            {
                color.r = tempComponent;
                changed = true;
            }
            EndHorizontal();

            BeginHorizontal();
            Label("G", Width(10));
            tempComponent = HorizontalSlider(color.g, min, max);
            if (tempComponent != color.g)
            {
                color.g = tempComponent;
                changed = true;
            }
            EndHorizontal();

            BeginHorizontal();
            Label("B", Width(10));
            tempComponent = HorizontalSlider(color.b, min, max);
            if (tempComponent != color.b)
            {
                color.b = tempComponent;
                changed = true;
            }
            EndHorizontal();

            if (showAlpha)
            {
                BeginHorizontal();
                Label("A", Width(10));
                tempComponent = HorizontalSlider(color.a, min, Mathf.Min(max, 1));
                if (tempComponent != color.a)
                {
                    color.a = tempComponent;
                    changed = true;
                }
                EndHorizontal();
            }

            EndVertical();
            BeginVertical("Box", new GUILayoutOption[] { Width(44), Height(44) });
            Color temp = GUI.color;
            float scale = Mathf.Max(color.maxColorComponent, 1);
            GUI.color = new Color(color.r / scale, color.g / scale, color.b / scale, color.a);
            Label(new Texture2D(60, 40));
            GUI.color = temp;
            EndVertical();
            EndHorizontal();
            Label($"{color.r:0.00}, {color.g:0.00}, {color.b:0.00}" + (showAlpha ? $", {color.a:0.00}" : ""));
            EndArea();

            return changed;
        }
    }
}
