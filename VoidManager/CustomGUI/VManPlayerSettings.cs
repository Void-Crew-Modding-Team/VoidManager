using CG.Game;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.GUILayout;

namespace VoidManager.CustomGUI
{
    class VManPlayerSettings : PlayerSettingsMenu
    {
        public override void Draw(Player selectedPlayer)
        {
            BeginHorizontal();
            if (PhotonNetwork.MasterClient.IsLocal && !selectedPlayer.IsLocal)
            { // Funny but a bit strange to be able to kick yourself
                if (Button("Kick"))
                    ClientGame.Current.KickPlayer(selectedPlayer);
            }

            string CloudID = VoipService.PlayerToCloudID(selectedPlayer);
            if (!VoipService.Instance.IsPlayerRegistered(CloudID))
            {
                EndHorizontal();
                return;
            }

            string muteButtonLabel = selectedPlayer.IsLocal 
                ? (VoipService.Instance.IsMutedSelf ? "Unmute" : "Mute") 
                : (VoipService.Instance.IsVoiceMuted(CloudID) ? "Unmute" : "Mute");

            if (Button(muteButtonLabel))
            {
                if (selectedPlayer.IsLocal) 
                    VoipService.Instance.MuteSelf(!VoipService.Instance.IsMutedSelf);
                else
                {
                    VoipService.Instance.MuteVoice(CloudID, !VoipService.Instance.IsVoiceMuted(CloudID));
                }
            }
            EndHorizontal();
        }
    }
}
