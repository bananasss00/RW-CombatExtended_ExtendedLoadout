using System.Collections.Generic;
using HarmonyLib;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Loadout))]
    public static class Loadout_Patch
    {
        static bool Prepare() => ConfigDefOf.Config.useHpAndQualityInLoadouts;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Loadout.Copy), typeof(Loadout))]
        private static void Copy(Loadout source, Loadout __result)
        {
            CE_LoadoutExtended.CopyLoadoutExtended(source, __result);
        }
    }
}