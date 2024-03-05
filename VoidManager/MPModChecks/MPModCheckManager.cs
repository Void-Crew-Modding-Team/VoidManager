using BepInEx;
using BepInEx.Bootstrap;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using VoidManager.Mod;
using VoidManager.MPModChecks.Callbacks;
using VoidManager.Utilities;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace VoidManager.MPModChecks
{
    internal class MPModCheckManager //Todo - Mark modded lobbies.
    {
        internal MPModCheckManager()
        {
            instance = this;
            UpdateMyModList();
            BuildRoomProperties();
        }

        internal const byte PlayerMPUserDataEventCode = 99;
        internal const string RoomModsPropertyKey = "Mods";

        internal static RoomCallbacks RoomCallbacksClass;
        private MPModDataBlock[] MyModList = null;
        private MPModDataBlock[] MyMPModList = null;
        byte[] RoomProperties;
        internal Dictionary<Player, MPUserDataBlock> NetworkedPeersModLists = new Dictionary<Player, MPUserDataBlock>();
        private MultiplayerType HighestLevelOfMPMods = MultiplayerType.Hidden;
        public string LastModCheckFailReason;

        private static MPModCheckManager instance;

        public static MPModCheckManager Instance
        {
            get
            {
                if (instance == null)
                {
                    return new MPModCheckManager();
                }
                return instance;
            }
        }

        

        public byte[] GetRoomProperties()
        {
            return RoomProperties;
        }

        public void BuildRoomProperties()
        {
            RoomProperties = SerializeHashlessMPUserData();
        }

        public void UpdateLobbyProperties()
        {
            if(!PhotonNetwork.IsMasterClient) //Only MC should update the lobby properties.
            {
                return;
            }

            Room CurrentRoom = PhotonNetwork.CurrentRoom;
            if (CurrentRoom == null) //This would probably break stuff
            {
                Plugin.Log.LogWarning("Attempted to update lobby properties while room was null");
                return;
            }
            if(!CurrentRoom.CustomProperties.ContainsKey(RoomModsPropertyKey))//If the key doesn't already exist, there have been no limitations imposed on the room.
            {
                return;
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { RoomModsPropertyKey, RoomProperties } });
        }

        static byte[] GetFileHash(string fileLocation)
        {
            using (SHA256 Hasher = SHA256.Create())
            {
                return Hasher.ComputeHash(File.ReadAllBytes(fileLocation));
            }
        }

        private void UpdateHighestLevelOfMPMods(MultiplayerType MT)
        {
            //Tier: All > Client > Hidden
            if (HighestLevelOfMPMods == MultiplayerType.Hidden && MT != MultiplayerType.Hidden)
            {
                HighestLevelOfMPMods = MT;
            }
            else if(HighestLevelOfMPMods == MultiplayerType.Client && MT == MultiplayerType.All)
            {
                HighestLevelOfMPMods = MultiplayerType.All;
            }
        }

        private void UpdateMyModList()
        {
            Plugin.Log.LogInfo("Building MyModList");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            KeyValuePair<string,PluginInfo>[] UnprocessedMods = Chainloader.PluginInfos.ToArray();
            MPModDataBlock[] ProcessedMods = new MPModDataBlock[UnprocessedMods.Length];
            for (int i = 0; i < UnprocessedMods.Length; i++)
            {
                PluginInfo currentMod = UnprocessedMods[i].Value;
                string GUID = currentMod.Metadata.GUID;
                if (PluginHandler.ActiveVoidPlugins.TryGetValue(GUID, out VoidPlugin voidPlugin)) //Check for metadata for MPType. If metadata doesn't exist, default to MPType.all
                {
                    ProcessedMods[i] = new MPModDataBlock(GUID, currentMod.Metadata.Name, currentMod.Metadata.Version.ToString(), voidPlugin.MPType, string.Empty, GetFileHash(currentMod.Location));
                }
                else
                {
                    ProcessedMods[i] = new MPModDataBlock(GUID, currentMod.Metadata.Name, currentMod.Metadata.Version.ToString(), MultiplayerType.All, string.Empty, GetFileHash(currentMod.Location));
                }
            }
            MyModList = ProcessedMods;
            MyMPModList = ProcessedMods.Where(Mod => Mod.MPType == MultiplayerType.All).ToArray();
            stopwatch.Stop();
            Plugin.Log.LogInfo("Finished Building MyModList, time elapsted: " + stopwatch.ElapsedMilliseconds.ToString());
        }

        /// <summary>
        /// Serilizes user data into a byte array for network transfer. Does not contain a hash
        /// </summary>
        /// <returns>Serilized User data (Hashless)</returns>
        public byte[] SerializeHashlessMPUserData()
        {
            MemoryStream dataStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(dataStream))
            {
                //Datastream storage structure:
                writer.Write(MyPluginInfo.PLUGIN_VERSION);      //--Header--
                writer.Write(MyModList.Length);                 //string VMVersion
                for (int i = 0; i < MyModList.Length; i++)      //int    modcount
                {                                               //
                    MPModDataBlock dataBlock = MyModList[i];    //--ModData--
                    writer.Write(dataBlock.ModName);            //string mod name
                    writer.Write(dataBlock.ModGUID);            //string harmony ident
                    writer.Write(dataBlock.Version);            //string mod version
                    writer.Write((byte)dataBlock.MPType);       //byte   AllRequireMod
                    writer.Write(dataBlock.DownloadID);         //string ModID
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
                Plugin.Log.LogInfo($"Failed to read mod list from Hashless MPUserData, returning null.\n{ex.Message}");
                memoryStream.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Serilizes user data into a byte array for network transfer. Contains a hash.
        /// </summary>
        /// <returns>Serilized User data (Hashfull)</returns>
        public byte[] SerializeHashfullMPUserData()
        {
            MemoryStream dataStream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(dataStream))
            {
                //Datastream storage structure:
                writer.Write(MyPluginInfo.PLUGIN_VERSION);      //--Header--
                writer.Write(MyModList.Length);                 //string VMVersion
                for (int i = 0; i < MyModList.Length; i++)      //int    modcount
                {                                               //
                    MPModDataBlock dataBlock = MyModList[i];    //--ModData--
                    writer.Write(dataBlock.ModName);            //string mod name
                    writer.Write(dataBlock.ModGUID);            //string harmony ident
                    writer.Write(dataBlock.Version);            //string mod version
                    writer.Write((byte)dataBlock.MPType);       //bool   AllRequireMod
                    writer.Write(dataBlock.DownloadID);         //string ModID
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

                    string PMLVersion = reader.ReadString();
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
                    return new MPUserDataBlock(PMLVersion, ModList);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo($"Failed to read mod list from Hashfull MPUserData, returning null.\n{ex.Message}");
                memoryStream.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Converts a ModDataBlock array to a string list, Usually for logging purposes. Starts with a new line
        /// </summary>
        /// <param name="ModDatas"></param>
        /// <returns>Converts ModDataBLocks to a string list.</returns>
        public static string GetModListAsString(MPModDataBlock[] ModDatas)
        {
            string ModList = string.Empty;
            foreach (MPModDataBlock DataBlock in ModDatas)
            {
                ModList += $"\n{DataBlock.ModName}";
            }
            return ModList;
        }

        /// <summary>
        /// Provides the host MPUserDataBlock from room properties.
        /// </summary>
        /// <returns></returns>
        internal MPUserDataBlock GetHostModList()
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomModsPropertyKey))
            {
                try
                {
                    return DeserializeHashlessMPUserData((byte[])PhotonNetwork.CurrentRoom.CustomProperties[RoomModsPropertyKey]);
                }
                catch
                {
                    Plugin.Log.LogError("Failed to Deserialize host mod list.");
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
        /// Checks if given player has mod, checked by HarmonyID
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="HarmonyIdentifier"></param>
        /// <returns>Returns true if player has mod</returns>
        public bool NetworkedPeerHasMod(Player Player, string HarmonyIdentifier)
        {
            MPUserDataBlock userData = GetNetworkedPeerMods(Player);
            if (userData != null)
            {
                foreach (MPModDataBlock modData in userData.ModData)
                {
                    if (modData.ModGUID == HarmonyIdentifier)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Finds all Networked Peers with a given harmony ID
        /// </summary>
        /// <param name="HarmonyIdentifier"></param>
        /// <returns>NetworkedPeers using given mod</returns>
        public List<Player> NetworkedPeersWithMod(string HarmonyIdentifier)
        {
            List<Player> playerList = new List<Player>();
            foreach (KeyValuePair<Player, MPUserDataBlock> userEntry in NetworkedPeersModLists)
            {
                foreach (MPModDataBlock modData in userEntry.Value.ModData)
                {
                    if (modData.ModGUID == HarmonyIdentifier)
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
            Plugin.Log.LogMessage($"recieved modlist from user with the following info:\nVMVersion: {modList.VMVersion}\n\nModlist:{MPModCheckManager.GetModListAsString(modList.ModData)}");
            if (NetworkedPeersModLists.ContainsKey(Player))
            {
                NetworkedPeersModLists[Player] = modList;
                return;
            }
            NetworkedPeersModLists.Add(Player, modList);
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
        /// <param name="Player"></param>
        internal void ClearAllNetworkedPeerMods()
        {
            NetworkedPeersModLists.Clear();
        }

        /// <summary>
        /// Checks NetworkedPeersModLists 
        /// </summary>
        /// <param name="Player"></param>
        /// <returns></returns>
        public bool GetNetworkedPeerModlistExists(Player Player)
        {
            return NetworkedPeersModLists.ContainsKey(Player);
        }

        private static MPUserDataBlock GetHostModList(RoomInfo room)
        {
            if (room.CustomProperties.ContainsKey("modList"))
            {
                try
                {
                    return DeserializeHashlessMPUserData((byte[])room.CustomProperties["modList"]);
                }
                catch
                {
                    Plugin.Log.LogError("Failed to Deserialize host mod list. Could be an older version of PML");
                }
            }
            return new MPUserDataBlock();
        }

        internal void SendModlistToClient(Player Player)
        {
            PhotonNetwork.RaiseEvent(MPModCheckManager.PlayerMPUserDataEventCode, new object[] { false, SerializeHashlessMPUserData() }, new RaiseEventOptions { TargetActors = new int[1] { Player.ActorNumber } }, SendOptions.SendReliable);
        }

        internal void SendModlistToHost()
        {
            PhotonNetwork.RaiseEvent(MPModCheckManager.PlayerMPUserDataEventCode, new object[] { true, SerializeHashfullMPUserData() }, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.MasterClient.ActorNumber } }, SendOptions.SendReliable);
        }

        internal void SendModListToOthers()
        {
            PhotonNetwork.RaiseEvent(MPModCheckManager.PlayerMPUserDataEventCode, new object[] { false, SerializeHashlessMPUserData() }, null, SendOptions.SendReliable);
        }

        public static IEnumerator PlayerJoinedChecks(Player JoiningPlayer)
        {
            for (int i = 0; i < 50; i++)
            {
                yield return new WaitForSeconds(.2f);
                if (Instance.GetNetworkedPeerModlistExists(JoiningPlayer))
                {
                    Instance.ModChecksHostOnClientJoin(JoiningPlayer);
                    yield break;
                }
            }
            if (Instance.HighestLevelOfMPMods == MultiplayerType.All)
            {
                //Kick player if mod no mod list recieved and there are local MPType.All Mods.
                Plugin.Log.LogMessage($"Kicked player {JoiningPlayer.NickName} for not having mods.");
                Messaging.Echo($"Kicked player {JoiningPlayer.NickName} for not having mods.", false);
                PhotonNetwork.CloseConnection(JoiningPlayer);
            }
        }

        internal bool ModChecksClientside(Hashtable RoomProperties)
        {
            if (!RoomProperties.ContainsKey(RoomModsPropertyKey))//Host doesn't have mods
            {
                if (HighestLevelOfMPMods == MultiplayerType.All)
                {
                    return false; //Case: Host doesn't have mods, but Client has mod(s) which need the host to install.
                }
                else
                {
                    return true; //Case: Host doesn't have mods, but client doesn't have restrictive mods.
                }
            }
            //Host must have mods beyond this point. 
            

            //Selects only mods which have MPType set to all for comparison.
            MPUserDataBlock HostModData = DeserializeHashlessMPUserData((byte[])RoomProperties[RoomModsPropertyKey]);
            MPModDataBlock[] HostMods = HostModData.ModData.Where(Mod => Mod.MPType == MultiplayerType.All).ToArray();

            List<string> MismatchedVersions = new List<string>();
            List<string> ClientMissing = new List<string>();
            List<string> HostMissing = new List<string>();

            //Compare Client mods against Host mods
            int i;
            int x;
            MPModDataBlock CurrentHostMod;
            for (i = 0; i < MyMPModList.Length; i++)
            {
                MPModDataBlock CurrentLocalMod = MyMPModList[i];
                bool found = false;
                for (x = 0; x < HostMods.Length; x++)
                {
                    if (CurrentLocalMod.ModGUID == HostMods[x].ModGUID)
                    {
                        CurrentHostMod = HostMods[x];
                        found = true;
                        if (CurrentLocalMod.Version != CurrentHostMod.Version)
                        {
                            //Mod versions do not match
                            MismatchedVersions.Add($"Client:{CurrentLocalMod.ModName}-{CurrentLocalMod.Version}, Host:{CurrentHostMod.Version}");
                            Plugin.Log.LogMessage($"Mismatched mod version - {MismatchedVersions.Last()}. { ((CurrentHostMod.DownloadID != string.Empty) ? $"Download Link: {CurrentHostMod.DownloadID}" : "") }");
                        }
                        break;
                    }
                }
                if (!found)
                {
                    //Client MPType.All Mod not found in host mods
                    HostMissing.Add(CurrentLocalMod.ModName);
                    Plugin.Log.LogMessage($"Host is missing the required mod '{CurrentLocalMod.ModName}'");
                }
            }

            //Compare Host mods against client mods
            for (i = 0; i < HostMods.Length; i++)
            {
                CurrentHostMod = HostMods[i];
                bool found = false;
                for (x = 0; x < MyMPModList.Length; x++)
                {
                    if (CurrentHostMod.ModGUID == MyMPModList[x].ModGUID)
                    {
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    //Host MPType.All Mod not found in Client mods
                    ClientMissing.Add(CurrentHostMod.ModName);
                    Plugin.Log.LogMessage($"Client is missing the required mod '{CurrentHostMod.ModName}'");
                }
            }


            //Check if problems were found and add to error message.
            string errorMessage = string.Empty;

            if(MismatchedVersions.Count > 0) //Case: Client and Host have mismatched mod versions
            {
                errorMessage += "The following mods have mismatched versions:\n";
                foreach(string str in MismatchedVersions)
                {
                    errorMessage += str + "\n";
                }
            }
            if (ClientMissing.Count > 0) //Case: Client is missing mods required by the host.
            {
                errorMessage += "The following mods are required to join the session:\n";
                foreach (string str in ClientMissing)
                {
                    errorMessage += str + "\n";
                }
            }
            if (HostMissing.Count > 0) //Case: Client has mods which need to be installed by the host.
            {
                errorMessage += "The following mods must be uninstalled to join the session:";
                foreach (string str in HostMissing)
                {
                    errorMessage += "\n" + str;
                }
            }

            //return false and finalize error message.
            if(errorMessage != string.Empty)
            {
                LastModCheckFailReason += errorMessage;

                errorMessage = "Couldn't join session.\n" + errorMessage;
                Plugin.Log.LogMessage(errorMessage);
                return false;
            }

            //Mod check passed.
            Plugin.Log.LogMessage("Clientside mod check passed.");
            return true;
        }

        internal void ModChecksHostOnClientJoin(Player joiningPlayer)
        {
            MPUserDataBlock JoiningPlayerMPData = GetNetworkedPeerMods(joiningPlayer);
            MPModDataBlock[] JoiningClientMPMods = JoiningPlayerMPData.ModData.Where(Mod => Mod.MPType == MultiplayerType.All).ToArray();

            List<string> MismatchedVersions = new List<string>();
            List<string> MismatchedHash = new List<string>();
            List<string> JoiningClientMissing = new List<string>();
            List<string> LocalClientMissing = new List<string>();


            int i;
            int x;
            MPModDataBlock CurrentJoiningClientMod;
            for (i = 0; i < MyMPModList.Length; i++)
            {
                MPModDataBlock CurrentLocalMod = MyMPModList[i];
                bool found = false;
                for (x = 0; x < JoiningClientMPMods.Length; x++)
                {
                    if (CurrentLocalMod.ModGUID == JoiningClientMPMods[x].ModGUID)
                    {
                        CurrentJoiningClientMod = JoiningClientMPMods[x];
                        found = true;
                        if (CurrentLocalMod.Version != CurrentJoiningClientMod.Version)
                        {
                            //Mod versions do not match
                            MismatchedVersions.Add($"Client:{CurrentLocalMod.ModName}-{CurrentLocalMod.Version}, Host:{CurrentJoiningClientMod.Version}");
                            Plugin.Log.LogMessage($"Mismatched mod version - {MismatchedVersions.Last()}. {((CurrentLocalMod.DownloadID != string.Empty) ? $"Download Link: {CurrentLocalMod.DownloadID}" : "")}");
                        }
                        else if (CurrentLocalMod.Hash != CurrentJoiningClientMod.Hash)
                        {
                            //Mod Hash Mismatch. Log hash, but tell joining client it's a version mismatch.
                            MismatchedVersions.Add($"Client:{ CurrentLocalMod.ModName}-{ CurrentLocalMod.Version}, Host: { CurrentJoiningClientMod.Version}");
                            Plugin.Log.LogMessage($"Mismatched mod hash - {MismatchedHash.Last()}. {((CurrentLocalMod.DownloadID != string.Empty) ? $"Download Link: {CurrentLocalMod.DownloadID}" : "")}");
                        }
                        break;
                    }
                }
                if (!found)
                {
                    //Client MPType.All Mod not found in host mods
                    JoiningClientMissing.Add(CurrentLocalMod.ModName);
                    Plugin.Log.LogMessage($"Client is missing the required mod '{CurrentLocalMod.ModName}'");
                }
            }

            //Compare Local mods against Joining Client mods
            for (i = 0; i < JoiningClientMPMods.Length; i++)
            {
                CurrentJoiningClientMod = JoiningClientMPMods[i];
                bool found = false;
                for (x = 0; x < MyMPModList.Length; x++)
                {
                    if (CurrentJoiningClientMod.ModGUID == MyMPModList[x].ModGUID)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    //Host MPType.All Mod not found in Joining Client mods
                    LocalClientMissing.Add(CurrentJoiningClientMod.ModName);
                    Plugin.Log.LogMessage($"Client must uninstall the MPType.All Mod '{CurrentJoiningClientMod.ModName}'");
                }
            }


                string errorMessage = string.Empty;

            if (MismatchedVersions.Count > 0) //Case: Client and Host have mismatched mod versions
            {
                errorMessage += "The following mods have mismatched versions:\n";
                foreach (string str in MismatchedVersions)
                {
                    errorMessage += str + "\n";
                }
            }
            if (JoiningClientMissing.Count > 0) //Case: Client is missing mods required by the host.
            {
                errorMessage += "The following mods are required to join the session:\n";
                foreach (string str in JoiningClientMissing)
                {
                    errorMessage += str + "\n";
                }
            }
            if (LocalClientMissing.Count > 0) //Case: Client has mods which need to be installed by the host.
            {
                errorMessage += "The following mods must be uninstalled to join the session:";
                foreach (string str in LocalClientMissing)
                {
                    errorMessage += "\n" + str;
                }
            }

            //Kick Player and finalize error message.
            if (errorMessage != string.Empty)
            {
                //Send message to joining client.
                //fixme Possible send list via steammanager for non-modded clients.
                PhotonNetwork.CloseConnection(joiningPlayer);
                Plugin.Log.LogMessage($"Kicked player {joiningPlayer.NickName} from session for incompatable mods.\n{errorMessage}");
            }

            Plugin.Log.LogMessage("Hostside mod check passed for player " + joiningPlayer.NickName);
        }
    }
}
