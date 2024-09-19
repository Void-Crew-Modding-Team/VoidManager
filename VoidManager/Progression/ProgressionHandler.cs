using CG.Cloud;
using CG.Profile;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;
using VoidManager.Callbacks;
using VoidManager.Utilities;

namespace VoidManager.Progression
{
    /// <summary>
    /// API for enabling/disabling player progression.
    /// </summary>
    public class ProgressionHandler
    {
        /// <summary>
        /// Gets progression enabled state.
        /// </summary>
        public static bool ProgressionEnabled { get; private set; } = true;

        /// <summary>
        /// Disables progression for the whole session.
        /// </summary>
        /// <param name="ModGUID">GUID of mod which called for progression disable.</param>
        public static void DisableProgression(string ModGUID)
        {
            BepinPlugin.Log.LogInfo("Recieved progression disable call from " + ModGUID);

            Messaging.Echo($"<size=30>[{ModGUID}]: Disabled Progression</size>", !PhotonNetwork.IsMasterClient);
            InternalDisableProgression();
            PhotonNetwork.RaiseEvent(InRoomCallbacks.BlockProgressionEventCode, null, default, SendOptions.SendReliable);
        }

        internal static void InternalDisableProgression()
        {
            ProgressionEnabled = false;
            BepinPlugin.Log.LogInfo("Progression Disabled");
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    if (!player.CustomProperties.ContainsKey(InRoomCallbacks.PlayerModsPropertyKey))
                    {
                        Messaging.Echo($"Kicking player {player.NickName} from session for using old Void Manager", false);
                        Messaging.KickMessage("Kicked: Session Progress Disabled", "Detected Void Manager 1.1.8 installed on client. Install latest Void Manager to rejoin session.", player);
                        PhotonNetwork.CloseConnection(player);
                        BepinPlugin.Log.LogMessage($"Kicked player {player.NickName} from session for Detected Void Manager 1.1.8 while session progress is disabled.");
                    }
                }
            }
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
            Messaging.Echo("Working . . . ");
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
            Messaging.Echo("Complete.");
            ProgressionEnabled = true;
        }

        internal static void OnPlayerJoin(Player joiningPlayer)
        {
            // If Player doesn't have mods in custom props, their Void Manager version must be lower than 1.2.0
            if (PhotonNetwork.IsMasterClient && !ProgressionEnabled)
            {
                if (!joiningPlayer.CustomProperties.ContainsKey(InRoomCallbacks.PlayerModsPropertyKey))
                {
                    Messaging.Echo($"Kicking player {joiningPlayer.NickName} from session for using old Void Manager", false);
                    Messaging.KickMessage("Kicked: Session Progress Disabled", "Detected Void Manager 1.1.8 installed on client. Install latest Void Manager to rejoin session.", joiningPlayer);
                    PhotonNetwork.CloseConnection(joiningPlayer);
                    BepinPlugin.Log.LogMessage($"Kicked player {joiningPlayer.NickName} from session for Detected Void Manager 1.1.8 while session progress is disabled.");
                }
                else
                {
                    PhotonNetwork.RaiseEvent(InRoomCallbacks.BlockProgressionEventCode, null, new RaiseEventOptions() { TargetActors = new int[] { joiningPlayer.ActorNumber } }, SendOptions.SendReliable);
                }
            }
        }
    }
}
