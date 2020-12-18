using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Utility_HoldTracker))]
    public class Utility_HoldTracker_Patch
    {
        /// <summary>
        /// Drop only 1 weapon per tick, if original false
        /// </summary>
        [HarmonyPatch(nameof(Utility_HoldTracker.GetExcessEquipment))]
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Utility_HoldTracker_GetExcessEquipment(Pawn pawn, ref ThingWithComps dropEquipment, ref bool __result)
        {
            if (__result) return; // try find equipped weapon for drop when not has ExcessThing

            Loadout loadout = pawn.GetLoadout();
            if (loadout == null || loadout.Slots.NullOrEmpty() || pawn.equipment?.Primary == null)
                return;

            var ceLoadoutExtended = CE_LoadoutExtended.LoadoutExtended(loadout);
            var gun = pawn.equipment.Primary;
            if (gun.def.IsWeapon && !ceLoadoutExtended.AllowEquip(gun))
            {
                dropEquipment = gun;
                __result = true;
                DbgLog.Msg($"{pawn.LabelCap} drop equipped {gun}. HP_RANGE:{ceLoadoutExtended.HpRange}; QUALITY_RANGE:{ceLoadoutExtended.QualityRange}");
            }
        }

        /// <summary>
        /// Drop only 1 weapon per tick, if original false
        /// </summary>
        [HarmonyPatch(nameof(Utility_HoldTracker.GetExcessThing))]
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Utility_HoldTracker_GetExcessThing(Pawn pawn, ref Thing dropThing, ref int dropCount, ref bool __result)
        {
            if (__result) return; // try find weapon for drop when not has ExcessThing

            Loadout loadout = pawn.GetLoadout();
            if (pawn.inventory?.innerContainer == null || loadout == null || loadout.Slots.NullOrEmpty())
                return;

            var ceLoadoutExtended = CE_LoadoutExtended.LoadoutExtended(loadout);
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                Thing thing2 = thing.GetInnerIfMinified();
                if (thing2.def.IsWeapon && !ceLoadoutExtended.AllowEquip(thing2))
                {
                    dropThing = thing2;
                    dropCount = 1;
                    __result = true;
                    DbgLog.Msg($"{pawn.LabelCap} drop from inventory {dropThing}. HP_RANGE:{ceLoadoutExtended.HpRange}; QUALITY_RANGE:{ceLoadoutExtended.QualityRange}");
                    return;
                }
            }
        }
    }
}