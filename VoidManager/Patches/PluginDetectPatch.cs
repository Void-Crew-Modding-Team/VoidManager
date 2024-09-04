using CG.Profile;
using HarmonyLib;
using UnityEngine;
using VoidManager.MPModChecks;

namespace VoidManager.Patches
{
    [HarmonyPatch(typeof(PlayerProfileLoader), "Awake")]
    internal class PluginDetectPatch
    {
        [HarmonyPostfix]
        public static void PostAwakeInit()
        {
            BepinPlugin.Log.LogInfo($"- - - Void Manager Initialization - - -");

            new GameObject("ModManager", typeof(CustomGUI.GUIMain)) { hideFlags = HideFlags.HideAndDontSave };
            new GameObject("ModList", typeof(ModListGUI)) { hideFlags = HideFlags.HideAndDontSave };

            PluginHandler.DiscoverPlugins();

            NetworkedPeerManager.Instance = new NetworkedPeerManager();
            MPModCheckManager.Instance = new MPModCheckManager();
            BepinPlugin.Log.LogInfo($"- - - - - - - - - - - - - - - - - - - -");
        }
    }
}
