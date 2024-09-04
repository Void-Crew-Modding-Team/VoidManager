using CG.GameLoopStateMachine;
using CG.GameLoopStateMachine.GameStates;
using HarmonyLib;
using Photon.Realtime;
using UI.Matchmaking;
using VoidManager.Callbacks;

namespace VoidManager.MPModChecks.Patches
{
    [HarmonyPatch(typeof(MatchMakingJoinPanel), "JoinRequested")]
    class OnJoinCheckModsPatch
    {
        static bool Prefix(MatchmakingList ___MatchList)
        {
            //Stop join if in main menu - Currrently fucked, so I'll just throw up a notification about it.
            if (GameStateMachine.Instance.CurrentState is GSMainMenu)
            {
                MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} cannot join rooms from the main menu at this time. Please select Host Game and join from a terminal.");
                return false;
            }


            LobbyCallbacks LCI = LobbyCallbacks.Instance;
            if (LCI == null || LCI.ActiveTerminal == null || LCI.RoomList == null) //LobbyCallbacks broke. RoomJoining cannot be checked.
            {
                if (LCI.ActiveTerminal == null)
                {
                    BepinPlugin.Log.LogInfo("Terminal");
                }
                if (LCI.RoomList == null)
                {
                    BepinPlugin.Log.LogInfo("Roomlist");
                }
                BepinPlugin.Log.LogInfo($"Attempted to join room, {MyPluginInfo.PLUGIN_NAME} LobbyCallbacks instance, Active Terminal, or RoomList not found");
                MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} could not find the room. Please close the terminal then try again.");
                return false;
            }

            MatchmakingRoom MRoom = ___MatchList.GetSelectedRoom();

            foreach (RoomInfo RI in LCI.RoomList)
            {
                if (RI.Name == MRoom.RoomId)
                {
                    if (MRoom.ModdingType != ModdingType.mod_session)
                    {
                        MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} blocked connection. Per new modding guidelines, {MyPluginInfo.USERS_PLUGIN_NAME} cannot join non-modded sessions. Future updates will enable this functionallity under certain conditions.");
                        return false;
                    }
                    if (!MPModCheckManager.Instance.ModChecksClientside(RI.CustomProperties, false))
                    {
                        MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} blocked connection, Modlists incompatable.\n" + MPModCheckManager.Instance.LastModCheckFailReason);
                        return false;
                    }
                    else
                    {
                        //Allow connection
                        return true;
                    }
                }
            }


            BepinPlugin.Log.LogInfo($"Attempted to join room, {MyPluginInfo.PLUGIN_NAME} could not find the room.");
            MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} could not find the room. Please close the terminal then try again.");
            return false;
        }
    }
}
