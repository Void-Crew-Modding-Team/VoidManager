using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace VoidManager.ModMessage
{
    public class RecieveModMessage : MonoBehaviourPun, IOnEventCallback
    {
        internal const byte ModMessageEventCode = 98;
        public static RecieveModMessage Instance = null;
        public RecieveModMessage()
        {
            Instance = this;
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (ModMessageEventCode == photonEvent.Code)
            {
                Player Sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(photonEvent.Sender);
                object[] args = (object[])photonEvent.CustomData;

                //Fail in event args cannot possibly work for a proper ModMessage
                if (args == null || args.Length < 2)
                {
                    Plugin.Log.LogInfo($"Recieved Invalid ModMessage from {Sender.NickName ?? "N/A"}");
                    return;
                }


                if (ModMessageHandler.modMessageHandlers.TryGetValue($"{args[0]}#{args[1]}", out ModMessage modMessage))
                {
                    modMessage.Handle((args.Length > 2 ? args.Skip(2).ToArray() : null), Sender);
                }

                //Fail in event targetted ModMessage was not found.
                Plugin.Log.LogInfo($"Recieved Unrecognised ModMessage ({args[0] ?? "N/A"}#{args[1] ?? "N/A"}) from {Sender.NickName ?? "N/A"}");
            }
        }
    }
}
