using CG.GameLoopStateMachine.GameStates;
using HarmonyLib;
using VoidManager.Callbacks;
using VoidManager.MPModChecks;

namespace VoidManager.Patches
{
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
