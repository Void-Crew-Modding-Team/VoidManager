using BepInEx.Logging;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Logger = VoidManager.Utilities.Logger;

namespace VoidManager.ModMessage
{
    /// <summary>
    /// Abstract class for ModMessages.
    /// </summary>
    public abstract class ModMessage
    {
        /// <summary>
        /// Gets the unique identifier for this mod
        /// </summary>
        /// <returns>namespace.name</returns>
        public string GetIdentifier() => GetType().Namespace + "." + GetType().Name;

        /// <summary>
        /// Send data to a Photon Player's mod specified by harmonyIdentifier and handlerIdentifier
        /// </summary>
        /// <param name="harmonyIdentifier">VoidCrewManager.VoidCrewMod.HarmonyIdentifier()</param>
        /// <param name="handlerIdentifier">VoidCrewManager.ModMessage.GetIdentifier()</param>
        /// <param name="player"></param>
        /// <param name="arguments"></param>
        /// <param name="reliable">Send as reliable event</param>
        public static void Send(string harmonyIdentifier, string handlerIdentifier, Player player, object[] arguments, bool reliable = false)
            => Send(harmonyIdentifier, handlerIdentifier, new Player[] { player }, arguments, reliable);

        /// <summary>
        /// Send data to multiple Photon Player's mod specified by harmonyIdentifier and handlerIdentifier
        /// </summary>
        /// <param name="harmonyIdentifier">VoidCrewManager.VoidCrewMod.HarmonyIdentifier()</param>
        /// <param name="handlerIdentifier">VoidCrewManager.ModMessage.GetIdentifier()</param>
        /// <param name="players">List of players</param>
        /// <param name="arguments"></param>
        /// <param name="reliable">Send as reliable event</param>
        public static void Send(string harmonyIdentifier, string handlerIdentifier, Player[] players, object[] arguments, bool reliable = false)
        {
            object[] information = new object[] { harmonyIdentifier, handlerIdentifier, 
                PhotonNetwork.LocalPlayer.ActorNumber};
            information.Concat(arguments);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.TargetActors = players.Select(player => player.ActorNumber).ToArray();

            PhotonNetwork.RaiseEvent(RecieveModMessage.eventCode, information, raiseEventOptions, 
                (reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable));
        }

        /// <summary>
        /// Send data to multiple Photon Player's mod specified by harmonyIdentifier and handlerIdentifier
        /// </summary>
        /// <param name="harmonyIdentifier">VoidCrewManager.VoidCrewMod.HarmonyIdentifier()</param>
        /// <param name="handlerIdentifier">VoidCrewManager.ModMessage.GetIdentifier()</param>
        /// <param name="recieverGroup">Photon.Realtime.ReceiverGroup (Others, All, Master)</param>
        /// <param name="arguments"></param>
        /// <param name="reliable">Send as reliable event</param>
        public static void Send(string harmonyIdentifier, string handlerIdentifier, ReceiverGroup recieverGroup, object[] arguments, bool reliable = false)
        {
            object[] information = new object[] { harmonyIdentifier, handlerIdentifier,
                PhotonNetwork.LocalPlayer.ActorNumber};
            information.Concat(arguments);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.Receivers = recieverGroup;

            PhotonNetwork.RaiseEvent(RecieveModMessage.eventCode, information, raiseEventOptions,
                (reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable));
        }

        /// <summary>
        /// Recieve data from other players
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="senderId"></param>
        public abstract void Handle(object[] arguments, int senderId);
    }
    public class RecieveModMessage : MonoBehaviourPun, IOnEventCallback
    {
        internal static byte eventCode = (byte)99;
        public static RecieveModMessage Instance = null;
        public RecieveModMessage()
        {
            Instance = this;
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (eventCode == photonEvent.Code)
            {
                object[] args = (object[])photonEvent.CustomData;
                if (args == null || args.Length < 3)
                {
                    Logger.Info("Recieved Invalid ModMessage");
                    return;
                }
                if (ModMessageHandler.modMessageHandlers.TryGetValue($"{args[0]}#{args[1]}", out ModMessage modMessage)
                    && int.TryParse((string)args[2], out int senderId))
                {
                    modMessage.Handle((args.Length > 3 ? args.Skip(3).ToArray() : null), senderId);
                }
                Logger.Info($"Recieved Unrecognised ModMessage ({args[0] ?? "N/A"}#{args[1] ?? "N/A"}) from {args[2] ?? "N/A"}");
            }
        }
    }
}
