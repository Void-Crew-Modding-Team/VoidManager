using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VoidManager.Callbacks;

namespace VoidManager.MPModChecks
{
    [HarmonyPatch(typeof(PhotonService), "PhotonCreateRoom")]
    internal class RoomInfoPatch
    {
        static RoomOptions PatchMethod(RoomOptions RoomOptions)
        {
            RoomOptions.CustomRoomProperties[InRoomCallbacks.RoomModsPropertyKey] = MPModCheckManager.Instance.RoomProperties;
            RoomOptions.CustomRoomProperties[InRoomCallbacks.OfficalModdedPropertyKey] = true;

            //Rebuild CRPFL array with new value. Litterally adding an index to an array.
            int CRPFLLength = RoomOptions.CustomRoomPropertiesForLobby.Length;
            string[] NewCRPFL = new string[CRPFLLength + 1];
            int i; //declair i outside of for loop.
            for (i = 0; i < CRPFLLength; i++)
            {
                NewCRPFL[i] = RoomOptions.CustomRoomPropertiesForLobby[i];
            }
            NewCRPFL[i] = InRoomCallbacks.RoomModsPropertyKey; //i was incremented and is still usefull.
            RoomOptions.CustomRoomPropertiesForLobby = NewCRPFL;

            //Compliance. Should be removed after update 5.
            if (!RoomOptions.CustomRoomPropertiesForLobby.Contains(InRoomCallbacks.OfficalModdedPropertyKey))
            {
                CRPFLLength++;
                NewCRPFL = new string[CRPFLLength + 1];
                for (i = 0; i < CRPFLLength; i++)
                {
                    NewCRPFL[i] = RoomOptions.CustomRoomPropertiesForLobby[i];
                }
                NewCRPFL[i] = InRoomCallbacks.OfficalModdedPropertyKey; //i was incremented and is still usefull.
                RoomOptions.CustomRoomPropertiesForLobby = NewCRPFL;
            }

            return RoomOptions;
        }
        [HarmonyTranspiler]
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
                        BepinPlugin.Log.LogError("Failed to patch PhotonService.PhotonCreateRoom. Targeted method appears to have changed. Index: " + i.ToString());
                    }
                    break;
                }
            }
            return InstructionList.AsEnumerable();
        }
    }
}
