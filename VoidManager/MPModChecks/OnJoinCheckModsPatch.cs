using HarmonyLib;
using Photon.Realtime;
using UI.Matchmaking;
using VoidManager.Callbacks;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(MatchmakingTerminal), "JoinRequested")]
    class OnJoinCheckModsPatch
    {
        static bool Prefix(FailPopup ___failPopup, MatchmakingList ___matchList)
        {
            LobbyCallbacks LCI = LobbyCallbacks.Instance;
            if (LCI == null || LCI.ActiveTerminal == null || LCI.RoomList == null) //LobbyCallbacks broke. RoomJoining cannot be checked.
            {
                if(LCI.ActiveTerminal == null)
                {
                    BepinPlugin.Log.LogInfo("Terminal");
                }
                if(LCI.RoomList == null)
                {
                    BepinPlugin.Log.LogInfo("Roomlist");
                }
                BepinPlugin.Log.LogInfo("Attempted to join room, VoidManager LobbyCallbacks instance, Active Terminal, or RoomList not found");
                ___failPopup.Show("VoidManager could not find the room. Please close the terminal then try again.");
                return false;
            }

            foreach(RoomInfo RI in LCI.RoomList)
            {
                if (RI.Name == ___matchList.GetSelectedRoom().RoomId)
                {
                    if (!MPModCheckManager.Instance.ModChecksClientside(RI.CustomProperties, false))
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


            BepinPlugin.Log.LogInfo("Attempted to join room, VoidManager could not find the room.");
            ___failPopup.Show("VoidManager could not find the room. Please close the terminal then try again.");
            return false;
        }
    }
}
