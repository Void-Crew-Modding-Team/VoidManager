using Gameplay.Terminals;
using HarmonyLib;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI.Core;
using UI.Matchmaking;
using UnityEngine;
using VoidManager.Callbacks;
using VoidManager.CustomGUI;

namespace VoidManager.MPModChecks
{
    internal class ModListGUI : MonoBehaviour
    {
        private static readonly FieldInfo MatchmakingTerminalField = AccessTools.Field(typeof(MatchmakingTerminal), "matchMakingJoinPanel");
        private static readonly FieldInfo MatchListField = AccessTools.Field(typeof(MatchMakingJoinPanel), "MatchList");

        internal static ModListGUI Instance { get; private set; }

        List<MPModDataBlock> mods;
        string LastCheckedRoom = string.Empty;
        internal RoomInfo CurrentRoom;
        internal TabsRibbon Tabs;


        //GUI vars
        GameObject MLCanvas;
        private bool GUIActive = false;
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
            if(CurrentRoom == null || !CurrentRoom.CustomProperties.ContainsKey(InRoomCallbacks.RoomModsPropertyKey))
            {
                GUIClose();
                return;
            }

            // Disable GUI when room settings are open.
            if (Tabs == null || Tabs.ActiveHeader == 0)
                GUIOpen();
            else
            {
                GUIClose();
                return;
            }
                

            // Save CPU cycles by only running when selecting new room. Side effect, if the room mod list gets changed, the room must be deselected/reselected to see updated list.
            if (LastCheckedRoom != CurrentRoom.Name)
            {
                MPUserDataBlock roomData = NetworkedPeerManager.DeserializeHashlessMPUserData((byte[])CurrentRoom.CustomProperties[InRoomCallbacks.RoomModsPropertyKey]);
                mods = roomData.ModData.Where(mod => mod.MPType > MultiplayerType.Client).ToList();
                mods.Sort((modA, modB) =>
                {
                    int type = modB.MPType.CompareTo(modA.MPType);
                    if (type != 0) return type;
                    return modA.ModName.CompareTo(modB.ModName);
                });
                LastCheckedRoom = CurrentRoom.Name;
                GUIOpen();
            }
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

            CurrentRoom = null;
            LastCheckedRoom = string.Empty;
            mods = null;
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
            if (mods.Count == 0)
            {
                GUILayout.Label("No Mods");
                return;
            }
            ListScroll = GUILayout.BeginScrollView(ListScroll);
            MultiplayerType lastType = mods[0].MPType;
            GUILayout.Label(GetMPTypeHeader(lastType));
            foreach (MPModDataBlock mod in mods)
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

        //Consider using GUIMain.GetColoredMPTypeText(MPType)
        private string GetMPTypeHeader(MultiplayerType type) => type switch
        {
            MultiplayerType.Unmanaged => "Unmanaged",
            MultiplayerType.Client => "Client",
            MultiplayerType.Host => $"<color={GUIMain.HostMPTypeColorCode}>Host</color>",
            MultiplayerType.Session => $"<color={GUIMain.SessionMPTypeColorCode}>Session</color>",
            MultiplayerType.All => $"<color={GUIMain.AllMPTypeColorCode}>Required</color>",
            _ => "Error - type not found"
        };
    }

    //Set selected room for GUI
    [HarmonyPatch(typeof(MatchMakingJoinPanel), "RoomSelected")]
    class OnRoomSelectedPatch
    {
        static void Postfix(MatchmakingRoom room)
        {
            if (MatchmakingController.Instance.GetCachedRoomList().TryGetValue(room.RoomId, out RoomInfo RI))
            {
                ModListGUI.Instance.CurrentRoom = RI;
            }
        }
    }

    //Close GUI when entering a game
    [HarmonyPatch(typeof(MatchmakingController), "JoinGame")]
    class OnRoomDeselectedPatch
    {
        static void Postfix(RoomJoinStatus __result)
        {
            if (__result == RoomJoinStatus.Success)
            {
                ModListGUI.Instance.Tabs = null;
                ModListGUI.Instance.GUIClose();
            }
        }
    }

    //Close GUI when backing out of main menu JoinPanel
    [HarmonyPatch(typeof(MatchMakingMenu), "OnExit")]
    class LeftMatchMakingMenuPatch
    {
        static void Postfix()
        {
            ModListGUI.Instance.GUIClose();
        }
    }

    //Close GUI when leaving any screen; Set Tabs value for disabling GUI while touching game settings.
    [HarmonyPatch(typeof(TerminalScreen), "SetPanelActive")]
    class LeftMatchmakingTerminalPatch
    {
        private static readonly FieldInfo tabsField = AccessTools.Field(typeof(MatchmakingTerminal), "tabs");
        static void Postfix(TerminalScreen __instance,  bool active)
        {
            if (active)
            {
                GameObject GO = __instance.gameObject;
                if (GO.name == "MatchmakingTerminal")
                {
                    ModListGUI.Instance.Tabs = (TabsRibbon)tabsField.GetValue(GO.GetComponent<MatchmakingTerminal>());
                }
            }
            else
            {
                ModListGUI.Instance.Tabs = null;
                ModListGUI.Instance.GUIClose();
            }
        }
    }
}
