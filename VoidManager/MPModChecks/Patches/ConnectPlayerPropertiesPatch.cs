using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using VoidManager.Callbacks;

namespace VoidManager.MPModChecks.Patches
{
    //Set Player properties on join lobby. Properties are cached and auto-propogated.
    [HarmonyPatch(typeof(PhotonService), "SetCommonPlayerProperties")]
    internal class SetLocalPlayerPropertiesPatch
    {
        static void Postfix()
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { InRoomCallbacks.PlayerModsPropertyKey, MPModCheckManager.Instance.MyModListData } });
        }
    }
}
