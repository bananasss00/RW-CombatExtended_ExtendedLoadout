using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HugsLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [DefOf]
    public static class ConfigDefOf
    {
        static ConfigDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ConfigDefOf));
        }

        public static Config Config;
    }

    public class Config : Def
    {
        public int columnsCount;
    }

    public class ExtendedLoadoutMod : ModBase
    {
        protected override bool HarmonyAutoPatch => false;

        public ExtendedLoadoutMod()
        {
            
        }

        public override void DefsLoaded()
        {
            var h = new Harmony("PirateBY.CombatExtended.ExtendedLoadout");
            // TODO: columns patch if columns count valid
            h.PatchAll();
            if (BPC.Active)
                BPC.Patch(h);
            Log.Message("[CombatExtended.ExtendedLoadout] Initialized");

            if (!(ConfigDefOf.Config.columnsCount >= 2 && ConfigDefOf.Config.columnsCount <= 5))
                return;

            // inject columns
            var assign = DefDatabase<PawnTableDef>.GetNamed("Assign");
            var pawnColumnDefs = assign.columns;
            int idx = pawnColumnDefs.FindIndex(x => x.defName.Equals("Loadout"));
            if (idx == -1)
            {
                Log.Error($"[ExtendedLoadoutMod] Can't find CE Loadout column");
                return;
            }
            pawnColumnDefs.RemoveAt(idx);
            pawnColumnDefs.InsertRange(idx, GeneratePawnColumnDefs());
        }

        private IEnumerable<PawnColumnDef> GeneratePawnColumnDefs()
        {
            var columns = typeof(PawnColumnWorker_Loadout_Multi)
                .AllSubclasses()
                .OrderBy(x => x.Name)
                .ToList();

            for (int i = 0; i < ConfigDefOf.Config.columnsCount; i++)
                yield return new PawnColumnDef()
                {
                    defName = $"Loadout{i}",
                    workerClass = columns[i],
                    label = $"Loadout{i + 1}",
                    sortable = true
                };
        }
    }
}
