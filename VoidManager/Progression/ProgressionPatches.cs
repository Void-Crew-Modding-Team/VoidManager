using CG.Cloud;
using CG.Profile;
using HarmonyLib;
using Steamworks;
using System;
using static VoidManager.Progression.ProgressionHandler;

namespace VoidManager.Progression
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

    [HarmonyPatch(typeof(SteamUserStats), "StoreStats")]
    internal class SteamUserStoreStatsPatch
    {
        static bool Prefix() => ProgressionEnabled;
    }

    [HarmonyPatch(typeof(SteamUserStats), "SetStat", new Type[] { typeof(string), typeof(int) })]
    internal class SteamUserSetStats1Patch
    {
        static bool Prefix() => ProgressionEnabled;
    }

    [HarmonyPatch(typeof(SteamUserStats), "SetStat", new Type[] { typeof(string), typeof(int) })]
    internal class SteamUserSetStats2Patch
    {
        static bool Prefix() => ProgressionEnabled;
    }

    [HarmonyPatch(typeof(SteamUserStats), "SetAchievement")]
    internal class SteamUserSetAchievementPatch
    {
        static bool Prefix() => ProgressionEnabled;
    }
}
