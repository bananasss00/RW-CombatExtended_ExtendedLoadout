﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    /// <summary>
    /// Replaced pawn.GetLoadout() => pawn.GetLoadout().Loadout2, and SetLoadout2
    /// </summary>
    public class PawnColumnWorker_Loadout2 : PawnColumnWorker_Loadout
    {
        private IEnumerable<Widgets.DropdownMenuElement<Loadout>> Button_GenerateMenu2(Pawn pawn)
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
                            pawn.SetLoadout2(loadout);
                        }),
                        payload = loadout
                    };
                }
            }
        }

        public override void DoHeader(Rect rect, PawnTable table)
        {
            // dont call PawnColumnWorker_Loadout.DoHeader, because he draw button ManageLoadouts
            //base.DoHeader(rect, table);

            // instead draw vanilla base code
            if (!def.label.NullOrEmpty())
            {
                Text.Font = DefaultHeaderFont;
                GUI.color = DefaultHeaderColor;
                Text.Anchor = TextAnchor.LowerCenter;
                var rect2 = rect;
                rect2.y += 3f;
                Widgets.Label(rect2, def.LabelCap.Resolve().Truncate(rect.width));
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
            }
            else if (def.HeaderIcon != null)
            {
                var headerIconSize = def.HeaderIconSize;
                var num = (int) ((rect.width - headerIconSize.x) / 2f);
                GUI.DrawTexture(new Rect(rect.x + num, rect.yMax - headerIconSize.y, headerIconSize.x, headerIconSize.y).ContractedBy(2f), def.HeaderIcon);
            }

            if (table.SortingBy == def)
            {
                var texture2D = table.SortingDescending ? SortingDescendingIcon : SortingIcon;
                GUI.DrawTexture(new Rect(rect.xMax - texture2D.width - 1f, rect.yMax - texture2D.height - 1f, texture2D.width, texture2D.height), texture2D);
            }

            if (def.HeaderInteractable)
            {
                var interactableHeaderRect = GetInteractableHeaderRect(rect, table);
                if (Mouse.IsOver(interactableHeaderRect))
                {
                    Widgets.DrawHighlight(interactableHeaderRect);
                    var headerTip = GetHeaderTip(table);
                    if (!headerTip.NullOrEmpty()) TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
                }

                if (Widgets.ButtonInvisible(interactableHeaderRect)) HeaderClicked(rect, table);
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
            string label = (pawn.GetLoadout() as Loadout_Multi).Loadout2.label.Truncate(loadoutButtonRect.width, null);
            Widgets.Dropdown<Pawn, Loadout>(loadoutButtonRect, pawn, p => (p.GetLoadout() as Loadout_Multi).Loadout2, Button_GenerateMenu2, label, null, null, null, null, true);

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
                Find.WindowStack.Add(new Dialog_ManageLoadouts((pawn.GetLoadout() as Loadout_Multi).Loadout2));
            }
            // Added this next line.
            TooltipHandler.TipRegion(assignTabRect, new TipSignal(textGetter("CE_Loadouts"), pawn.GetHashCode() * 613));
            num3 += (float)num2;
        }
    }
}