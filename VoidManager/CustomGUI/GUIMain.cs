using BepInEx;
using CG.Input;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoidManager.MPModChecks;
using static UnityEngine.GUILayout;

namespace VoidManager.CustomGUI
{
    class GUIMain : MonoBehaviour, IShowCursorSource, IInputActionMapRequest
    {
        public static GUIMain Instance { get; internal set; }
        GameObject Background;
        GameObject MMCanvas;
        UnityEngine.UI.Image Image;
        public bool GUIActive = false;
        Rect Window;
        byte Tab = 0;

        List<VoidPlugin> mods = new();
        VoidPlugin selectedMod = null;

        List<VoidPlugin> NonVManMods = new();

        Rect ModListArea;
        Vector2 ModListScroll = Vector2.zero;

        Rect ModInfoArea;
        Vector2 ModInfoScroll = Vector2.zero;

        Rect PlayerListArea;
        Vector2 PlayerListScroll = Vector2.zero;
        Player selectedPlayer;

        Rect PlayerModInfoArea;
        Vector2 PlayerModInfoScroll = Vector2.zero;


        Rect ModSettingsArea;
        Vector2 ModSettingsScroll = Vector2.zero;
        List<ModSettingsMenu> settings = new();
        ModSettingsMenu selectedSettings;

        internal void updateWindowSize()
        {
            float Height = BepinPlugin.Bindings.MenuHeight.Value;
            float Width = BepinPlugin.Bindings.MenuWidth.Value;
            float ModlistWidth = BepinPlugin.Bindings.MenuListWidth.Value;
            float PlayerlistWidth = BepinPlugin.Bindings.PlayerListWidth.Value;

            Window = new Rect((Screen.width * .5f - ((Screen.width * Width) / 2)), Screen.height * .5f - ((Screen.height * Height) / 2), Screen.width * Width, Screen.height * Height);
            ModListArea = new Rect(6, 43, Window.width * ModlistWidth, Screen.height * Height - 45);
            ModInfoArea = new Rect(ModListArea.width + 15, 43, (Screen.width * Width - (ModListArea.width + 11)) - 10, Screen.height * Height - 45);
            PlayerListArea = new Rect(6, 43, Window.width * PlayerlistWidth, Screen.height * Height - 45);
            PlayerModInfoArea = new Rect(PlayerListArea.width + 15, 43, (Screen.width * Width - (PlayerListArea.width + 11)) - 10, Screen.height * Height - 45);
            ModSettingsArea = new Rect(6, 43, Screen.width * Width - 12, Screen.height * Height - 45);
        }

        internal GUIMain()
        {
            Instance = this;
            MMCanvas = new GameObject("ModManagerCanvas", new Type[] { typeof(Canvas) });
            Canvas canvasComponent = MMCanvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 1000;
            canvasComponent.transform.SetAsLastSibling();
            DontDestroyOnLoad(MMCanvas);

            updateWindowSize();
            settings.Add(new VManSettings());

            //Background image to block mouse clicks passing IMGUI
            Background = new GameObject("GUIMainBG", new Type[] { typeof(GraphicRaycaster) });
            Image = Background.AddComponent<UnityEngine.UI.Image>();
            Image.color = Color.clear;
            Background.transform.SetParent(MMCanvas.transform);
            Background.SetActive(false);
        }

        void Awake()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }

        void Update()
        {
            if (BepinPlugin.Bindings.OpenMenu.IsDown())
            {
                GUIActive = !GUIActive;
                if (GUIActive)
                {
                    GUIOpen();
                }
                else
                {
                    GUIClose();
                }
            }
        }

        void GUIOpen()
        {
            if (selectedSettings != null) //Menu Opening and MM selected
            {
                selectedSettings.OnOpen();
            }
            GUIToggleCursor(true);
            Background.SetActive(true);
        }

        void GUIClose()
        {
            if (selectedSettings != null) //Menu Closing and MM Selected
            {
                selectedSettings.OnClose();
            }
            GUIToggleCursor(false);
            Background.SetActive(false);
        }

        void OnGUI()
        {
            if (GUIActive)
            {
                GUI.skin = ChangeSkin();
                Window = GUI.Window(999910, Window, WindowFunction, "ModManager");

                //float y = Window.center.y * 2 * -1;
                Image.rectTransform.position = new Vector3(Window.center.x, (Window.center.y * -1) + Screen.height, 0);
                Image.rectTransform.sizeDelta = Window.size;
            }
        }

