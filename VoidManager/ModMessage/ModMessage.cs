using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;

namespace VoidManager.ModMessages
{
    /// <summary>
    /// Abstract class for ModMessages.
    /// </summary>
    public abstract class ModMessage
    {
        /// <summary>
        /// Gets the unique identifier for this ModMessaeg
        /// </summary>
        /// <returns>namespace.name</returns>
        public string GetIdentifier() => GetType().Namespace + "." + GetType().Name;

        /// <summary>
        /// Gets the unique identifier for the given ModMessage
        /// </summary>
        /// <param name="ModMessageType"></param>
        /// <returns></returns>
        public static string GetIdentifier(Type ModMessageType) => ModMessageType.Namespace + "." + ModMessageType.Name;

        /// <summary>
        /// Send data to a Photon Player's mod specified by PluginGUID and handlerIdentifier
        /// </summary>
        /// <param name="pluginGUID">BepinPlugin.GUID</param>
        /// <param name="handlerIdentifier">VoidManager.ModMessage.GetIdentifier()</param>
        /// <param name="player"></param>
        /// <param name="arguments"></param>
        /// <param name="reliable">Send as reliable event</param>
        public static void Send(string pluginGUID, string handlerIdentifier, Player player, object[] arguments, bool reliable = false)
            => Send(pluginGUID, handlerIdentifier, new Player[] { player }, arguments, reliable);

        /// <summary>
        /// Send data to multiple Photon Player's mod specified by PluginGUID and handlerIdentifier
        /// </summary>
        /// <param name="pluginGUID">BepinPlugin.GUID</param>
        /// <param name="handlerIdentifier">VoidManager.ModMessage.GetIdentifier()</param>
        /// <param name="players">Array of players</param>
        /// <param name="arguments"></param>
        /// <param name="reliable">Send as reliable event</param>
        public static void Send(string pluginGUID, string handlerIdentifier, Player[] players, object[] arguments, bool reliable = false)
        {
            object[] information = new object[] { pluginGUID, handlerIdentifier};
            information = information.Concat(arguments).ToArray();

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.TargetActors = players.Select(player => player.ActorNumber).ToArray();

            PhotonNetwork.RaiseEvent(InRoomCallbacks.ModMessageEventCode, information, raiseEventOptions, 
                (reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable));
        }

        /// <summary>
        /// Send data to multiple Photon Player's mod specified by PluginGUID and handlerIdentifier
        /// </summary>
        /// <param name="pluginGUID">BepinPlugin.GUID</param>
        /// <param name="handlerIdentifier">VoidManager.ModMessage.GetIdentifier()</param>
        /// <param name="recieverGroup">Photon.Realtime.ReceiverGroup (Others, All, Master)</param>
        /// <param name="arguments"></param>
        /// <param name="reliable">Send as reliable event</param>
        public static void Send(string pluginGUID, string handlerIdentifier, ReceiverGroup recieverGroup, object[] arguments, bool reliable = false)
        {
            object[] information = new object[] { pluginGUID, handlerIdentifier};
            information = information.Concat(arguments).ToArray();

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.Receivers = recieverGroup;

            PhotonNetwork.RaiseEvent(InRoomCallbacks.ModMessageEventCode, information, raiseEventOptions,
                (reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable));
        }

        /// <summary>
        /// Recieve data from other players
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="sender"></param>
        public abstract void Handle(object[] arguments, Player sender);
    }
}
