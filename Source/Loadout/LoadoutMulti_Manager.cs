using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public static class LoadoutMulti_Manager
    {
        private static List<Pawn> keysWorkingList;
        private static List<Loadout_Multi> valuesWorkingList;
        private static Dictionary<Pawn, Loadout_Multi> assignedLoadoutsMulti = new Dictionary<Pawn, Loadout_Multi>();

        public static void ExposeData()
        {
            Scribe_Collections.Look(ref assignedLoadoutsMulti, "assignedLoadoutsMulti", LookMode.Reference, LookMode.Deep, ref keysWorkingList, ref valuesWorkingList);
        }

        public static void ClearData()
        {
            assignedLoadoutsMulti.Clear();
        }

        public static int GetUniqueLoadoutID()
        {
            var loadoutsMulti = assignedLoadoutsMulti.Values;
            if (loadoutsMulti.Any())
                return loadoutsMulti.Max(l => l.uniqueID) + 1;
            return 1;
        }

        public static Loadout GetLoadout(Pawn pawn)
        {
            if (!assignedLoadoutsMulti.TryGetValue(pawn, out var loadout))
            {
                loadout = new Loadout_Multi();
                assignedLoadoutsMulti.Add(pawn, loadout);
            }
            return loadout;
        }

        public static void SetLoadout1(this Pawn pawn, Loadout loadout)
        {
            if (pawn == null)
                throw new ArgumentNullException("pawn");

            if (assignedLoadoutsMulti.ContainsKey(pawn))
                assignedLoadoutsMulti[pawn].Loadout1 = loadout;
            else
                assignedLoadoutsMulti.Add(pawn, new Loadout_Multi() {Loadout1 = loadout});
        }

        public static void SetLoadout2(this Pawn pawn, Loadout loadout)
        {
            if (pawn == null)
                throw new ArgumentNullException("pawn");

            if (assignedLoadoutsMulti.ContainsKey(pawn))
                assignedLoadoutsMulti[pawn].Loadout2 = loadout;
            else
                assignedLoadoutsMulti.Add(pawn, new Loadout_Multi() {Loadout2 = loadout});
        }
    }
}