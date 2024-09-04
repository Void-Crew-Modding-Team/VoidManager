using CG.GameLoopStateMachine.GameStates;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VoidManager.MPModChecks.Patches
{
    internal class KickMessagePatches
    {
        internal static string KickTitle;
        internal static string KickMessage;

        [HarmonyPatch(typeof(GSPhotonDisconnected), "OnSceneLoaded")]
        class KickedPatch
        {
            [HarmonyPostfix]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "N/A")]
            static void patch(Scene scene)
            {
                if (scene.buildIndex == CloneStarConstants.MainMenuSceneIndex && KickTitle != null && KickMessage != null)
                {
                    BepinPlugin.Log.LogInfo($"Pushing Kick Message via info screen. Title:{KickTitle} message:\n{KickMessage}");

                    MenuScreenController.Instance.ShowMessagePopup(KickTitle, KickMessage);

                    KickTitle = null;
                    KickMessage = null;
                }
            }
        }
    }

}
