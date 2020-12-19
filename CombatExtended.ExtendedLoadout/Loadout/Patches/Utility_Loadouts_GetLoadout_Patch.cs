﻿using HarmonyLib;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Utility_Loadouts))]
    public static class Utility_Loadouts_GetLoadout_Patch
    {
        [HarmonyPatch(nameof(Utility_Loadouts.GetLoadout))]
        [HarmonyPrefix]
        public static bool GetLoadout(Pawn pawn, ref Loadout __result)
        {
            __result = LoadoutMulti_Manager.GetLoadout(pawn);
            return false;
        }
    }
}