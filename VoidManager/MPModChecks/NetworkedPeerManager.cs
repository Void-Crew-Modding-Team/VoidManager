using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using VoidManager.Callbacks;

namespace VoidManager.MPModChecks
{
    /// <summary>
    /// Manages User Mod Lists.
    /// </summary>
    public class NetworkedPeerManager
    {
        /// <summary>
        /// The Current Instance of the Peer Manager
        /// </summary>
        public static NetworkedPeerManager Instance { get; internal set; }

        internal Dictionary<Player, MPUserDataBlock> NetworkedPeersModLists = new();


        /// <summary>
        /// Provides the host MPUserDataBlock from room properties.
        /// </summary>
        /// <returns>Host's MPUserDataBlock</returns>
        public MPUserDataBlock GetHostModList()
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(InRoomCallbacks.RoomModsPropertyKey))
            {
                try
                {
                    return DeserializeHashlessMPUserData((byte[])PhotonNetwork.CurrentRoom.CustomProperties[InRoomCallbacks.RoomModsPropertyKey]);
                }
                catch
                {
                    BepinPlugin.Log.LogError("Failed to Deserialize host mod list.");
                }
            }
            return null;
        }

        /// <summary>
        /// Gets full mod list of Networked Peer.
        /// </summary>
        /// <param name="Player"></param>
        /// <returns>MPUserDataBlock of NetworkedPeer. Returns null if no modlist found.</returns>
        public MPUserDataBlock GetNetworkedPeerMods(Player Player)
        {
            if (NetworkedPeersModLists.TryGetValue(Player, out MPUserDataBlock value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if given player has mod, checked by mod GUID
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="ModGUID"></param>
        /// <returns>Returns true if player has mod</returns>
        public bool NetworkedPeerHasMod(Player Player, string ModGUID)
        {
            MPUserDataBlock userData = GetNetworkedPeerMods(Player);
            if (userData != null)
            {
                foreach (MPModDataBlock modData in userData.ModData)
                {
                    if (modData.ModGUID == ModGUID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Finds all Networked Peers with a given mod GUID
        /// </summary>
        /// <param name="ModGUID"></param>
        /// <returns>NetworkedPeers using given mod</returns>
        public List<Player> NetworkedPeersWithMod(string ModGUID)
        {
            List<Player> playerList = new List<Player>();
            foreach (KeyValuePair<Player, MPUserDataBlock> userEntry in NetworkedPeersModLists)
            {
                foreach (MPModDataBlock modData in userEntry.Value.ModData)
                {
                    if (modData.ModGUID == ModGUID)
                    {
                        playerList.Add(userEntry.Key);
                    }
                }
            }
            return playerList;
        }

        /// <summary>
        /// Adds a player's mod list to the local NetworkedPeersModLists
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="modList"></param>
        public void AddNetworkedPeerMods(Player Player, MPUserDataBlock modList)
        {
            BepinPlugin.Log.LogMessage($"recieved modlist from user '{Player.NickName}' with the following info:\nVoidManager Version: {modList.VMVersion}\nModList:\n{GetModListAsString(modList.ModData)}\n");
            if (NetworkedPeersModLists.ContainsKey(Player))
            {
                NetworkedPeersModLists[Player] = modList;
                return;
            }
            NetworkedPeersModLists.Add(Player, modList);

            Events.Instance.OnClientModlistRecieved(Player);
        }

        /// <summary>
        /// Clears player from NetworkedPeersModLists
        /// </summary>
        /// <param name="Player"></param>
        public void RemoveNetworkedPeerMods(Player Player)
        {
            NetworkedPeersModLists.Remove(Player);
        }

        /// <summary>
        /// Clears all lists from NetworkedPeersModLists
        /// </summary>
        internal void ClearAllNetworkedPeerMods()
        {
            NetworkedPeersModLists.Clear();
        }

        /// <summary>
        /// Checks if player has a mod list in NetworkedPeersModLists 
        /// </summary>
        /// <param name="Player"></param>
        /// <returns>existance of player key in dictionary</returns>
        public bool GetNetworkedPeerModlistExists(Player Player)
        {
            return NetworkedPeersModLists.ContainsKey(Player);
        }




        private static MPUserDataBlock GetHostModList(RoomInfo room)
        {
            if (room.CustomProperties.ContainsKey("modList"))
            {
                try { return DeserializeHashlessMPUserData((byte[])room.CustomProperties["modList"]); }
                catch { BepinPlugin.Log.LogError("Failed to Deserialize host mod list. Could be an older version of VoidManager"); }
            }
            return new MPUserDataBlock();
        }

        internal static void SendModlistToClient(MPModDataBlock[] Data, Player Player)
        {
            if (Player.IsLocal) { return; }

            PhotonNetwork.RaiseEvent(InRoomCallbacks.PlayerMPUserDataEventCode, new object[] { false, SerializeHashlessMPUserData(Data) }, new RaiseEventOptions { TargetActors = new int[1] { Player.ActorNumber } }, SendOptions.SendReliable);
        }

        internal static void SendModlistToHost(MPModDataBlock[] Data)
        {
            if (PhotonNetwork.IsMasterClient) { return; }

            PhotonNetwork.RaiseEvent(InRoomCallbacks.PlayerMPUserDataEventCode, new object[] { true, SerializeHashfullMPUserData(Data) }, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
        }

        internal static void SendModListToOthers(MPModDataBlock[] Data)
        {
            BepinPlugin.Log.LogMessage("sending others");
            PhotonNetwork.RaiseEvent(InRoomCallbacks.PlayerMPUserDataEventCode, new object[] { false, SerializeHashlessMPUserData(Data) }, null, SendOptions.SendReliable);
        }

        internal void LeftRoom()
        {
            Instance.ClearAllNetworkedPeerMods();
        }

        internal void PlayerLeftRoom(Player leavingPlayer)
        {
            Instance.RemoveNetworkedPeerMods(leavingPlayer);
        }


        /// <summary>
        /// Serilizes user data into a byte array for network transfer. Does not contain a hash
        /// </summary>
        /// <returns>Serilized User data (Hashless)</returns>
        public static byte[] SerializeHashlessMPUserData(MPModDataBlock[] Data)
        {
            MemoryStream dataStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(dataStream))
            {
                //Datastream storage structure:
                writer.Write(MyPluginInfo.PLUGIN_VERSION);      //--Header--
                writer.Write(Data.Length);                      //string VMVersion
                for (int i = 0; i < Data.Length; i++)           //int    modcount
                {                                               //
                    MPModDataBlock dataBlock = Data[i];         //--ModData--
                    writer.Write(dataBlock.ModName);            //string mod name
                    writer.Write(dataBlock.ModGUID);            //string Mod GUID
                    writer.Write(dataBlock.Version);            //string mod version
                    writer.Write((byte)dataBlock.MPType);       //byte   MPType
                    writer.Write(dataBlock.DownloadID);         //string Thunderstore ID
                }
            }

            return dataStream.ToArray();
        }

        /// <summary>
        /// Deserializes bytes representing a serialized MPUserDataBlock which does not contain a hash.
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns>MPUserDataBlock (Hashless)</returns>
        public static MPUserDataBlock DeserializeHashlessMPUserData(byte[] byteData)
        {
            MemoryStream memoryStream = new MemoryStream(byteData);
            memoryStream.Position = 0;
            try
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {

                    string VMVersion = reader.ReadString();
                    int ModCount = reader.ReadInt32();
                    MPModDataBlock[] ModList = new MPModDataBlock[ModCount];
                    for (int i = 0; i < ModCount; i++)
                    {
                        string modname = reader.ReadString();
                        string HarmonyIdent = reader.ReadString();
                        string ModVersion = reader.ReadString();
                        MultiplayerType MPType = (MultiplayerType)reader.ReadByte();
                        string ModID = reader.ReadString();
                        ModList[i] = new MPModDataBlock(HarmonyIdent, modname, ModVersion, MPType, ModID);
                    }

                    memoryStream.Dispose();
                    return new MPUserDataBlock(VMVersion, ModList);

                }
            }
            catch (Exception ex)
            {
                BepinPlugin.Log.LogInfo($"Failed to read mod list from Hashless MPUserData, returning null.\n{ex.Message}");
                memoryStream.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Serilizes user data into a byte array for network transfer. Contains a hash.
        /// </summary>
        /// <returns>Serilized User data (Hashfull)</returns>
        public static byte[] SerializeHashfullMPUserData(MPModDataBlock[] Data)
        {
            MemoryStream dataStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(dataStream))
            {
                //Datastream storage structure:
                writer.Write(MyPluginInfo.PLUGIN_VERSION);      //--Header--
                writer.Write(Data.Length);                      //string VMVersion
                for (int i = 0; i < Data.Length; i++)           //int    modcount
                {                                               //
                    MPModDataBlock dataBlock = Data[i];         //--ModData--
                    writer.Write(dataBlock.ModName);            //string mod name
                    writer.Write(dataBlock.ModGUID);            //string Mod GUID
                    writer.Write(dataBlock.Version);            //string mod version
                    writer.Write((byte)dataBlock.MPType);       //byte   MPType
                    writer.Write(dataBlock.DownloadID);         //string Thunderstore ID
                    writer.Write(dataBlock.Hash);               //byte[] Hash
                }
            }
            return dataStream.ToArray();
        }

        /// <summary>
        /// Deserializes bytes representing a serialized MPUserDataBlock containing a hash.
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns>MPUserDataBlock (Hashfull)</returns>
        public static MPUserDataBlock DeserializeHashfullMPUserData(byte[] byteData)
        {
            MemoryStream memoryStream = new MemoryStream(byteData);
            memoryStream.Position = 0;
            try
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {

                    string VoidManager = reader.ReadString();
                    int ModCount = reader.ReadInt32();
                    MPModDataBlock[] ModList = new MPModDataBlock[ModCount];
                    for (int i = 0; i < ModCount; i++)
                    {
                        string modname = reader.ReadString();
                        string HarmonyIdent = reader.ReadString();
                        string ModVersion = reader.ReadString();
                        MultiplayerType MPType = (MultiplayerType)reader.ReadByte();
                        string ModID = reader.ReadString();
                        byte[] Hash = reader.ReadBytes(32);
                        ModList[i] = new MPModDataBlock(HarmonyIdent, modname, ModVersion, MPType, ModID, Hash);
                    }
                    memoryStream.Dispose();
                    return new MPUserDataBlock(VoidManager, ModList);
                }
            }
            catch (Exception ex)
            {
                BepinPlugin.Log.LogInfo($"Failed to read mod list from Hashfull MPUserData, returning null.\n{ex.Message}");
                memoryStream.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Converts a ModDataBlock array to a string list, usually for logging purposes. Starts with a new line.
        /// </summary>
        /// <param name="ModDatas"></param>
        /// <returns>Converts ModDataBLocks to a string list.</returns>
        public static string GetModListAsString(MPModDataBlock[] ModDatas)
        {
            string ModList = string.Empty;
            foreach (MPModDataBlock DataBlock in ModDatas)
            {
                ModList += $"\n - {DataBlock.ModName} {DataBlock.Version}";
            }
            return ModList;
        }

        /// <summary>
        /// Converts a ModDataBlock array to a string list for echo chat purposes.
        /// </summary>
        /// <param name="ModDatas"></param>
        /// <returns>Converts ModDataBLocks to a string list.</returns>
        public static string GetModListAsStringForChat(MPModDataBlock[] ModDatas)
        {
            string ModList = string.Empty;
            bool first = true;
            foreach (MPModDataBlock DataBlock in ModDatas)
            {
                if (first)
                {
                    first = false;
                    ModList += $" - {DataBlock.ModName} {DataBlock.Version}";
                }
                else
                {
                    ModList += $"\n - {DataBlock.ModName} {DataBlock.Version}";
                }
            }
            return ModList;
        }
    }
}
