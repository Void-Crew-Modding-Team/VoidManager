using Photon.Realtime;
using Steamworks;

namespace VoidManager.LobbyPlayerList
{
    internal class LobbyPlayer
    {
        public LobbyPlayer(Player player)
        {
            Name = player.NickName;
            SteamID = (CSteamID)ulong.Parse(player.UserId);
            myPlayer = player;
        }

        public LobbyPlayer(string name, ulong steamID)
        {
            Name = name;
            SteamID = (CSteamID)steamID;
        }

        public LobbyPlayer(string name, CSteamID steamID)
        {
            Name = name;
            SteamID = steamID;
        }

        internal string Name;
        internal CSteamID SteamID;
        internal Player myPlayer;
    }
}
