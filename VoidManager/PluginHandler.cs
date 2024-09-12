using BepInEx;
using BepInEx.Bootstrap;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using VoidManager.Chat.Router;
using VoidManager.CustomGUI;
using VoidManager.ModMessages;
using VoidManager.MPModChecks;

namespace VoidManager
{
    static class PluginHandler
    {
        public static Dictionary<string, PluginInfo> ActiveBepinPlugins { get => Chainloader.PluginInfos; }
        public static Dictionary<string, VoidPlugin> ActiveVoidPlugins { get; private set; }
        public static Dictionary<string, VoidPlugin> GeneratedVoidPlugins { get; private set; }

        /// <summary>
        /// Iterates through the current plugin files and searches for VoidPlugins.
        /// </summary>
        internal static void DiscoverPlugins()
        {
            VoidPlugin voidPlugin;

            BepinPlugin.Bindings.LoadModListOverride();
            Dictionary<string, MultiplayerType> OverridenMods = BepinPlugin.Bindings.ModOverrideDictionary;

            ActiveVoidPlugins = new();
            GeneratedVoidPlugins = new();
            foreach (PluginInfo CurrentBepinPlugin in Chainloader.PluginInfos.Values)
            {
                Assembly assembly = CurrentBepinPlugin.Instance.GetType().Assembly;
                string BPluginGUID = CurrentBepinPlugin.Metadata.GUID;
                string BPluginName = CurrentBepinPlugin.Metadata.Name;
                // Finds VoidPlugin class.
                if (BPluginGUID == MyPluginInfo.PLUGIN_GUID)
                {
                    CommandHandler.DiscoverCommands(assembly, BPluginName);
                    CommandHandler.DiscoverPublicCommands(assembly, BPluginName);
                    ModMessageHandler.DiscoverModMessages(assembly, CurrentBepinPlugin);
                    continue;
                }
                try
                {
                    IEnumerable<Type> voidPluginInstances = assembly.GetTypes().Where(t => typeof(VoidPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    if (voidPluginInstances.Any())
                    {
                        voidPlugin = (VoidPlugin)Activator.CreateInstance(voidPluginInstances.First());
                        voidPlugin.VersionInfo = FileVersionInfo.GetVersionInfo(CurrentBepinPlugin.Location);
                        voidPlugin.ModHash = GetFileHash(CurrentBepinPlugin.Location);
                        voidPlugin.BepinPlugin = CurrentBepinPlugin;
                        CommandHandler.DiscoverCommands(assembly, BPluginName);
                        CommandHandler.DiscoverPublicCommands(assembly, BPluginName);
                        ModMessageHandler.DiscoverModMessages(assembly, CurrentBepinPlugin);
                        GUIMain.Instance.DiscoverGUIMenus(assembly, voidPlugin);
                        ActiveVoidPlugins.Add(CurrentBepinPlugin.Metadata.GUID, voidPlugin);
                    }
                    else
                    {
                        MultiplayerType MPType;
                        if (OverridenMods.TryGetValue(BPluginGUID, out MPType) || OverridenMods.TryGetValue(BPluginName, out MPType)) //Overrides Unmanaged type mods with input from config.
                        {
                            voidPlugin = new DefaultVoidPlugin(MPType);
                            BepinPlugin.Log.LogInfo($"Discovered MPType override for {BPluginName}. Setting MPType to {MPType}");
                        }
                        else
                        {
                            voidPlugin = new DefaultVoidPlugin(MultiplayerType.Unmanaged);
                        }

                        voidPlugin.VersionInfo = FileVersionInfo.GetVersionInfo(CurrentBepinPlugin.Location);
                        voidPlugin.ModHash = GetFileHash(CurrentBepinPlugin.Location);
                        voidPlugin.BepinPlugin = CurrentBepinPlugin;
                        GUIMain.Instance.DiscoverNonVoidManagerMod(voidPlugin);
                        GeneratedVoidPlugins.Add(BPluginGUID, voidPlugin);
                    }
                }
                catch (Exception ex)
                {
                    BepinPlugin.Log.LogError($"Error loading mod '{BPluginName}'\n{ex}");
                }
            }
            GUIMain.Instance.settings = GUIMain.Instance.settings.OrderByDescending(v => v is VManSettings).ThenBy(v => v.Name()).ToList();
            GUIMain.Instance.mods.Sort((plugin1, plugin2) => plugin1.BepinPlugin.Metadata.Name.CompareTo(plugin2.BepinPlugin.Metadata.Name));
            BepinPlugin.Log.LogInfo($"Loaded {CommandHandler.chatCommandCount} local command(s) and {CommandHandler.publicCommandCount} public command(s)");
            BepinPlugin.Log.LogInfo($"Loaded {ModMessageHandler.modMessageHandlers.Count()} mod message(s)");
            BepinPlugin.Log.LogInfo($"Discovered {ActiveVoidPlugins.Count} {MyPluginInfo.PLUGIN_NAME} plugin(s) from {ActiveBepinPlugins.Count - 1} mod(s)");
        }

        internal static void OnSessionChanged(object sender, SessionChangedInput e)
        {
            /* Last two values of input are fairly generalised IsMod_Session = isMod_Session; HostHasMod = hostHasMod;
             * so it will be handled here
            
             * Host Start            SessionChangedInput(true, CallType.Hosting, true)
             * Join Room             SessionChangedInput(false, CallType.Joining, PhotonNetwork.MasterClient.IsLocal)
             * Host Change           SessionChangedInput(newMasterClient.IsLocal, CallType.HostChange, false)
             * Session Escalation    SessionChangedInput(PhotonNetwork.MasterClient.IsLocal, CallType.SessionEscalated)
            */
            //BepinPlugin.Log.LogInfo($"[OnSessionChanged - Event] {e.CallType} | {e.IsHost} {e.CallType} {e.StartedAsHost}");

            if (MPModCheckManager.RoomIsModded(PhotonNetwork.CurrentRoom)) e.IsMod_Session = true;
            MPUserDataBlock userData = null;
            if (!e.IsHost) userData = NetworkedPeerManager.Instance.GetHostModList();
            e.HostHasMod = true;

            bool MarkAsModSession = false;
            foreach (VoidPlugin voidPlugin in ActiveVoidPlugins.Values)
            {
                // Check for VoidPlugin in Hosts list for `HostHasMod` condition
                if (userData != null)
                {
                    foreach (MPModDataBlock mPModDataBlock in userData.ModData)
                    {
                        if (mPModDataBlock.ModGUID == voidPlugin.BepinPlugin.Metadata.GUID) break;
                    }
                    e.HostHasMod = false;
                }
                //BepinPlugin.Log.LogInfo($"[OnSessionChanged - Call] {e.CallType} | {voidPlugin.BepinPlugin.Metadata.Name} | {e.IsHost} {e.CallType} {e.StartedAsHost} {e.IsMod_Session} {e.HostHasMod}");
                SessionChangedReturn result = voidPlugin.OnSessionChange(e);
                if (result.SetMod_Session || voidPlugin.MPType == MultiplayerType.Session || voidPlugin.MPType == MultiplayerType.All) MarkAsModSession = true;
            }
        }

        public static byte[] GetFileHash(string fileLocation)
        {
            using (SHA256 Hasher = SHA256.Create())
            {
                return Hasher.ComputeHash(File.ReadAllBytes(fileLocation));
            }
        }
    }
}
