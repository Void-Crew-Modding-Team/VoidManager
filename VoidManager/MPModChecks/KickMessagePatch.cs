using CG.GameLoopStateMachine.GameStates;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(GSPhotonDisconnected), "OnSceneLoaded")]
    internal class KickMessagePatch
    {
        internal static string KickTitle;
        internal static string KickMessage;

        [HarmonyPostfix]
        static void patch(Scene scene)
        {
            if (scene.buildIndex == CloneStarConstants.MainMenuSceneIndex && KickTitle != null && KickMessage != null)
            {
                Plugin.Log.LogInfo($"Pushing Kick Message via info screen. Title:{KickTitle} message:\n");

                MenuScreenController.Instance.ShowMessagePopup(KickTitle, KickMessage);

                KickTitle = null;
                KickMessage = null;
            }
        }
    }
}
