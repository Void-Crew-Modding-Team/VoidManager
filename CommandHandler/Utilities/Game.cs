using CG.Game;
using System;
using System.Collections.Generic;

namespace CommandHandler.Utilities
{
    internal class Game
    {
        /// <summary>
        /// Reference to the clients current game.
        /// </summary>
        public static ClientGame Instance
        {
            get => ClientGame.Current;
        }

        /// <summary>
        /// Boolean value if within game.
        /// </summary>
        public static bool InGame
        {
            get
            {
                return Instance == null ? true : false;
            }
        }

        /// <summary>
        /// Returns list of in game players.
        /// </summary>
        public static List<CG.Game.Player.Player> Players
        {
            get
            {
                if (!InGame) return new List<CG.Game.Player.Player>();
                return ClientGame.Current.Players;
            }
        }

        /// <summary>
        /// Gets a player id from player.
        /// </summary>
        /// <param name="player">Player of the id / ActorNumber to get</param>
        /// <returns>id / ActorNumber</returns>
        public static int GetIDFromPlayer(CG.Game.Player.Player player)
        {
            if (player == null || player.photonView == null || player.photonView.Owner == null) return -1;
            return player.photonView.Owner.ActorNumber;
        }

        /// <summary>
        /// Gets a player from supplied player id.
        /// </summary>
        /// <param name="id">Id of the Player to get (From Playerlist) / ActorNumber</param>
        /// <returns>Player GameObject</returns>
        public static CG.Game.Player.Player GetPlayerFromID(int id)
        {
            if (!InGame) return null;
            return ClientGame.Current.GetPlayerCharacterByActorNumber(id);
        }

        /// <summary>
        /// Gets a player from supplied player name.
        /// </summary>
        /// <param name="playerName">Fullname of the player to get</param>
        /// <returns>Player GameObject</returns>
        public static CG.Game.Player.Player GetPlayerByName(string playerName)
        {
            if (!InGame) return null;
            foreach (CG.Game.Player.Player player in Players)
            {
                if (string.Equals(player.DisplayName, playerName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return player;
                }
            }
            return null;
        }
    }
}
