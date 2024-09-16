using CG.Profile;
using HarmonyLib;
using Photon.Pun;
using System.Reflection;
using UnityEngine;
using VoidManager.MPModChecks;

namespace VoidManager.Patches
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        static MethodInfo PhotonSetupLogging = AccessTools.Method(typeof(PhotonNetwork), "SetupLogging");

        [HarmonyPostfix]
        public static void PostAwakeInit()
        {
            BepinPlugin.Log.LogInfo($"- - - Void Manager Initialization - - -");

            new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };
            new GameObject("ModList", typeof(ModListGUI)) { hideFlags = HideFlags.HideAndDontSave };

            PluginHandler.DiscoverPlugins();

            NetworkedPeerManager.Instance = new NetworkedPeerManager();
            MPModCheckManager.Instance = new MPModCheckManager();

            //Load Photon Logging settings.
            ServerSettings serverSettings = PhotonNetwork.PhotonServerSettings;
            if (serverSettings != null)
            {
                serverSettings.PunLogging = BepinPlugin.Bindings.PunLoggingSettingLevel.Value;
                serverSettings.AppSettings.NetworkLogging = BepinPlugin.Bindings.PunDebugLogLevel.Value;
                PhotonSetupLogging.Invoke(null, null);
            }
            BepinPlugin.Log.LogInfo($"- - - - - - - - - - - - - - - - - - - -");
        }
    }
}
