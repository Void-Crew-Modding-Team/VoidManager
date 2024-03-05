using HarmonyLib;
using UI.Matchmaking;
using VoidManager.MPModChecks.Callbacks;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(MatchmakingTerminal), "JoinRequested")]
    class OnJoinCheckModsPatch
    {
        static bool Prefix(FailPopup __failPopup, MatchmakingList __matchList)
        {
            LobbyCallbacks LCI = LobbyCallbacks.Instance;
            if (LCI == null || LCI.SelectedRoomProperties == null) //LobbyCallbacks broke. RoomJoining cannot be checked.
            {
                Plugin.Log.LogInfo("Attempted to join room, VoidManager LobbyCallbacks instance or selected room properties not found");
                __failPopup.Show("VoidManager Could not find the room. Please wait a moment then try again. Could also be bugged.");
                return false;
            }

            if(!MPModCheckManager.Instance.ModChecksClientside(LCI.SelectedRoomProperties))
            {
                __failPopup.Show("VoidManager blocked connection, Modlists incompatable.\n" + MPModCheckManager.Instance.LastModCheckFailReason);
                return false;
            }
            else
            {
                //Allow connection
                return true;
            }
        }
    }
}
