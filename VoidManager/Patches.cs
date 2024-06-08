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
            BepinPlugin.Log.LogInfo($"- - - Void Manager Initialization - - -");

            new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };
            
            PluginHandler.DiscoverPlugins();

            MPModCheckManager.Instance = new MPModCheckManager();
            BepinPlugin.Log.LogInfo($"- - - - - - - - - - - - - - - - - - - -");
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