        void WindowFunction(int WindowID)
        {

            BeginHorizontal(); // TAB Start
            {
                if (Button("Mod Info"))
                    Tab = 0;
                if (Button("Mod Settings"))
                    Tab = 1;
                if (Button("Player List"))
                    Tab = 2;
            }
            EndHorizontal(); // TAB End
            switch (Tab)
            {
                #region ModList and ModInfo
                case 0:
                    BeginArea(ModListArea);
                    {
                        ModListScroll = BeginScrollView(ModListScroll);
                        {
                            if (Button("VoidManager"))
                            {
                                selectedMod = null;
                            }
                            foreach (VoidPlugin vp in mods)
                            {
                                DrawModButton(vp);
                            }
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            Label("<color=yellow>Non-VoidManager Mods</color>");
                            foreach (VoidPlugin vp in NonVManMods)
                            {
                                DrawModButton(vp);
                            }
                            Label("Overall MPType: " + GetColoredMPTypeText(MPModCheckManager.Instance.HighestLevelOfMPMods));
                        }
                        EndScrollView();
                    }
                    EndArea();
                    GUI.skin.label.alignment = BepinPlugin.Bindings.ModInfoTextAnchor.Value;
                    BeginArea(ModInfoArea);
                    {
                        ModInfoScroll = BeginScrollView(ModInfoScroll);
                        {
                            if (selectedMod != null)
                            {
                                BepInPlugin bepInPlugin = selectedMod.BepinPlugin.Metadata;
                                Label($"Author: {selectedMod.Author}");
                                Label($"Name: {bepInPlugin.Name}");
                                Label($"Version: {bepInPlugin.Version}");
                                if (selectedMod.Description != string.Empty)
                                    Label($"Description: {selectedMod.Description}");
                                Label($"MPRequirement: {GetTextForMPType(selectedMod.MPType)}");
                            }
                            else
                            {
                                //VoidManager about page when no mod selected.
                                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                Label($"VoidManager - BepInEx Plugin Manager for Void Crew.");
                                Label("Provides APIs to developers and multiplayer mod management.");
                                Label($"Version: {MyPluginInfo.PLUGIN_VERSION}");
                                Label($"\n\nDeveloped by Mest and Dragon");
                                Label($"Based on the 'Pulsar Mod Loader' developed by Tom Ritcher");
                                BeginHorizontal();
                                FlexibleSpace();
                                if (Button("Github"))
                                    Application.OpenURL("https://github.com/Void-Crew-Modding-Team/VoidManager");
                                if (Button("Discord"))
                                    Application.OpenURL("https://discord.gg/4QhRRBWsJz");
                                FlexibleSpace();
                                EndHorizontal();
                                if(Button("VoidManager Settings"))
                                {
                                    OpenSettingsMenu(settings[0]);
                                }
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    break;
                #endregion
                #region ModSettings
                case 1:
                    GUI.skin.label.alignment = BepinPlugin.Bindings.ModInfoTextAnchor.Value;
                    BeginArea(ModSettingsArea);
                    {
                        ModSettingsScroll = BeginScrollView(ModSettingsScroll);
                        {
                            if (selectedSettings == null)
                            {
                                foreach (ModSettingsMenu menu in settings)
                                {
                                    if (Button(menu.Name()))
                                    {
                                        menu.OnOpen();
                                        selectedSettings = menu;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (Button("Back"))
                                {
                                    selectedSettings.OnClose();
                                    selectedSettings = null;
                                }
                                else
                                {
                                    selectedSettings.Draw();
                                }
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    break;
                #endregion
                #region Player List
                case 2:
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    BeginArea(PlayerListArea);
                    {
                        PlayerListScroll = BeginScrollView(PlayerListScroll);
                        {
                            foreach (Player player in PhotonNetwork.PlayerList)
                            {
                                if (player.IsLocal)
                                {
                                    continue;
                                }
                                if (Button(player.NickName))
                                {
                                    selectedPlayer = player;
                                }
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    BeginArea(PlayerModInfoArea);
                    {
                        PlayerModInfoScroll = BeginScrollView(PlayerModInfoScroll);
                        {
                            if (selectedPlayer != null)
                            {
                                Label($"Player: {selectedPlayer.NickName} {(selectedPlayer.IsMasterClient ? "(Host)" : string.Empty)}");

                                DrawPlayerModList(selectedPlayer);
                            }
                        }
                        EndScrollView();
                    }
                    EndArea();
                    break;
                    #endregion
            }
            GUI.DragWindow();
        }

        internal static GUISkin _cachedSkin;
        private static readonly Color32 _classicMenuBackground = new Color32(32, 32, 32, 255);
        private static readonly Color32 _classicButtonBackground = new Color32(40, 40, 40, 255);
        private static readonly Color32 _hoverButtonFromMenu = new Color32(18, 79, 179, 255);
        GUISkin ChangeSkin()
        {
            if (_cachedSkin is null || _cachedSkin.window.active.background is null)
            {
                _cachedSkin = GUI.skin;
                Texture2D windowBackground = BuildTexFrom1Color(_classicMenuBackground);
                _cachedSkin.window.active.background = windowBackground;
                _cachedSkin.window.onActive.background = windowBackground;
                _cachedSkin.window.focused.background = windowBackground;
                _cachedSkin.window.onFocused.background = windowBackground;
                _cachedSkin.window.hover.background = windowBackground;
                _cachedSkin.window.onHover.background = windowBackground;
                _cachedSkin.window.normal.background = windowBackground;
                _cachedSkin.window.onNormal.background = windowBackground;

                _cachedSkin.window.hover.textColor = Color.white;
                _cachedSkin.window.onHover.textColor = Color.white;

                Color32 hoverbutton = new Color32(255, 255, 0, 255);

                Texture2D buttonBackground = BuildTexFrom1Color(_classicButtonBackground);
                Texture2D hbuttonBackground = BuildTexFrom1Color(hoverbutton);
                _cachedSkin.button.active.background = buttonBackground;
                _cachedSkin.button.onActive.background = buttonBackground;
                _cachedSkin.button.focused.background = buttonBackground;
                _cachedSkin.button.onFocused.background = buttonBackground;
                _cachedSkin.button.hover.background = hbuttonBackground;
                _cachedSkin.button.onHover.background = hbuttonBackground;
                _cachedSkin.button.normal.background = buttonBackground;
                _cachedSkin.button.onNormal.background = buttonBackground;


                Texture2D sliderBackground = BuildTexFrom1Color(new Color32(47, 79, 79, 255));
                _cachedSkin.horizontalSlider.active.background = sliderBackground;
                _cachedSkin.horizontalSlider.onActive.background = sliderBackground;
                _cachedSkin.horizontalSlider.focused.background = sliderBackground;
                _cachedSkin.horizontalSlider.onFocused.background = sliderBackground;
                _cachedSkin.horizontalSlider.hover.background = sliderBackground;
                _cachedSkin.horizontalSlider.onHover.background = sliderBackground;
                _cachedSkin.horizontalSlider.normal.background = sliderBackground;
                _cachedSkin.horizontalSlider.onNormal.background = sliderBackground;

                Texture2D sliderHandleBackground = BuildTexFrom1Color(new Color32(47, 79, 79, 255));
                _cachedSkin.horizontalSliderThumb.active.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onActive.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.focused.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onFocused.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.hover.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onHover.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.normal.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onNormal.background = sliderHandleBackground;

                Texture2D textfield = BuildTexFromColorArray(new Color[] { _classicButtonBackground, _classicButtonBackground, _classicMenuBackground,
                _classicMenuBackground, _classicMenuBackground, _classicMenuBackground , _classicMenuBackground}, 1, 7);
                _cachedSkin.textField.active.background = textfield;
                _cachedSkin.textField.onActive.background = textfield;
                _cachedSkin.textField.focused.background = textfield;
                _cachedSkin.textField.onFocused.background = textfield;
                _cachedSkin.textField.hover.background = textfield;
                _cachedSkin.textField.onHover.background = textfield;
                _cachedSkin.textField.normal.background = textfield;
                _cachedSkin.textField.onNormal.background = textfield;

                _cachedSkin.textField.active.textColor = hoverbutton;
                _cachedSkin.textField.onActive.textColor = hoverbutton;
                _cachedSkin.textField.hover.textColor = hoverbutton;
                _cachedSkin.textField.onHover.textColor = hoverbutton;

                UnityEngine.Object.DontDestroyOnLoad(windowBackground);
                UnityEngine.Object.DontDestroyOnLoad(buttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(hbuttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(textfield);
                UnityEngine.Object.DontDestroyOnLoad(_cachedSkin);
                // TODO: Add custom skin for Toggle and other items
            }

            return _cachedSkin;
        }

        static string GetColorTextForMPType(MultiplayerType mptype)
        {
            switch (mptype)
            {
                //case MultiplayerType.Client:
                //    return "green";
                case MultiplayerType.Unspecified:
                    return "#FFFF99";
                case MultiplayerType.All:
                    return "#FF3333";
                default:
                    return string.Empty;
            }
        }

        static string GetColoredMPTypeText(MultiplayerType mptype)
        {
            switch (mptype)
            {
                case MultiplayerType.Client:
                    return "<color=#00CC00>Client</color>";
                case MultiplayerType.Host:
                    return "<color=#00CC00>Host</color>";
                case MultiplayerType.Unspecified:
                    return "<color=#FFFF99>Unspecified</color>";
                case MultiplayerType.All:
                    return "<color=#FF3333>All</color>";
                default:
                    return mptype.ToString();
            }
        }

        static string GetTextForMPType(MultiplayerType mptype)
        {
            switch (mptype)
            {
                case MultiplayerType.All:
                    return "<color=#FF3333>All</color> - All Clients will be required to install this mod.";
                case MultiplayerType.Client:
                    return "<color=#00CC00>Client</color> - This mod is client-side, but might have special behavior.";
                case MultiplayerType.Host:
                    return "<color=#00CC00>Host</color> - The host must have this mod for functionality, but it won't prevent joining a vanilla client's game.";
                case MultiplayerType.Unspecified:
                    return "<color=#FFFF99>Unspecified</color> - This mod has not had it's multiplayer operations specified for VoidManager.\n" +
                        "- If the host has VoidManager and this mod, Connection will be allowed.\n" +
                        "- If the host has VoidManager but not this mod, they can optionally trust Unspecified Mods.\n" +
                        "- If the host does not have VoidManager, Connection will be disallowed.\n" +
                        "- If the local client is hosting, vanilla clients will be allowed to join the session.";
                default:
                    return mptype.ToString();
            }
        }

        void DrawModButton(VoidPlugin voidPlugin)
        {
            if (voidPlugin.MPType > MPModChecks.MultiplayerType.Host)
            {
                if (Button($"<color={GetColorTextForMPType(voidPlugin.MPType)}>{voidPlugin.BepinPlugin.Metadata.Name}</color>")) //FFFF99
                    selectedMod = voidPlugin;
            }
            else
            {
                if (Button(voidPlugin.BepinPlugin.Metadata.Name))
                    selectedMod = voidPlugin;
            }
        }

        void DrawPlayerModList(Player player)
        {
            MPUserDataBlock userData = MPModCheckManager.Instance.GetNetworkedPeerMods(player);
            if (userData != null)
            {
                Label($"User VoidManager version: {userData.VMVersion}");
                Label("ModList:");
                string ModListText = string.Empty;
                bool first = true;
                foreach (MPModDataBlock modData in userData.ModData)
                {
                    if (first)
                        first = false;
                    else
                        ModListText += "\n";

                    ModListText += $"- {modData.ModName} v{modData.Version}, MPType: {GetColoredMPTypeText(modData.MPType)}";
                }
                Label(ModListText);
            }
            else
            {
                Label("No Mod data.");
            }
        }

        public void OpenSettingsMenu(ModSettingsMenu menu)
        {
            Tab = 1;
            menu.OnOpen();
            selectedSettings = menu;
        }

        Texture2D BuildTexFrom1Color(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        Texture2D BuildTexFromColorArray(Color[] color, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(color);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Iterates through the current Plugin files and searches for gui menus.
        /// </summary>
        public void DiscoverGUIMenus(System.Reflection.Assembly assembly, VoidPlugin voidPlugin)
        {
            mods.Add(voidPlugin);
            var types = assembly.GetTypes();
            // Finds gui menu implementations from all the Assemblies in the same file location.
            var chatCommandInstances = types.Where(t => typeof(ModSettingsMenu).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            bool hasSettingsMenu = false;
            foreach (var modType in chatCommandInstances)
            { // Iterates through each discovered gui menu
                ModSettingsMenu modInstance = (ModSettingsMenu)Activator.CreateInstance(modType);
                settings.Add(modInstance);
                hasSettingsMenu = true;
            }
            if (hasSettingsMenu) BepinPlugin.Log.LogInfo($"[{voidPlugin.BepinPlugin.Metadata.Name}] detected settings menu");
        }

        public void DiscoverNonVManMod(VoidPlugin voidPlugin)
        {
            NonVManMods.Add(voidPlugin);
        }

        bool ShowingCursor;

        void GUIToggleCursor(bool enable)
        {
            if (!BepinPlugin.Bindings.MenuUnlockCursor.Value && !(!enable && ShowingCursor))
            {
                return; // Stop early if unlocking cursor is disabled, but allow passthrough if cursor is enabled and is getting set to disabled.
            }

            ShowingCursor = enable;
            CursorUtility.ShowCursor(this, enable);

            if (ShowingCursor)
            {
                InputActionMapRequests.AddOrChangeRequestAllMaps(this, false);
                InputActionMapRequests.AddOrChangeRequest(this, "GlobalBindings", true);
                InputActionMapRequests.AddOrChangeRequest(this, "Debug", true);
            }
            else
            {
                InputActionMapRequests.RemoveRequestAllMaps(this);
            }
        }
    }
}
