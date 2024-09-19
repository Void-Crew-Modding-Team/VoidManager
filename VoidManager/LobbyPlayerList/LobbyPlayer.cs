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
            Rank = LobbyPlayerListManager.GetPlayerRank(player);
            FavorRank = LobbyPlayerListManager.GetPlayerFavorRank(player);
        }

        public LobbyPlayer(string name, ulong steamID, int rank, int favorRank)
        {
            Name = name;
            SteamID = (CSteamID)steamID;
            Rank = rank;
            FavorRank = favorRank;
        }

        public LobbyPlayer(string name, CSteamID steamID, int rank, int favorRank)
        {
            Name = name;
            SteamID = steamID;
            Rank = rank;
            FavorRank = favorRank;
        }

        internal string Name;
        internal CSteamID SteamID;
        internal int Rank;
        internal int FavorRank;
        internal Player myPlayer;
    }
}
