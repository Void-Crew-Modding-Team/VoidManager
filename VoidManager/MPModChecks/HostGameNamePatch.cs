using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using VoidManager.Utilities;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(PhotonService))]
    internal class HostGameNamePatch
    {
        [HarmonyPatch("SetCurrentRoomName")]
        static void Prefix(ref string name)
        {
            name = SetGameName(name);
        }

        [HarmonyPatch("PhotonCreateRoom")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new()
            {
                new CodeInstruction(OpCodes.Ldstr, " Room")
            };
            List<CodeInstruction> patchSequence = new()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HostGameNamePatch), nameof(SetGameName)))
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, patchSequence, HarmonyHelpers.PatchMode.BEFORE, HarmonyHelpers.CheckMode.ALWAYS);
        }

        public static string SetGameName(string name)
        {
            switch (MPModCheckManager.Instance.HighestLevelOfMPMods)
            {
                case MultiplayerType.Host:
                case MultiplayerType.Unspecified:
                    if (!name.StartsWith("[Modded", System.StringComparison.CurrentCultureIgnoreCase) &&
                        !name.StartsWith("Modded", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        return "[Modded] " + name;
                    }
                    break;
                case MultiplayerType.All:
                    if (!name.StartsWith("[Mods Required", System.StringComparison.CurrentCultureIgnoreCase) &&
                        !name.StartsWith("Mods Required", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        return "[Mods Required] " + name;
                    }
                    break;
            }
            return name;
        }
    }
}
