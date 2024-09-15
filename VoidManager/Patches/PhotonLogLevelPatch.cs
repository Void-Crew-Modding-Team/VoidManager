using HarmonyLib;
using Photon.Pun;

namespace VoidManager.Patches
{
    [HarmonyPatch(typeof(PhotonNetwork), "LoadOrCreateSettings")]
    internal class PhotonLogLevelPatch
    {
        static void Postfix()
        {
            ServerSettings serverSettings = PhotonNetwork.PhotonServerSettings;
            if (serverSettings != null)
            {
                serverSettings.PunLogging = BepinPlugin.Bindings.PunLoggingSettingLevel.Value;
                serverSettings.AppSettings.NetworkLogging = ExitGames.Client.Photon.DebugLevel.ALL;
            }
        }
    }
}
