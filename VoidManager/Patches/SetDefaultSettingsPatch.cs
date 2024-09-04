using HarmonyLib;

namespace VoidManager.Patches
{
    [HarmonyPatch(typeof(GeneralSettings), "Load")]
    class SetDefaultSettingsPatch
    {
        static void Postfix(GeneralSettings __instance)
        {
            __instance.JoinModdedGames.SetValue(1);
        }
    }
}
