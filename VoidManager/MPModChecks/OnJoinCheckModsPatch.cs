using HarmonyLib;
using UI.Matchmaking;
using VoidManager.Callbacks;

namespace VoidManager.MPModChecks
{
    //[HarmonyPatch(typeof(MatchmakingTerminal), "JoinRequested")]
    class OnJoinCheckModsPatch
    {
        static bool Prefix(FailPopup ___failPopup)
        {
            LobbyCallbacks LCI = LobbyCallbacks.Instance;
            if (LCI == null || LCI.SelectedRoomProperties == null) //LobbyCallbacks broke. RoomJoining cannot be checked.
            {
                Plugin.Log.LogInfo("Attempted to join room, VoidManager LobbyCallbacks instance or selected room properties not found");
                ___failPopup.Show("VoidManager Could not find the room. Please wait a moment then try again. Could also be bugged.");
                return false;
            }

            if(!MPModCheckManager.Instance.ModChecksClientside(LCI.SelectedRoomProperties))
            {
                ___failPopup.Show("VoidManager blocked connection, Modlists incompatable.\n" + MPModCheckManager.Instance.LastModCheckFailReason);
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
