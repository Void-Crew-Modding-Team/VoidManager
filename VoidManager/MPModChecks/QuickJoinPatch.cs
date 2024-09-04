using HarmonyLib;
using UI.Matchmaking;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(MatchMakingJoinPanel), "QuickJoinRequested")]
    internal class QuickJoinPatch
    {
        static bool Postfix()
        {
            MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"Quick Join disabled by {MyPluginInfo.USERS_PLUGIN_NAME}");
            return false;
        }
    }
}
