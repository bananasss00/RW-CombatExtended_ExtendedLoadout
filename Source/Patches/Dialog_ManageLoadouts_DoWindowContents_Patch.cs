using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Dialog_ManageLoadouts), nameof(Dialog_ManageLoadouts.DoWindowContents))]
    public class Dialog_ManageLoadouts_DoWindowContents_Patch
    {
        [HarmonyTranspiler]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var drawSlotList = AccessTools.Method(typeof(Dialog_ManageLoadouts), nameof(Dialog_ManageLoadouts.DrawSlotList));

            bool heightFixed = false;
            bool drawHpQualityInjected = false;
            foreach (var ci in instructions)
            {
                if (!heightFixed && ci.opcode == OpCodes.Ldc_R4 && (float) ci.operand == 48f)
                {
                    // decrease slotListRect height
                    ci.operand = 120f; // canvas3..ctor(0f, canvas2.yMax + 6f, (canvas.width - 6f) / 2f, canvas.height - 30f - canvas2.height - 48f - 30f);
                    yield return ci;
                    heightFixed = true;
                }
                else if (heightFixed && !drawHpQualityInjected && ci.opcode == OpCodes.Call && ci.operand == drawSlotList)
                {
                    // draw after DrawSlotList(slotListRect);
                    yield return ci;
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 8); // local: bulkBarRect
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dialog_ManageLoadouts_DoWindowContents_Patch), nameof(DrawHpQuality)));
                    drawHpQualityInjected = true;
                }
                else yield return ci;
            }
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!drawHpQualityInjected || !heightFixed) Log.Error($"drawHpQualityInjected = {drawHpQualityInjected}; heightFixed = {heightFixed}");
        }

        public static void DrawHpQuality(Dialog_ManageLoadouts dialog, Rect bulkBarRect)
        {
            Rect hpRect = new Rect(bulkBarRect.xMin, bulkBarRect.yMax + Dialog_ManageLoadouts._margin, bulkBarRect.width, Dialog_ManageLoadouts._barHeight);
            Rect qualityRect = new Rect(hpRect.xMin, hpRect.yMax + Dialog_ManageLoadouts._margin, hpRect.width, Dialog_ManageLoadouts._barHeight);
            var ceLoadoutExtended = CE_LoadoutExtended.LoadoutExtended(dialog.CurrentLoadout);
            Widgets.FloatRange(hpRect, 976833332, ref ceLoadoutExtended.HpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
            Widgets.QualityRange(qualityRect, 976833333, ref ceLoadoutExtended.QualityRange);
        }
    }
}