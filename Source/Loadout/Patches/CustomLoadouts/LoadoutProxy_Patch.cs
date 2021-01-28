using System.Collections.Generic;
using HarmonyLib;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Loadout))]
    public static class LoadoutProxy_Patch
    {
        static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Loadout.SlotCount), MethodType.Getter)]
        public static bool SlotCount(Loadout __instance, ref int __result)
        {
            if (__instance is Loadout_Multi loadout)
            {
                __result = loadout.SlotCount;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Loadout.Slots), MethodType.Getter)]
        public static bool Slots(Loadout __instance, ref List<LoadoutSlot> __result)
        {
            if (__instance is Loadout_Multi loadout)
            {
                __result = loadout.Slots;
                return false;
            }
            return true;
        }
    }
}