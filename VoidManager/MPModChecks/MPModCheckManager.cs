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
using ToolClasses;
using UnityEngine;
using VoidManager.Callbacks;
using VoidManager.Utilities;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace VoidManager.MPModChecks
{
    /// <summary>
    /// Manages multiplayer mod checking and user mod caching.
    /// </summary>
    public class MPModCheckManager
    {
        internal MPModCheckManager()
        {
            UpdateMyModList();
            BuildRoomProperties();
        }

        /// <summary>
        /// Highest level of mods MPType. Does as says
        /// </summary>
        public MultiplayerType HighestLevelOfMPMods { get; private set; } = MultiplayerType.Hidden;

        internal static InRoomCallbacks RoomCallbacksClass;
        private MPModDataBlock[] MyModList = null;
        internal byte[] RoomProperties { get; private set; }
        internal Dictionary<Player, MPUserDataBlock> NetworkedPeersModLists = new Dictionary<Player, MPUserDataBlock>();
        internal string LastModCheckFailReason;

        /// <summary>
        /// The static instance of MPModCheckManager
        /// </summary>
        public static MPModCheckManager Instance { get; internal set; }

        internal void BuildRoomProperties()
        {
            RoomProperties = SerializeHashlessMPUserData();
        }

        internal void UpdateLobbyProperties()
        {
            if (!PhotonNetwork.IsMasterClient) //Only MC should update the lobby properties.
            {
                return;
            }

            Room CurrentRoom = PhotonNetwork.CurrentRoom;
            if (CurrentRoom == null) //This would probably break stuff
            {
                BepinPlugin.Log.LogWarning("Attempted to update lobby properties while room was null");
                return;
            }
            if (!CurrentRoom.CustomProperties.ContainsKey(InRoomCallbacks.RoomModsPropertyKey))//If the key doesn't already exist, there have been no limitations imposed on the room.
            {
                return;
            }

            //Sets VMan modded property.
            CurrentRoom.SetCustomProperties(new Hashtable { { InRoomCallbacks.RoomModsPropertyKey, RoomProperties } });
        }

        private void UpdateHighestLevelOfMPMods(MultiplayerType MT)
        {
            switch (MT)
            {
                case MultiplayerType.All:
                    if (MT > HighestLevelOfMPMods)
                    {
                        HighestLevelOfMPMods = MT;
                        BepinPlugin.Log.LogInfo("Incrementing HighestLevelOfMPMods to " + MT.ToString());
                    }
                    break;
                case MultiplayerType.Session:
                    if (MT > HighestLevelOfMPMods)
                    {
                        HighestLevelOfMPMods = MT;
                        BepinPlugin.Log.LogInfo("Incrementing HighestLevelOfMPMods to " + MT.ToString());
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Detects if a room has been modified.
        /// </summary>
        /// <param name="room"></param>
        /// <returns>true if room is detected as modded</returns>
        public static bool RoomIsModded(RoomInfo room)
        {
            return room.CustomProperties.ContainsKey(InRoomCallbacks.RoomModsPropertyKey) || room.CustomProperties.ContainsKey(InRoomCallbacks.OfficalModdedPropertyKey);
        }

        /// <summary>
        /// Detects if a room has been modified.
        /// </summary>
        /// <param name="roomProperties"></param>
        /// <returns>true if room is detected as modded</returns>
        public static bool RoomIsModded(Hashtable roomProperties)
        {
            return roomProperties.ContainsKey(InRoomCallbacks.RoomModsPropertyKey) || roomProperties.ContainsKey(InRoomCallbacks.OfficalModdedPropertyKey);
        }


        private void UpdateMyModList()
        {
            BepinPlugin.Log.LogInfo("Building MyModList");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            KeyValuePair<string, PluginInfo>[] UnprocessedMods = Chainloader.PluginInfos.ToArray();
            MPModDataBlock[] ProcessedMods = new MPModDataBlock[UnprocessedMods.Length];
            for (int i = 0; i < UnprocessedMods.Length; i++)
            {
                PluginInfo currentMod = UnprocessedMods[i].Value;
                string GUID = currentMod.Metadata.GUID;
                if (GUID == MyPluginInfo.PLUGIN_GUID)//Do not add VoidManager. Without a VoidPlugin it defaults to lacking
                {
                    continue;
                }
                else if (PluginHandler.ActiveVoidPlugins.TryGetValue(GUID, out VoidPlugin voidPlugin) || PluginHandler.GeneratedVoidPlugins.TryGetValue(GUID, out voidPlugin)) //Check for metadata for MPType. If metadata doesn't exist, default to MPType.all
                {
                    if (voidPlugin.MPType != MultiplayerType.Hidden) //Do nothing if marked as hidden.
                    {
                        ProcessedMods[i] = new MPModDataBlock(GUID, currentMod.Metadata.Name, currentMod.Metadata.Version.ToString(), voidPlugin.MPType, string.Empty, voidPlugin.ModHash);
                        UpdateHighestLevelOfMPMods(voidPlugin.MPType);
                    }
                }
            }
            ProcessedMods = ProcessedMods.Where(mod => mod != null).ToArray();
            MyModList = ProcessedMods;
            stopwatch.Stop();
            BepinPlugin.Log.LogInfo("Finished Building MyModList, time elapsted: " + stopwatch.ElapsedMilliseconds.ToString() + " ms");
            BepinPlugin.Log.LogInfo($"MyModList:\n{GetModListAsString(MyModList)}\n");
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
                BepinPlugin.Log.LogInfo($"Failed to read mod list from Hashless MPUserData, returning null.\n{ex.Message}");
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
            BepinPlugin.Log.LogMessage($"recieved modlist from user '{Player.NickName}' with the following info:\nVoidManager Version: {modList.VMVersion}\nModList:\n{MPModCheckManager.GetModListAsString(modList.ModData)}\n");
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

        internal void SendModlistToClient(Player Player)
        {
            if (Player.IsLocal) { return; }

            PhotonNetwork.RaiseEvent(InRoomCallbacks.PlayerMPUserDataEventCode, new object[] { false, SerializeHashlessMPUserData() }, new RaiseEventOptions { TargetActors = new int[1] { Player.ActorNumber } }, SendOptions.SendReliable);
        }

        internal void SendModlistToHost()
        {
            if (PhotonNetwork.IsMasterClient) { return; }

            PhotonNetwork.RaiseEvent(InRoomCallbacks.PlayerMPUserDataEventCode, new object[] { true, SerializeHashfullMPUserData() }, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
        }

        internal void SendModListToOthers()
        {
            BepinPlugin.Log.LogMessage("sending others");
            PhotonNetwork.RaiseEvent(InRoomCallbacks.PlayerMPUserDataEventCode, new object[] { false, SerializeHashlessMPUserData() }, null, SendOptions.SendReliable);
        }

        internal void PlayerJoined(Player JoiningPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PunSingleton<PhotonService>.Instance.StartCoroutine(MPModCheckManager.PlayerJoinedChecks(JoiningPlayer)); //Plugin is not a valid monobehaviour.
            }
            else
            {
                MPModCheckManager.Instance.SendModlistToClient(JoiningPlayer);
            }
        }

        internal static IEnumerator PlayerJoinedChecks(Player JoiningPlayer)
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
                BepinPlugin.Log.LogMessage($"Kicked player {JoiningPlayer.NickName} for not having mods.");
                Messaging.Echo($"Kicked player {JoiningPlayer.NickName} for not having mods.\n{GetModListAsStringForChat(Instance.MyModList.Where(MDB => MDB.MPType == MultiplayerType.All).ToArray())}", false);
                PhotonNetwork.CloseConnection(JoiningPlayer);
            }
            else
            {
                Events.Instance.CallHostOnClientVerified(JoiningPlayer);
            }
        }

        /// <summary>
        /// Calls vanilla isSessioModded check.
        /// </summary>
        /// <returns>ModdingUtils.IsCurrentSessionModded</returns>
        public static bool IsMod_Session()
        {
            return ModdingUtils.IsCurrentSessionModded();
        }

        internal bool ModChecksClientside(Hashtable RoomProperties, bool inRoom = true)
        {
            LastModCheckFailReason = string.Empty;
            BepinPlugin.Log.LogMessage($"Starting Clientside mod checks for room: {RoomProperties[InRoomCallbacks.OfficalRoomNamePropertyKey]}");

            bool roomToJoinIsModded = RoomIsModded(RoomProperties);


            // Stop early if host doesn't have mods but local client requires such.
            if (!roomToJoinIsModded)
            {
                if (HighestLevelOfMPMods >= MultiplayerType.Session)
                {
                    // Case: Host doesn't have mods, but Client has mod(s) which need the room to be marked as 'Mod_Session'.
                    LastModCheckFailReason = $"Host has no mods, but client has Mod_Session or higher mods.{GetModListAsString(Instance.MyModList.Where(MDB => MDB.MPType >= MultiplayerType.Session).ToArray())}";

                    KickMessagePatches.KickTitle = "Disconnected: Incompatable mod list";
                    KickMessagePatches.KickMessage = LastModCheckFailReason;
                    BepinPlugin.Log.LogMessage("Mod check failed.\n" + LastModCheckFailReason);
                    return false;
                }
                else
                {
                    // Case: Host doesn't have mods, but client doesn't have restrictive mods.
                    BepinPlugin.Log.LogMessage("Clientside mod check passed.");
                    return true;
                }
            }
            //Host must have mods beyond this point. 


            //Conditions Dictionary Init
            Dictionary<string, CheckConditions> conditions = new();


            //Selects only mods which have MPType set to all for comparison.
            MPUserDataBlock HostModData = DeserializeHashlessMPUserData((byte[])RoomProperties[InRoomCallbacks.RoomModsPropertyKey]);
            MPModDataBlock[] HostMods = HostModData.ModData;

            BepinPlugin.Log.LogMessage($"Void Manager versions - Host: {HostModData.VMVersion} Client: {MyPluginInfo.PLUGIN_VERSION}");



            //Loop through Local/Client mods
            foreach (MPModDataBlock mod in MyModList)
            {
                CheckConditions condition = new();
                condition.IsMod_Session = roomToJoinIsModded;
                condition.HostCheck = false;
                condition.PlayersWithMod = PlayersWithMod.Client;
                condition.Mod = mod;
                condition.ClientModVersion = mod.Version;

                conditions.Add(mod.ModGUID, condition);
            }

            //Loop through Host mods
            foreach (MPModDataBlock mod in HostMods)
            {
                CheckConditions condition;
                if (conditions.TryGetValue(mod.ModGUID, out condition))
                {
                    condition.ClientModVersion = mod.Version;
                    condition.PlayersWithMod = PlayersWithMod.Both;
                }
                else
                {
                    condition = new();
                    condition.PlayersWithMod = PlayersWithMod.Host;
                    condition.HostCheck = false;
                    condition.Mod = mod;
                    condition.HostModVersion = mod.Version;
                    conditions.Add(mod.ModGUID, condition);
                }
            }


            //Fail reasons for messaging later.
            List<FailInfo> Session = new();
            List<FailInfo> MismatchedVersions = new();
            List<FailInfo> AllClientLacking = new();
            List<FailInfo> AllHostLacking = new();
            List<FailInfo> Custom = new();

            //Check if problems were found and add to error message.
            string errorMessage = string.Empty;


            if (Session.Count > 0) //Case: Client and Host have mismatched mod versions
            {
                errorMessage += "The following mods require rooms marked as Mod_Session:\n";
                foreach (FailInfo FI in Session)
                {
                    errorMessage += FI.FailingMod.ModName + "\n";
                }
            }
            if (MismatchedVersions.Count > 0) //Case: Client and Host have mismatched mod versions
            {
                errorMessage += "The following mods have mismatched versions:\n";
                foreach (FailInfo FI in MismatchedVersions)
                {
                    errorMessage += FI.FailingMod.ModName + "\n";
                }
            }
            if (AllClientLacking.Count > 0) //Case: Client is missing mods required by the host.
            {
                errorMessage += "The following mods are required to join the session:\n";
                foreach (FailInfo FI in AllClientLacking)
                {
                    errorMessage += FI.FailingMod.ModName + "\n";
                }
            }
            if (AllHostLacking.Count > 0) //Case: Client has mods which need to be installed by the host.
            {
                errorMessage += "The following mods must be uninstalled to join the session:";
                foreach (FailInfo FI in AllHostLacking)
                {
                    errorMessage += "\n" + FI.FailingMod.ModName;
                }
            }

            //return false and finalize error message.
            if (errorMessage != string.Empty)
            {
                if (inRoom) //Provide kickedMessage popup
                {
                    KickMessagePatches.KickTitle = "Disconnected: Incompatable mod list";
                    KickMessagePatches.KickMessage = errorMessage;
                }
                else //Provide Terminal fail to join popup
                {
                    LastModCheckFailReason = errorMessage;
                }


                BepinPlugin.Log.LogMessage("Couldn't join session.\n" + errorMessage);
                return false;
            }

            //Mod check passed.
            BepinPlugin.Log.LogMessage("Clientside mod check passed.");
            return true;
        }

        internal void ModChecksHostOnClientJoin(Player joiningPlayer)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }


            //Collect Client/Host mods
            MPUserDataBlock JoiningPlayerMPData = GetNetworkedPeerMods(joiningPlayer);
            MPModDataBlock[] JoiningClientMods = JoiningPlayerMPData.ModData;

            //Conditions Dictionary Init
            Dictionary<string, CheckConditions> conditions = new();


            //Loop through Local/Host mods
            foreach(MPModDataBlock mod in MyModList)
            {
                CheckConditions condition = new CheckConditions();
                condition.IsMod_Session = IsMod_Session();
                condition.HostCheck = true;
                condition.PlayersWithMod = PlayersWithMod.Host;
                condition.Mod = mod;
                condition.HostModVersion = mod.Version;

                conditions.Add(mod.ModGUID, condition);
            }

            //Loop through Joining Client mods
            foreach(MPModDataBlock mod in JoiningClientMods)
            {
                CheckConditions condition;
                if (conditions.TryGetValue(mod.ModGUID, out condition))
                {
                    condition.ClientModVersion = mod.Version;
                    condition.PlayersWithMod = PlayersWithMod.Both;
                    condition.HashesMatch = mod.Hash.SequenceEqual(condition.Mod.Hash);
                }
                else
                {
                    condition = new CheckConditions();
                    condition.PlayersWithMod = PlayersWithMod.Client;
                    condition.HostCheck = true;
                    condition.Mod = mod;
                    condition.ClientModVersion = mod.Version;
                    conditions.Add(mod.ModGUID, condition);
                }
            }


            //Fail reasons for messaging later.
            List<FailInfo> Session = new();
            List<FailInfo> MismatchedVersions = new();
            List<FailInfo> AllClientLacking = new();
            List<FailInfo> AllHostLacking = new();
            List<FailInfo> Custom = new();


            //Check mods and gather failDetails.
            foreach (CheckConditions condition in conditions.Values)
            {
                FailInfo failDetail = CheckMod(condition);
                switch (failDetail.CheckFailReason)
                {
                    case CheckFailReason.Session:
                        Session.Add(failDetail);
                        break;
                    case CheckFailReason.MismatchedVersions:
                        MismatchedVersions.Add(failDetail);
                        break;
                    case CheckFailReason.AllClientLacking:
                        AllClientLacking.Add(failDetail);
                        break;
                    case CheckFailReason.AllHostLacking:
                        AllHostLacking.Add(failDetail);
                        break;
                    case CheckFailReason.Custom:
                        Custom.Add(failDetail);
                        break;
                }
            }
            
            
            string errorMessage = string.Empty;

            if (Session.Count > 0) //Case: Client and Host have mismatched mod versions
            {
                errorMessage += "The following mods require rooms marked as Mod_Session\n";
                foreach (FailInfo FI in Session)
                {
                    errorMessage += FI.FailingMod.ModName + "\n";
                }
            }
            if (MismatchedVersions.Count > 0) //Case: Client and Host have mismatched mod versions
            {
                errorMessage += "The following mods have mismatched versions:\n";
                foreach (FailInfo FI in MismatchedVersions)
                {
                    errorMessage += FI.FailingMod.ModName + "\n";
                }
            }
            if (AllClientLacking.Count > 0) //Case: Client is missing mods required by the host.
            {
                errorMessage += "The following mods are required to join the session:\n";
                foreach (FailInfo FI in AllClientLacking)
                {
                    errorMessage += FI.FailingMod.ModName + "\n";
                }
            }
            if (AllHostLacking.Count > 0) //Case: Client has mods which need to be installed by the host.
            {
                errorMessage += "The following mods must be uninstalled to join the session:";
                foreach (FailInfo FI in AllHostLacking)
                {
                    errorMessage += "\n" + FI.FailingMod.ModName;
                }
            }

            //Kick Player and finalize error message.
            if (errorMessage != string.Empty)
            {
                //Send message to joining client.
                Messaging.Echo($"Kicking player {joiningPlayer.NickName} from session for incompatable mods.\n{errorMessage}", false);
                Messaging.KickMessage("Kicked: Incompatable mod list", errorMessage, joiningPlayer);
                PhotonNetwork.CloseConnection(joiningPlayer);
                BepinPlugin.Log.LogMessage($"Kicked player {joiningPlayer.NickName} from session for incompatable mods.\n{errorMessage}");
            }
            else
            {
                BepinPlugin.Log.LogMessage("Hostside mod check passed for player " + joiningPlayer.NickName);
                Events.Instance.CallHostOnClientVerified(joiningPlayer);
            }
        }

        internal FailInfo CheckMod(CheckConditions Conditions)
        {
            FailInfo failDetails = new(); //Default NoFail
            failDetails.FailingMod = Conditions.Mod;

            switch (Conditions.Mod.MPType)
            {
                case MultiplayerType.All:
                    if (Conditions.PlayersWithMod == PlayersWithMod.Both)
                    {
                        if (Conditions.HostModVersion == Conditions.ClientModVersion)
                        {
                            if (Conditions.HashesMatch || !Conditions.HostCheck) // Hash check only matters if host is checking.
                            {
                                failDetails.CheckFailReason = CheckFailReason.NoFail;
                                break;
                            }
                            BepinPlugin.Log.LogMessage($"Mismatched mod hash - {Conditions.Mod.ModName}");
                        }
                        else BepinPlugin.Log.LogMessage($"Mismatched mod version - Client:{Conditions.Mod.ModName}-{Conditions.ClientModVersion}, Host:{Conditions.HostModVersion}");
                        failDetails.CheckFailReason = CheckFailReason.MismatchedVersions; //Report Mismatched version for both hash and version fails.
                        break;
                    }

                    // All-required mod and a client or the host don't have the mod.
                    if (Conditions.PlayersWithMod == PlayersWithMod.Client)
                    {
                        failDetails.CheckFailReason = CheckFailReason.AllHostLacking;
                        BepinPlugin.Log.LogMessage($"Host is missing the required mod '{Conditions.Mod.ModName}'");
                    }
                    else
                    {
                        failDetails.CheckFailReason = CheckFailReason.AllClientLacking;
                        BepinPlugin.Log.LogMessage($"Client is missing the required mod '{Conditions.Mod.ModName}'");
                    }
                    break;


                // If a user has one of these mods, the session must be Mod_Session. If it is not, the host or joining client should fail connection.
                case MultiplayerType.Session:
                    if (Conditions.IsMod_Session)
                    {
                        failDetails.CheckFailReason = CheckFailReason.NoFail;
                    }
                    else
                    {
                        BepinPlugin.Log.LogMessage($"User attempting to join session with MPType.Session mod.");
                        failDetails.CheckFailReason = CheckFailReason.Session;
                    }
                    break;


                // These mods will enable/disable themselves as needed.
                case MultiplayerType.Client: 
                case MultiplayerType.Host:
                    failDetails.CheckFailReason = CheckFailReason.NoFail;
                    break;
            }

            //All other MPTypes will default to allow.
            return failDetails;
        }

        /// <summary>
        /// Contains data for a given mod pair when being checked.
        /// </summary>
        public struct CheckConditions
        {
            /// <summary>
            /// Contains data for a given mod pair when being checked.
            /// </summary>
            public CheckConditions()
            {
            }

            /// <summary>
            /// The mod belonging to the ChecKConditions instance.
            /// </summary>
            public MPModDataBlock Mod;

            /// <summary>
            /// If host is calling, true; If Client is calling, false.
            /// </summary>
            public bool HostCheck = false;

            /// <summary>
            /// If Client, Host, or both players have this mod installed.
            /// </summary>
            public PlayersWithMod PlayersWithMod;

            /// <summary>
            /// Version string of client's mod.
            /// </summary>
            public string ClientModVersion = string.Empty;

            /// <summary>
            /// Version string of host's mod.
            /// </summary>
            public string HostModVersion = string.Empty;

            /// <summary>
            /// Current session is Mod_Session.
            /// </summary>
            public bool IsMod_Session;

            /// <summary>
            /// If is HostCheck and sha256 hash sent by client matches host's hash.
            /// </summary>
            public bool HashesMatch = false;
        }

        /// <summary>
        /// The response from CheckMod
        /// </summary>
        public struct FailInfo
        {
            /// <summary>
            /// The response from CheckMod
            /// </summary>
            public FailInfo()
            {
            }

            /// <summary>
            /// An error message for custom fail reasons.
            /// </summary>
            public string FailMessage = string.Empty;

            /// <summary>
            /// The error reason.
            /// </summary>
            public CheckFailReason CheckFailReason;

            //Tracks failling mod with fail reason.
            internal MPModDataBlock FailingMod;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public enum PlayersWithMod : byte
        {
            Client,
            Host,
            Both
        }

        public enum CheckFailReason : byte
        {
            NoFail,
            MismatchedVersions,
            AllClientLacking,
            AllHostLacking,
            Session,
            Custom
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
