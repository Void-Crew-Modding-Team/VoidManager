﻿using System;
using System.Linq;
using UnityEngine;
using static UnityEngine.GUILayout;
using static VoidManager.Plugin.Bindings;


namespace VoidManager.CustomGUI
{
    class VManSettings : ModSettingsMenu
    {
        public override string Name() => MyPluginInfo.PLUGIN_NAME;

        string SizeX = string.Empty;
        string SizeY = string.Empty;
        string ModListSizeX = string.Empty;
        string SizeErrString = string.Empty;

        public override void OnOpen()
        {
            SizeX = MenuWidth.Value.ToString();
            SizeY = MenuHeight.Value.ToString();
            ModListSizeX = MenuListWidth.Value.ToString();
        }


        public override void Draw()
        {
            if (Button("Debug Mode: " + (DebugMode.Value ? "Enabled" : "Disabled")))
            {
                DebugMode.Value = !DebugMode.Value;

                /*if (!VManConfig.DebugMode)
                {
                    PLInGameUI.Instance.CurrentVersionLabel.text = PulsarModLoader.Patches.GameVersion.Version;
                }*/
            }

            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            BeginHorizontal();
            {
                Label($"ModInfoTextAnchor: {Plugin.Bindings.ModInfoTextAnchor.Value.ToString()}");

                if (Button("<"))
                    ModInfoTextAnchor.Value = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)ModInfoTextAnchor.Value - 1).First();
                if (Button(">"))
                    ModInfoTextAnchor.Value = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().SkipWhile(e => (int)e != (int)ModInfoTextAnchor.Value).Skip(1).First();
            }
            EndHorizontal();

            if (Button("Reset to default")) SetDefault();


            //Size Settings
            HorizontalSlider(0, 100, 100);
            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            Label("ModManager Size");

            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleRight;
            BeginHorizontal();
            Label("Width:");
            SizeX = TextField(SizeX);
            EndHorizontal();
            BeginHorizontal();
            Label("Height:");
            SizeY = TextField(SizeY);
            EndHorizontal();
            BeginHorizontal();
            Label("Modlist Scrollbar Width:");
            ModListSizeX = TextField(ModListSizeX);
            EndHorizontal();

            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            if (SizeErrString != string.Empty)
            {
                Label($"<color=red>{SizeErrString}</color>");
            }

            if (Button("Apply Size"))
            {
                if (!float.TryParse(SizeX, out float X) || !float.TryParse(SizeY, out float Y) || !float.TryParse(ModListSizeX, out float MLx))
                {
                    SizeErrString = "Size values are not numbers";
                }
                else
                {
                    if (X < .3 || Y < .3 || MLx < .2)
                    {
                        SizeErrString = "Size Values cannot be smaller than .3, .3, and .2";
                    }
                    else
                    {
                        MenuHeight.Value = Y;
                        MenuWidth.Value = X;
                        MenuListWidth.Value = MLx;
                        SizeErrString = string.Empty;
                        GUIMain.Instance.updateWindowSize();
                    }
                }
            }

            //Cursor Unlock Toggle
            HorizontalSlider(0, 100, 100);
            if (Button($"Unlock Cursor While Open: {(MenuUnlockCursor.Value ? "Enabled" : "Disabled")}"))
            {
                MenuUnlockCursor.Value = !MenuUnlockCursor.Value;
            }
        }
    }
}