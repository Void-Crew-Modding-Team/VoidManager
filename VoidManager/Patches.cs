using CG.GameLoopStateMachine.GameStates;
using CG.Profile;
using HarmonyLib;
using UnityEngine;
using VoidManager.Callbacks;
using VoidManager.MPModChecks;

namespace VoidManager
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        [HarmonyPostfix]
        public static void PostAwakeInit()
        {
            new Events();

            new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };

            Plugin.Log.LogInfo($"- - - Void Manager Initialization - - -");
            PluginHandler.DiscoverPlugins();
            Plugin.Log.LogInfo($"- - - - - - - - - - - - - - - - - - - -");

            new MPModCheckManager();
        }
    }


    //Initialize Room Callbacks class.
    [HarmonyPatch(typeof(GSMainMenu), "OnEnter")]
    class InitRoomCallbacksPatch
    {
        static bool RoomCallbacksInitialized = false;

        [HarmonyPostfix]
        static void InitRoomCallbacks()
        {
            if (RoomCallbacksInitialized)
            {
                return;
            }
            RoomCallbacksInitialized = true;
            MPModCheckManager.RoomCallbacksClass = new InRoomCallbacks();
        }
    }
}
