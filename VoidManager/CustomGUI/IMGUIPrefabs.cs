using Photon.Pun;
using System;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace VoidManager.CustomGUI
{
    //Consider changing name and location of IMGUIPrefabs class. This might not be the ideal place for it, or it might be better with a different name so we can fit more things such as the public methods in GUIMain

    /// <summary>
    /// Prefabs for IMGUI displays
    /// </summary>
    public class IMGUIPrefabs
    {
        /// <summary>
        /// GUIStyle to make buttons take as little space as possible.
        /// </summary>
        public static readonly GUIStyle ButtonMinSizeStyle;

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
    }
}
