using CG.Cloud;
using CG.Profile;
using ExitGames.Client.Photon;
using Gameplay.Utilities;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;
using VoidManager.Callbacks;
using VoidManager.MPModChecks;
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

            if (!ProgressionEnabled) { return; }
            Messaging.Echo($"{ModGUID} Disabled Progression", !PhotonNetwork.IsMasterClient);
            InternalDisableProgression();
            KickPlayersWithoutDisableProgression();
            SendBlockProgression();
        }

        /// <summary>
        /// Runs on all clients disabling progression. Called by Progression Disable Callback and local DisableProgression(GUID) call.
        /// </summary>
        internal static void InternalDisableProgression()
        {
            // Provides [Mods Required] tag for vanilla players, so they know why they're getting kicked.
            MPModCheckManager.Instance.UpdateHighestLevelOfMPMods(MultiplayerType.All);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonService.Instance.SetCurrentRoomName(PhotonService.Instance.GetCurrentRoomName());
            }

            ProgressionEnabled = false;
            BepinPlugin.Log.LogInfo("Progression Disabled");
        }

        internal static void SendBlockProgression()
        {
            PhotonNetwork.RaiseEvent(InRoomCallbacks.BlockProgressionEventCode, null, default, SendOptions.SendReliable);
        }

        internal static bool CheckKickPlayer(Player player)
        {
            if (!player.CustomProperties.TryGetValue(InRoomCallbacks.PlayerModsPropertyKey, out object HashlessData))
            {
                KickPlayer(player, "1.1.8");
                return true;
            }
            else
            {
                MPUserDataBlock DeserializedData = NetworkedPeerManager.DeserializeHashlessMPUserData((byte[])HashlessData);
                if (DeserializedData.VMVersion != MyPluginInfo.PLUGIN_VERSION)
                {
                    KickPlayer(player, DeserializedData.VMVersion);
                    return true;
                }
            }
            return false;
        }

        internal static void KickPlayer(Player player, string DetectedVMVersion)
        {
            Messaging.Echo($"Kicking player {player.NickName} from session for using old Void Manager ({DetectedVMVersion} vs host's {MyPluginInfo.PLUGIN_VERSION})", false);
            Messaging.KickMessage("Kicked: Session Progress Disabled", $"Detected Void Manager {DetectedVMVersion} installed on client. Install latest Void Manager {MyPluginInfo.PLUGIN_VERSION} to rejoin session.", player);
            PhotonNetwork.CloseConnection(player);
            BepinPlugin.Log.LogMessage($"Kicked player {player.NickName} from session for Detected Void Manager {DetectedVMVersion} while session progress is disabled.");
        }

        internal static void KickPlayersWithoutDisableProgression()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    CheckKickPlayer(player);
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
                if (!CheckKickPlayer(joiningPlayer))
                {
                    PhotonNetwork.RaiseEvent(InRoomCallbacks.BlockProgressionEventCode, null, new RaiseEventOptions() { TargetActors = new int[] { joiningPlayer.ActorNumber } }, SendOptions.SendReliable);
                }
            }
        }

        internal static void OnHostChange(Player newHost)
        {
            if(newHost.IsLocal && !ProgressionEnabled)
            {
                Messaging.Echo("New host has progression disabled, disabling progression for others.", false);
                KickPlayersWithoutDisableProgression();
                SendBlockProgression();
            }
        }
    }
}
