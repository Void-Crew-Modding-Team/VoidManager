using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(PhotonService), "PhotonCreateRoom")]
    internal class RoomInfoPatch
    {
        static RoomOptions PatchMethod(RoomOptions RoomOptions)
        {
            RoomOptions.CustomRoomProperties.Add(MPModCheckManager.RoomModsPropertyKey, MPModCheckManager.Instance.GetRoomProperties());


            //Rebuild CRPFL array with new value. Litterally adding an index to an array.
            int CRPFLLength = RoomOptions.CustomRoomPropertiesForLobby.Length;
            string[] NewCRPFL = new string[CRPFLLength + 1];
            int i; //declair i outside of for loop.
            for (i = 0; i < CRPFLLength; i++)
            {
                NewCRPFL[i] = RoomOptions.CustomRoomPropertiesForLobby[i];
            }
            NewCRPFL[i] = MPModCheckManager.RoomModsPropertyKey; //i was incremented and is still usefull.
            RoomOptions.CustomRoomPropertiesForLobby = NewCRPFL;


            return RoomOptions;
        }
        [HarmonyTranspiler]//
        private static IEnumerable<CodeInstruction> RoomPropertiesPatch(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> InstructionList = instructions.ToList();

            int count = InstructionList.Count;
            for(int i = count - 1; i >= 0; i--)
            {
                if (InstructionList[i].opcode == OpCodes.Call)
                {
                    if ((MethodInfo)InstructionList[i].operand == AccessTools.Method(typeof(PhotonNetwork), "CreateRoom"))
                    {
                        InstructionList.Insert(i - 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RoomInfoPatch), "PatchMethod")));
                    }
                    else
                    {
                        Plugin.Log.LogError("Failed to patch PhotonService.PhotonCreateRoom. Targeted method appears to have changed. Index: " + i.ToString());
                    }
                    break;
                }
            }
            return InstructionList.AsEnumerable();
        }
    }
}
