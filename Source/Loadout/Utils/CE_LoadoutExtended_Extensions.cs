using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public static class CE_LoadoutExtended_Extensions
    {
        public static bool AllowEquip(this CE_LoadoutExtended ceLoadoutExtended, Thing t)
        {
            if (t.def.useHitPoints)
            {
                var hpRange = ceLoadoutExtended.HpRange;
                float hp = GenMath.RoundedHundredth((float)t.HitPoints / (float)t.MaxHitPoints);
                if (!hpRange.IncludesEpsilon(Mathf.Clamp01(hp)))
                {
                    DbgLog.Wrn($"{t.LabelCap} not allowed with hp: {hp}. range: {hpRange}");
                    return false;
                }
            }

            var qualityRange = ceLoadoutExtended.QualityRange;
            if (qualityRange != QualityRange.All && t.def.FollowQualityThingFilter())
            {
                if (!t.TryGetQuality(out var qc))
                    qc = QualityCategory.Normal;
                if (!qualityRange.Includes(qc))
                {
                    DbgLog.Wrn($"{t.LabelCap} not allowed with quality: {qc}. range: {qualityRange}");
                    return false;
                }
            }

            return true;
        }
    }
}