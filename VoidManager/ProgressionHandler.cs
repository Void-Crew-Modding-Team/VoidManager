using CG.Cloud;
using CG.Profile;
using HarmonyLib;
using System.Reflection;

namespace VoidManager
{
    public class ProgressionHandler
    {
        internal static bool ProgressionEnabled = true;
        public static void DisableProgression()
        {
            BepinPlugin.Log.LogInfo("Progression Disabled");
            ProgressionEnabled = false;
        }
        internal static void EnableProgression()
        {
            BepinPlugin.Log.LogInfo("Progression Enabled");
            Load();
        }
        private static PropertyInfo ProfileInfo = AccessTools.Property(typeof(PlayerProfile), "Profile");
        private static PropertyInfo UnlockedItemsInfo = AccessTools.Property(typeof(PlayerProfile), "UnlockedItems");
        private static PropertyInfo PerksInfo = AccessTools.Property(typeof(PlayerProfile), "Perks");
        private static PropertyInfo PlayerLoadoutInfo = AccessTools.Property(typeof(PlayerProfile), "PlayerLoadout");
        private static PropertyInfo ShipLoadoutsInfo = AccessTools.Property(typeof(PlayerProfile), "ShipLoadouts");
        private static PropertyInfo UnseenInfo = AccessTools.Property(typeof(PlayerProfile), "Unseen");
        private static PropertyInfo RewardsInfo = AccessTools.Property(typeof(PlayerProfile), "Rewards");
        private static async void Load()
        {
            VoidManager.Utilities.Messaging.Echo("Working . . . ");
            PlayerData playerData = await CloudProfileReader.GetProfile();
            // Doesnt equip on character the cosmetics
            // Doesnt update gene tree on terminal
            // Reccomended to wait until process completes before opening terminal
            ProfileInfo.SetValue(PlayerProfile.Instance, playerData.Profile);
            UnlockedItemsInfo.SetValue(PlayerProfile.Instance, playerData.UnlockedItems);
            PerksInfo.SetValue(PlayerProfile.Instance, playerData.Perks);
            PlayerLoadoutInfo.SetValue(PlayerProfile.Instance, playerData.PlayerLoadout);
            ShipLoadoutsInfo.SetValue(PlayerProfile.Instance, playerData.ShipLoadouts);
            UnseenInfo.SetValue(PlayerProfile.Instance, playerData.Unseen);
            RewardsInfo.SetValue(PlayerProfile.Instance, playerData.Rewards);
            VoidManager.Utilities.Messaging.Echo("Complete.");
            ProgressionEnabled = true;
        }
    }
}
