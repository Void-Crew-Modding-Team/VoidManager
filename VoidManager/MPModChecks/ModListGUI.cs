using HarmonyLib;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI.Matchmaking;
using UnityEngine;
using VoidManager.Callbacks;
using VoidManager.CustomGUI;

namespace VoidManager.MPModChecks
{
    internal class ModListGUI : MonoBehaviour
    {
        private static readonly FieldInfo matchListField = AccessTools.Field(typeof(MatchmakingTerminal), "matchList");

        internal static ModListGUI Instance { get; private set; }
        GameObject MLCanvas;
        private bool GUIActive = false;
        List<MPModDataBlock> mods;
        Rect WindowPos;
        Vector2 ListScroll = Vector2.zero;

        internal ModListGUI()
        {
            Instance = this;
            MLCanvas = new GameObject("ModListCanvas", new Type[] { typeof(Canvas) });
            Canvas canvasComponent = MLCanvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 1000;
            canvasComponent.transform.SetAsLastSibling();
            DontDestroyOnLoad(MLCanvas);
        }

        private void Update()
        {
            LobbyCallbacks LCI = LobbyCallbacks.Instance;
            if (LCI == null || LCI.ActiveTerminal == null || LCI.RoomList == null || LCI.Tabs == null || LCI.Tabs.ActiveHeader != 0)
            {
                GUIClose();
                return;
            }

            MatchmakingList matchList = (MatchmakingList)matchListField.GetValue(LCI.ActiveTerminal);
            if (matchList.GetSelectedRoom()?.RoomId == null)
            {
                GUIClose();
                return;
            }

            RoomInfo room = LCI.RoomList.FirstOrDefault(room => room.Name == matchList.GetSelectedRoom().RoomId);
            if (room == null)
            {
                GUIClose();
                return;
            }

            if (!room.CustomProperties.ContainsKey(InRoomCallbacks.RoomModsPropertyKey))
            {
                GUIClose();
                return;
            }
            MPUserDataBlock roomData = MPModCheckManager.DeserializeHashlessMPUserData((byte[])room.CustomProperties[InRoomCallbacks.RoomModsPropertyKey]);
            mods = roomData.ModData.Where(mod => mod.MPType > MultiplayerType.Client).ToList();
            mods.Sort((modA, modB) =>
            {
                int type = modB.MPType.CompareTo(modA.MPType);
                if (type != 0) return type;
                return modA.ModName.CompareTo(modB.ModName);
            });
            GUIOpen();
        }

        private void GUIOpen()
        {
            if (GUIActive) return;

            GUIActive = true;
            WindowPos = new Rect(0, Screen.height * 1f / 6f, Screen.width / 12, Screen.height * 2f / 3f);
        }

        internal void GUIClose()
        {
            if (!GUIActive) return;

            GUIActive = false;
        }

        private void OnGUI()
        {
            if (GUIActive)
            {
                GUI.skin = GUIMain.Instance.ChangeSkin();
                WindowPos = GUI.Window(918107, WindowPos, WindowFunction, "Mod List");
            }
        }

        private void WindowFunction(int windowID)
        {
            ListScroll = GUILayout.BeginScrollView(ListScroll);
            MultiplayerType lastType = mods[0].MPType;
            GUILayout.Label(GetMPTypeHeader(lastType));
            foreach(MPModDataBlock mod in mods)
            {
                if (mod.MPType != lastType)
                {
                    lastType = mod.MPType;
                    GUILayout.Label("");
                    GUILayout.Label(GetMPTypeHeader(lastType));
                }
                GUILayout.Label(mod.ModName);
            }
            GUILayout.EndScrollView();
        }

        private string GetMPTypeHeader(MultiplayerType type) => type switch
        {
            MultiplayerType.Hidden => "",
            MultiplayerType.Client => "Client",
            MultiplayerType.Host => "<color=#00CC00>Host</color>",
            MultiplayerType.Unspecified => "<color=#FFFF99>Unknown</color>",
            MultiplayerType.All => "<color=#FF3333>Required</color>",
            _ => "Error - type not found"
        };
    }
}
