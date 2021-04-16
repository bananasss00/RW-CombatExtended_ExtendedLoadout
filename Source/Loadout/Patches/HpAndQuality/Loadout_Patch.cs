using HarmonyLib;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Loadout))]
    public static class Loadout_Patch
    {
        static bool Prepare() => ExtendedLoadoutMod.Instance.useHpAndQualityInLoadouts;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Loadout.Copy), typeof(Loadout))]
        private static void Copy(Loadout source, Loadout __result)
        {
            Loadout_Extended.Copy(source, __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Loadout.ExposeData))]
        private static void ExposeData(Loadout __instance)
        {
            Loadout_Extended.ExposeData(__instance);
        }
    }
}