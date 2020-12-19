using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    /// <summary>
    /// Replaced pawn.GetLoadout() => pawn.GetLoadout().Loadout1, and SetLoadout1
    /// </summary>
    public class PawnColumnWorker_Loadout1 : PawnColumnWorker_Loadout
    {
        private IEnumerable<Widgets.DropdownMenuElement<Loadout>> Button_GenerateMenu1(Pawn pawn)
        {
            using (List<Loadout>.Enumerator enu = LoadoutManager.Loadouts.GetEnumerator())
            {
                while (enu.MoveNext())
                {
                    Loadout loadout = enu.Current;
                    yield return new Widgets.DropdownMenuElement<Loadout>
                    {
                        option = new FloatMenuOption(loadout.LabelCap, delegate ()
                        {
                            pawn.SetLoadout1(loadout);
                        }),
                        payload = loadout
                    };
                }
            }
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.outfits == null)
            {
                return;
            }
            //changed: int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
            int num = Mathf.FloorToInt((rect.width - 4f) - IconSize);
            //changed: int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
            int num2 = Mathf.FloorToInt(IconSize);
            float num3 = rect.x;
            //added:
            float num4 = rect.y + ((rect.height - IconSize) / 2);

            // Reduce width if we're adding a clear forced button
            bool somethingIsForced = pawn.HoldTrackerAnythingHeld();
            Rect loadoutButtonRect = new Rect(num3, rect.y + 2f, (float)num, rect.height - 4f);
            if (somethingIsForced)
            {
                loadoutButtonRect.width -= 4f + (float)num2;
            }

            // Main loadout button
            string label = (pawn.GetLoadout() as Loadout_Multi).Loadout1.label.Truncate(loadoutButtonRect.width, null);
            Widgets.Dropdown<Pawn, Loadout>(loadoutButtonRect, pawn, p => (p.GetLoadout() as Loadout_Multi).Loadout1, Button_GenerateMenu1, label, null, null, null, null, true);

            // Clear forced button
            num3 += loadoutButtonRect.width;
            num3 += 4f;
            //changed: Rect forcedHoldRect = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
            Rect forcedHoldRect = new Rect(num3, num4, (float)num2, (float)num2);
            if (somethingIsForced)
            {
                if (Widgets.ButtonImage(forcedHoldRect, ClearImage))
                {
                    pawn.HoldTrackerClear(); // yes this will also delete records that haven't been picked up and thus not shown to the player...
                }
                TooltipHandler.TipRegion(forcedHoldRect, new TipSignal(delegate
                {
                    string text = "CE_ForcedHold".Translate() + ":\n";
                    foreach (HoldRecord rec in LoadoutManager.GetHoldRecords(pawn))
                    {
                        if (!rec.pickedUp) continue;
                        text = text + "\n   " + rec.thingDef.LabelCap + " x" + rec.count;
                    }
                    return text;
                }, pawn.GetHashCode() * 613));
                num3 += (float)num2;
                num3 += 4f;
            }

            //changed: Rect assignTabRect = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
            Rect assignTabRect = new Rect(num3, num4, (float)num2, (float)num2);
            //changed: if (Widgets.ButtonText(assignTabRect, "AssignTabEdit".Translate(), true, false, true))
            if (Widgets.ButtonImage(assignTabRect, EditImage))
            {
                Find.WindowStack.Add(new Dialog_ManageLoadouts((pawn.GetLoadout() as Loadout_Multi).Loadout1));
            }
            // Added this next line.
            TooltipHandler.TipRegion(assignTabRect, new TipSignal(textGetter("CE_Loadouts"), pawn.GetHashCode() * 613));
            num3 += (float)num2;
        }
    }
}