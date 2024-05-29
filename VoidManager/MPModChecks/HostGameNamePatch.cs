using HarmonyLib;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(PhotonService), "SetCurrentRoomName")]
    internal class HostGameNamePatch
    {
        static void Prefix(ref string name)
        {
            switch (MPModCheckManager.Instance.HighestLevelOfMPMods)
            {
                case MultiplayerType.Host:
                case MultiplayerType.Unspecified:
                    if (!name.StartsWith("[Modded", System.StringComparison.CurrentCultureIgnoreCase) &&
                        !name.StartsWith("Modded", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        name = "[Modded] " + name;
                    }
                    break;
                case MultiplayerType.All:
                    if (!name.StartsWith("[Mods Required", System.StringComparison.CurrentCultureIgnoreCase) &&
                        !name.StartsWith("Mods Required", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        name = "[Mods Required] " + name;
                    }
                    break;
            }
        }
    }
}
