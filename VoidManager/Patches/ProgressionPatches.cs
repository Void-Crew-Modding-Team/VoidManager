using CG.Cloud;
using CG.Profile;
using HarmonyLib;
using static VoidManager.ProgressionHandler;

namespace VoidManager.Patches
{
    /// <summary>
    /// Enables/Disables progression handlers save methods.
    /// </summary>
    
    [HarmonyPatch(typeof(CloudLocalProfile), "OnQuit")]
    internal class CloudLocalProfilePatch
    {
        static bool Prefix() => ProgressionEnabled;
    }

    [HarmonyPatch(typeof(LocalPlayerProfile), "OnQuit")]
    internal class LocalPlayerProfilePatch
    {
        static bool Prefix() => ProgressionEnabled;
    }

    [HarmonyPatch(typeof(CloudSyncController), "Write")]
    internal class CloudSyncControllerPatch
    {
        static bool Prefix() => ProgressionEnabled;
    }
}
