using HarmonyLib;
using UI.Matchmaking;

namespace VoidManager.MPModChecks.Patches
{
    [HarmonyPatch(typeof(MatchMakingJoinPanel), "QuickJoinRequested")]
    internal class QuickJoinPatch
    {
        static bool Prefix()
        {
            MenuScreenController.Instance.ShowMessagePopup("matchmaking_unable_join".GetLocalized("Terminals"), $"Quick Join disabled by {MyPluginInfo.USERS_PLUGIN_NAME}");
            return false;
        }
    }
}
