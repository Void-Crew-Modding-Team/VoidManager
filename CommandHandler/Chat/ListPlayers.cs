using CommandHandler.Chat.Router;
using CommandHandler.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Text;

namespace CommandHandler.Chat
{
    class ListPlayers : ChatCommand
    {
        public override string[] CommandAliases()
            => new string[] { "listplayers", "players", "list" };

        public override string Description()
            => "Returns a list of players with their ID's";

        public override string[] UsageExamples()
            => new string[] { $"/{CommandAliases()[0]}" };

        public override void Execute(string arguments)
        {
            if (!PhotonNetwork.InRoom) return;
            StringBuilder stringBuilder= new StringBuilder();
            stringBuilder.AppendLine("[Player List]");
            foreach (Player photonPlayer in PhotonNetwork.PlayerList)
            {
                if (photonPlayer == null) continue;
                stringBuilder.AppendLine($"{photonPlayer.ActorNumber} - {photonPlayer.NickName}");
            }
            Messaging.Notification(stringBuilder.ToString());
        }
    }
}
