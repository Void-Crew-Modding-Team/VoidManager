using HarmonyLib;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(PhotonService), "SetCurrentRoomName")]
    internal class HostGameNamePatch
    {
        static void Prefix(ref string name)
        {
            if (!name.StartsWith("[Modded]", System.StringComparison.CurrentCultureIgnoreCase) &&
                !name.StartsWith("modded", System.StringComparison.CurrentCultureIgnoreCase))
            {
                name = "[Modded] " + name;
            }
        }
    }
}
