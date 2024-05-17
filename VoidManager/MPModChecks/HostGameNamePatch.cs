using HarmonyLib;
using Photon.Realtime;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(LoadBalancingClient), "OpCreateRoom")]
    internal class HostGameNamePatch
    {
        static void Prefix(EnterRoomParams enterRoomParams)
        {
            string name = enterRoomParams.RoomName.ToLower();
            if (!name.StartsWith("[modded]") && !name.StartsWith("modded"))
            {
                enterRoomParams.RoomName = "[Modded] " + enterRoomParams.RoomName;
            }
        }
    }
}
