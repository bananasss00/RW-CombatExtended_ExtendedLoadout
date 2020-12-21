﻿using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Loadout), nameof(Loadout.ExposeData))]
    public class CE_LoadoutExtended
    {
        static bool Prepare() => ConfigDefOf.Config.useHpAndQualityInLoadouts;

        private static int _ticks;
        private static readonly Dictionary<Loadout, CE_LoadoutExtended> Loadouts = new Dictionary<Loadout, CE_LoadoutExtended>();

        public Loadout Loadout;
        public FloatRange HpRange = FloatRange.ZeroToOne;
        public QualityRange QualityRange = QualityRange.All;

        [HarmonyPostfix]
        [UsedImplicitly]
        private static void Loadout_ExposeData_Postfix(Loadout __instance)
        {
            var ceLoadoutExtended = LoadoutExtended(__instance);
            Scribe_Values.Look(ref ceLoadoutExtended.HpRange, "hpRange", FloatRange.ZeroToOne);
            Scribe_Values.Look(ref ceLoadoutExtended.QualityRange, "qualityRange", QualityRange.All);
            DbgLog.Msg($"CE_LoadoutExtended ExposeData");
        }

        public static CE_LoadoutExtended LoadoutExtended(Loadout loadout)
        {
            if (!Loadouts.TryGetValue(loadout, out var result))
            {
                result = new CE_LoadoutExtended() {Loadout = loadout};
                Loadouts.Add(loadout, result);
            }
            DebugLog();
            return result;
        }

        public static void CopyLoadoutExtended(Loadout from, Loadout to)
        {
            var src = LoadoutExtended(from);
            var dest = LoadoutExtended(to);
            dest.HpRange = src.HpRange;
            dest.QualityRange = src.QualityRange;
        }

        public static void ClearData()
        {
            Loadouts.Clear();
            DbgLog.Wrn($"[CE_LoadoutExtended] Clear data");
        }

        [Conditional("DEBUG")]
        private static void DebugLog()
        {
            _ticks++;
            if (_ticks % 100 == 0)
            {
                Log.Warning($"[CE_LoadoutExtended:Loadouts] Count: {Loadouts.Count}");
            }
        }
    }
}