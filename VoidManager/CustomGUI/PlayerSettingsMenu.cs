﻿using Photon.Realtime;

namespace VoidManager.CustomGUI
{
    /// <summary>
    /// PlayerSettingsMenu, called by VoidManager's ModManager (F5 menu). VoidManager automatically finds and instatiates all classes inheriting from PlayerSettingsMenu.
    /// </summary>
    public abstract class PlayerSettingsMenu
    {
        /// <summary>
        /// PSM display name in ModManager Player details
        /// </summary>
        /// <returns></returns>
        public virtual string Name() => string.Empty;

        /// <summary>
        /// GUI Frame update call. Use UnityEngine.GUILayout, referencing UnityEngine.IMGUIModule.
        /// </summary>
        public abstract void Draw(Player selectedPlayer);


        internal VoidPlugin MyVoidPlugin;
    }
}