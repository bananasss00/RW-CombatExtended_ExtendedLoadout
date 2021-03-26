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
    [StaticConstructorOnStartup]
    public static class EarlyInit
    {
        static EarlyInit()
        {
            /*
             * Fix for mod: Rim of Madness - Vampires
             * After added postfix from vampires on method JobGiver_UpdateLoadout:TryGiveJob
             * 
             * Need to patch it first, before other mods patch the methods where Loadout::Slots is used
             */
            LoadoutProxy_Patch.Patch();
        }
    }

    public class ExtendedLoadoutMod : ModBase
    {
        public static ExtendedLoadoutMod Instance;
        // indicate need patches or not
        public bool useMultiLoadouts, useHpAndQualityInLoadouts;

        protected override bool HarmonyAutoPatch => false;

        private ModSettingsPack modSettingsPack;

        public static readonly string HarmonyId = "PirateBY.CombatExtended.ExtendedLoadout";
        public static Harmony Harmony => _harmony ?? (_harmony = new Harmony(HarmonyId));

        private static Harmony _harmony;

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
            if (useMultiLoadouts && ModActive.BetterPawnControl)
                BPC.Patch(Harmony);
            Harmony.PatchAll();

            if (!useMultiLoadouts) // disable unused patch
                LoadoutProxy_Patch.Unpatch();

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
