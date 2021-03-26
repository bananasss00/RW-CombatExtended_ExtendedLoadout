using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public class ExtendedLoadoutMod : ModBase
    {
        public static ExtendedLoadoutMod Instance;
        // indicate need patches or not
        public bool useMultiLoadouts, useHpAndQualityInLoadouts;

        protected override bool HarmonyAutoPatch => false;

        private ModSettingsPack modSettingsPack;

        public ExtendedLoadoutMod()
        {
            Instance = this;
        }

        public override string ModIdentifier => "CombatExtended.ExtendedLoadout";

        public override void DefsLoaded()
        {
            // init settings
            modSettingsPack = HugsLibController.Instance.Settings.GetModSettings("CombatExtended.ExtendedLoadout");
            var UseHpAndQualityInLoadouts = modSettingsPack.GetHandle($"UseHpAndQualityInLoadouts", "Settings.UseHpAndQualityInLoadouts.Label".Translate(), "Settings.UseHpAndQualityInLoadouts.Desc".Translate(), true);
            var UseMultiLoadouts = modSettingsPack.GetHandle($"UseMultiLoadouts", "Settings.UseMultiLoadouts.Label".Translate(), "Settings.UseMultiLoadouts.Desc".Translate(), true);
            var MultiLoadoutsCount = modSettingsPack.GetHandle($"MultiLoadoutsCount", "Settings.MultiLoadoutsCount.Label".Translate(), "Settings.MultiLoadoutsCount.Desc".Translate(), 3, value => int.TryParse(value, out int num) && num >= 2 && num <= 10);
                MultiLoadoutsCount.VisibilityPredicate = () => UseMultiLoadouts;

            // inject columns and set settings  
            useHpAndQualityInLoadouts = UseHpAndQualityInLoadouts;
            if (UseMultiLoadouts && MultiLoadoutsCount >= 2 && MultiLoadoutsCount <= 10)
            {
                var assign = DefDatabase<PawnTableDef>.GetNamed("Assign");
                var pawnColumnDefs = assign.columns;
                int idx = pawnColumnDefs.FindIndex(x => x.defName.Equals("Loadout"));
                if (idx != -1)
                {
                    pawnColumnDefs.RemoveAt(idx);
                    pawnColumnDefs.InsertRange(idx, GeneratePawnColumnDefs(MultiLoadoutsCount));
                    Loadout_Multi.ColumnsCount = MultiLoadoutsCount;
                    useMultiLoadouts = true;
                    Log.Message($"[CombatExtended.ExtendedLoadout] {MultiLoadoutsCount}x Loadout columns injected");
                } else Log.Error($"[CombatExtended.ExtendedLoadout] Can't find CE Loadout column");
            }

            // apply patches
            var h = new Harmony("PirateBY.CombatExtended.ExtendedLoadout");
            if (useMultiLoadouts && ModActive.BetterPawnControl)
                BPC.Patch(h);
            h.PatchAll();

            /**
             * Fix for mod: Rim of Madness - Vampires
             * After added postfix from vapires on method JobGiver_UpdateLoadout:TryGiveJob
             * JIT inline loadout.Slots in this method???
             * 
             * operation Harmony::Unpatch or Harmony::Patch prevent inlining!
             * h.Unpatch(AccessTools.Method("CombatExtended.JobGiver_UpdateLoadout:TryGiveJob"), HarmonyPatchType.All, "some.random.string");
             */
            h.Unpatch(AccessTools.Method("CombatExtended.JobGiver_UpdateLoadout:TryGiveJob"), HarmonyPatchType.All, "some.random.string");
            Log.Message("[CombatExtended.ExtendedLoadout] Initialized");
        }

        private IEnumerable<PawnColumnDef> GeneratePawnColumnDefs(int count)
        {
            for (int i = 0; i < count; i++)
                yield return new PawnColumnDef()
                {
                    defName = $"Loadout_{i}",
                    workerClass = typeof(PawnColumnWorker_Loadout_Multi),
                    label = $"Loadout{i + 1}".Translate(),
                    sortable = true
                };
        }
    }
}
