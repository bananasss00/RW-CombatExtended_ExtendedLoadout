using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BetterPawnControl;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public class BPC_AssignLink_Manager
    {
        public static Dictionary<AssignLink, List<int>> LoadoutIds = new Dictionary<AssignLink, List<int>>();
    }

    public static class BPC
    {
        public static bool Active => ModLister.GetActiveModWithIdentifier("VouLT.BetterPawnControl") != null;

        public static void Patch(Harmony h)
        {
            var saveCurrentState = AccessTools.Method(typeof(AssignManager), nameof(AssignManager.SaveCurrentState));
            var loadState = AccessTools.Method(typeof(AssignManager), nameof(AssignManager.LoadState));
            h.Patch(saveCurrentState, prefix: new HarmonyMethod(typeof(BPC), nameof(SaveCurrentState_Prefix)));
            //h.Patch(saveCurrentState, transpiler: new HarmonyMethod(typeof(BPC), nameof(BetterPawnControl_AssignManager_SaveCurrentState)));
            h.Patch(loadState, transpiler: new HarmonyMethod(typeof(BPC), nameof(BetterPawnControl_AssignManager_LoadState)));
        }

        public static void LoadLoadoutById(Pawn pawn, AssignLink assignLink)
        {
            if (BPC_AssignLink_Manager.LoadoutIds.TryGetValue(assignLink, out var ids))
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    var loadout = LoadoutManager.GetLoadoutById(ids[i]);
                    pawn.SetLoadout(loadout, i);
                }
            }
            Log.Warning($"LoadLoadoutById");
        }

        public static void SaveLoadoutId(AssignLink assignLink, Pawn pawn)
        {
            var loadoutMulti = pawn.GetLoadout() as Loadout_Multi;
            if (!BPC_AssignLink_Manager.LoadoutIds.ContainsKey(assignLink))
            {
                BPC_AssignLink_Manager.LoadoutIds.Add(assignLink, loadoutMulti.Loadouts.Select(x => x.uniqueID).ToList());
            }
            else
            {
                BPC_AssignLink_Manager.LoadoutIds[assignLink] = loadoutMulti.Loadouts.Select(x => x.uniqueID).ToList();
            }
            Log.Warning($"SaveLoadoutId");
        }

        static bool SaveCurrentState_Prefix(List<Pawn> pawns)
        {
			int currentMap = Find.CurrentMap.uniqueID;
            //Save current state
            foreach (Pawn p in pawns)
            {
                //find colonist on the current zone in the current map
                AssignLink link = AssignManager.links.Find(
                    x => p.Equals(x.colonist) &&
                    x.zone == AssignManager.GetActivePolicy().id &&
                    x.mapId == currentMap);

                if (link != null)
                {
                    //colonist found! save 
                    link.outfit = p.outfits.CurrentOutfit;
                    link.drugPolicy = p.drugs.CurrentPolicy;
                    link.hostilityResponse =
                        p.playerSettings.hostilityResponse;
                    link.foodPolicy = p.foodRestriction.CurrentFoodRestriction;
                    if (Widget_CombatExtended.CombatExtendedAvailable)
                    {
                        //link.loadoutId = Widget_CombatExtended.GetLoadoutId(p);
                        SaveLoadoutId(link, p);
                    }
                }
                else
                {
                    //colonist not found. So add it to the AssignLink list
                    int loadoutId = 0;
                    //if (Widget_CombatExtended.CombatExtendedAvailable)
                    //{
                    //    loadoutId = Widget_CombatExtended.GetLoadoutId(p);
                    //}

                    Outfit outfit = p.outfits.CurrentOutfit;
                    if (outfit ==
                        Current.Game.outfitDatabase.DefaultOutfit())
                    {
                        outfit = AssignManager.DefaultOutfit;
                    }

                    DrugPolicy drug = p.drugs.CurrentPolicy;
                    if (drug ==
                        Current.Game.drugPolicyDatabase.DefaultDrugPolicy())
                    {
                        drug = AssignManager.DefaultDrugPolicy;
                    }

                    FoodRestriction food = p.foodRestriction.CurrentFoodRestriction;
                    if (food ==
                        Current.Game.foodRestrictionDatabase.DefaultFoodRestriction())
                    {
                        food = AssignManager.DefaultFoodPolicy;
                    }

                    link = new AssignLink(
                        AssignManager.GetActivePolicy().id,
                        p,
                        outfit,
                        food,
                        drug,
                        p.playerSettings.hostilityResponse,
                        loadoutId,
                        currentMap);
                    SaveLoadoutId(link, p);
                    AssignManager.links.Add(link);
                }
            }
            return false;
        }

        //public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_SaveCurrentState_DBG(IEnumerable<CodeInstruction> instructions)
        //{
        //    var after = BetterPawnControl_AssignManager_SaveCurrentState(instructions);
        //    File.WriteAllLines("a:\\before_save.txt", instructions.Select(x => x.ToString()));
        //    File.WriteAllLines("a:\\after_save.txt", after.Select(x => x.ToString()));
        //    return after;
        //}

        //public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_LoadState_DBG(IEnumerable<CodeInstruction> instructions)
        //{
        //    var after = BetterPawnControl_AssignManager_LoadState(instructions);
        //    File.WriteAllLines("a:\\before_load.txt", instructions.Select(x => x.ToString()));
        //    File.WriteAllLines("a:\\after_load.txt", after.Select(x => x.ToString()));
        //    return after;
        //}

        public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_LoadState(IEnumerable<CodeInstruction> instructions)
        {
            /*
            82	010B	brfalse.s	87 (011A) ldloca.s V_4 (4)
            83	010D	ldloc.3
            84	010E	ldloc.s	V_5 (5)
            85	0110	ldfld	int32 BetterPawnControl.AssignLink::loadoutId
            86	0115	call	void BetterPawnControl.Widget_CombatExtended::SetLoadoutById(class ['Assembly-CSharp']Verse.Pawn, int32)
             */
            var code = instructions.ToList();
            var setLoadoutById = AccessTools.Method($"BetterPawnControl.Widget_CombatExtended:SetLoadoutById");
            int fixedSetLoadoutId = 0;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Brfalse_S)
                {
                    if (code[i + 1].opcode == OpCodes.Ldloc_3 && (code[i + 4].opcode == OpCodes.Call && code[i + 4].operand == setLoadoutById))
                    {
                        fixedSetLoadoutId++;
                        yield return code[i++]; // 82	010B	brfalse.s	87 (011A) ldloca.s V_4 (4)
                        yield return code[i++]; // 83	010D	ldloc.3
                        yield return code[i++]; // 84	010E	ldloc.s	V_5 (5)
                        i++; // skip: 85	0110	ldfld	int32 BetterPawnControl.AssignLink::loadoutId
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPC), nameof(LoadLoadoutById)));
                        continue;
                    }
                }
                yield return code[i];
            }

            if (fixedSetLoadoutId != 1)
            {
                Log.Error($"Transpiler outdated!");
            }
        }

        //public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_SaveCurrentState(IEnumerable<CodeInstruction> instructions)
        //{
        //    /*
        //    51	00AE	call	bool BetterPawnControl.Widget_CombatExtended::get_CombatExtendedAvailable()

        //    52	00B3	brfalse	121 (01AB) ldloca.s V_1 (1)
        //    53	00B8	ldloc.3
        //    54	00B9	ldloc.2
        //    55	00BA	ldfld	class ['Assembly-CSharp']Verse.Pawn BetterPawnControl.AssignManager/'<>c__DisplayClass21_1'::p
        //    56	00BF	call	int32 BetterPawnControl.Widget_CombatExtended::GetLoadoutId(class ['Assembly-CSharp']Verse.Pawn)
        //    57	00C4	stfld	int32 BetterPawnControl.AssignLink::loadoutId
        //    58	00C9	br	121 (01AB) ldloca.s V_1 (1)
        //    59	00CE	ldc.i4.0
        //    60	00CF	stloc.s	V_4 (4)
        //    61	00D1	call	bool BetterPawnControl.Widget_CombatExtended::get_CombatExtendedAvailable()

        //    62	00D6	brfalse.s	67 (00E5) ldloc.2 
        //    63	00D8	ldloc.2
        //    64	00D9	ldfld	class ['Assembly-CSharp']Verse.Pawn BetterPawnControl.AssignManager/'<>c__DisplayClass21_1'::p
        //    65	00DE	call	int32 BetterPawnControl.Widget_CombatExtended::GetLoadoutId(class ['Assembly-CSharp']Verse.Pawn)
        //    66	00E3	stloc.s	V_4 (4)

        //     */
        //    var code = instructions.ToList();
        //    var getLoadoutId = AccessTools.Method($"BetterPawnControl.Widget_CombatExtended:GetLoadoutId");
        //    int fixedGetLoadoutId = 0;
        //    for (int i = 0; i < code.Count; i++)
        //    {
        //        if (code[i].opcode == OpCodes.Brfalse || code[i].opcode == OpCodes.Brfalse_S)
        //        {
        //            if (code[i + 1].opcode == OpCodes.Ldloc_3 && (code[i + 4].opcode == OpCodes.Call && code[i + 4].operand == getLoadoutId))
        //            {
        //                fixedGetLoadoutId++;
        //                yield return code[i++]; // 52	00B3	brfalse
        //                yield return code[i++]; // 53	00B8	ldloc.3 => AssignLink
        //                yield return code[i++]; // 54	00B9	ldloc.2
        //                yield return code[i++]; // 55	00BA	ldfld => Pawn
        //                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPC), nameof(SaveLoadoutId)));
        //                i++; // skip: stfld	int32 BetterPawnControl.AssignLink::loadoutId
        //                continue;
        //            }
        //            else if (code[i + 1].opcode == OpCodes.Ldloc_2 && (code[i + 3].opcode == OpCodes.Call && code[i + 3].operand == getLoadoutId))
        //            {
        //                fixedGetLoadoutId++;
        //                yield return code[i++]; // 62	00D6	brfalse.s
        //                yield return new CodeInstruction(OpCodes.Ldloc_3); // => AssignLink
        //                yield return code[i++]; // 63	00D8	ldloc.2
        //                yield return code[i++]; // 64	00D9	ldfld => Pawn
        //                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPC), nameof(SaveLoadoutId))); // CALL WHEN AssignLink = NULL
        //                i++; // skip: 66	00E3	stloc.s	V_4 (4)
        //                continue;
        //            }
        //        }
        //        yield return code[i];
        //    }

        //    if (fixedGetLoadoutId != 2)
        //    {
        //        Log.Error($"Transpiler outdated!");
        //    }
        //}
    }
}