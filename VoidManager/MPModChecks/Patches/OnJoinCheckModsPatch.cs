using HarmonyLib;
using Photon.Realtime;
using UI.Matchmaking;

namespace VoidManager.MPModChecks.Patches
{
    [HarmonyPatch(typeof(MatchMakingJoinPanel), "JoinRequested")]
    class OnJoinCheckModsPatch
    {
        static bool Prefix(MatchmakingList ___MatchList)
        {
            MatchmakingRoom MRoom = ___MatchList.GetSelectedRoom();

            if(MatchmakingController.Instance.GetCachedRoomList().TryGetValue(MRoom.RoomId, out RoomInfo roomInfo))
            {
                if (MRoom.ModdingType != ModdingType.mod_session)
                {
                    MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} blocked connection. Per new modding guidelines, {MyPluginInfo.USERS_PLUGIN_NAME} cannot join non-modded sessions. Future updates will enable this functionallity under certain conditions.");
                    return false;
                }
                if (!MPModCheckManager.Instance.ModChecksClientside(roomInfo.CustomProperties, false))
                {
                    MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} blocked connection, Modlists incompatable.\n" + MPModCheckManager.Instance.LastModCheckFailReason);
                    return false;
                }
                else
                {
                    //Allow connection
                    if (BepinPlugin.Bindings.DebugMode.Value)
                        BepinPlugin.Log.LogInfo($"Joining room '{MRoom.RoomName}'");
                    return true;
                }
            }

            BepinPlugin.Log.LogInfo($"Attempted to join room, {MyPluginInfo.PLUGIN_NAME} could not find the room.");
            MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"{MyPluginInfo.USERS_PLUGIN_NAME} could not find the room. Please close the terminal then try again.");
            return false;
        }
    }
}
