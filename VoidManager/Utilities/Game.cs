using CG.Game;
using Gameplay.Quests;
using ResourceAssets;
using System;
using System.Collections.Generic;

namespace VoidManager.Utilities
{
    /// <summary>
    /// Contains ultility methods for the game.
    /// </summary>
    public class Game
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
                //Equivelant to Instance == null ? false : true
                return Instance != null;
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

        /// <summary>
        /// Returns the Local player
        /// </summary>
        [Obsolete("Please switch to CG.Game.Player.LocalPlayer.Instance")]
        public static CG.Game.Player.Player LocalPlayer
        {
            get
            {
                return  CG.Game.Player.LocalPlayer.Instance;
            }
        }

        /// <summary>
        /// Safely attempts to get QuestAsset from GUID
        /// </summary>
        /// <param name="QuestGUID"></param>
        /// <param name="QuestAsset"></param>
        /// <returns>QuestAsset</returns>
        public static bool TryGetQuestAsset(GUIDUnion QuestGUID, out QuestAsset QuestAsset)
        {
            if (ResourceAssetContainer<QuestAssetContainer, QuestAsset, QuestAssetDef>.Instance.TryGetByGuid(QuestGUID, out QuestAssetDef questAssetDef))
            {
                QuestAsset = questAssetDef.Asset;
                return true;
            }
            BepinPlugin.Log.LogError("Provided QuestGUID did not exist");
            QuestAsset = null;
            return false;
        }

        /// <summary>
        /// Gets QuestAsset from GUID
        /// </summary>
        /// <param name="QuestGUID"></param>
        /// <returns>QuestAsset</returns>
        /// <exception cref="ArgumentException">Targeted GUID did not exist</exception>
        public static QuestAsset GetQuestAsset(GUIDUnion QuestGUID)
        {
            if (ResourceAssetContainer<QuestAssetContainer, QuestAsset, QuestAssetDef>.Instance.TryGetByGuid(QuestGUID, out QuestAssetDef questAssetDef))
            {
                return questAssetDef.Asset;
            }
            throw new ArgumentException("Provided QuestGUID did not exist");
        }
    }
}
