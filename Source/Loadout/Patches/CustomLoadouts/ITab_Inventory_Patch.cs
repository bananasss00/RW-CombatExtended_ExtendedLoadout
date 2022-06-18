using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.ExtendedLoadout;

/// <summary>
/// Fix 'Make loadout' button in pawn inventory
/// </summary>
[HarmonyPatch(typeof(ITab_Inventory))]
public static class ITab_Inventory_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ITab_Inventory.FillTab))]
    public static IEnumerable<CodeInstruction> FillTab_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
        if (Widgets.ButtonText(loadoutButtonRect, "CE_MakeLoadout".Translate())) {
            Loadout loadout = SelPawnForGear.GenerateLoadoutFromPawn();
            LoadoutManager.AddLoadout(loadout);
            SelPawnForGear.SetLoadout(loadout);
            Find.WindowStack.Add(new Dialog_ManageLoadouts(SelPawnForGear.GetLoadout()));
        }
        -1 ldloc.s	V_61 (61)
         0 call	void CombatExtended.Utility_Loadouts::SetLoadout(class ['Assembly-CSharp']Verse.Pawn, class CombatExtended.Loadout)
         1 call	class ['Assembly-CSharp']Verse.WindowStack ['Assembly-CSharp']Verse.Find::get_WindowStack()
         2 ldarg.0
         3 call CombatExtended.ITab_Inventory::get_SelPawnForGear()
         4 call  CombatExtended.Utility_Loadouts::GetLoadout(class ['Assembly-CSharp']Verse.Pawn)
         5 newobj	instance void CombatExtended.Dialog_ManageLoadouts::.ctor(class CombatExtended.Loadout)
         6 callvirt Verse.WindowStack::Add(class ['Assembly-CSharp']Verse.Window)
         */
        var code = instructions.ToList();
        var getLoadout = AccessTools.Method(typeof(Utility_Loadouts), nameof(Utility_Loadouts.GetLoadout));
        var setLoadout = AccessTools.Method(typeof(Utility_Loadouts), nameof(Utility_Loadouts.SetLoadout));
        var setLoadoutNew = AccessTools.Method(typeof(LoadoutMulti_Manager), nameof(LoadoutMulti_Manager.SetLoadout));

#pragma warning disable 252,253
        int setLoadoutReplaceIdx = code.FindIndex(c => c.opcode == OpCodes.Call && c.operand == setLoadout);
#pragma warning restore 252,253
        if (setLoadoutReplaceIdx == -1)
        {
            Log.Error($"Can't find SetLoadout in ITab_Inventory.FillTab");
            return instructions;
        }

        var loadoutLocal = code[setLoadoutReplaceIdx - 1].operand; // ldloc.s	V_61 (61)
        // add new arg index = 0
        code.Insert(setLoadoutReplaceIdx++, new CodeInstruction(OpCodes.Ldc_I4_0));
        // call LoadoutMulti_Manager.SetLoadout(pawn, loadout, index) instead Utility_Loadouts.SetLoadout(pawn, loadout)
        code[setLoadoutReplaceIdx].operand = setLoadoutNew;


        /*
            call void CombatExtended.Utility_Loadouts:SetLoadout
            nop  // new nop instruction - in 24.04.2021 build CombatExtended
            call Verse.Find::get_WindowStack()
            
            // replace SelPawnForGear.GetLoadout() => local_var loadout
            {
                ldarg.0
                call CombatExtended.ITab_Inventory::get_SelPawnForGear
                call CombatExtended.Utility_Loadouts::GetLoadout
            }
        */
        bool nextNop = code[setLoadoutReplaceIdx + 1].opcode == OpCodes.Nop;
        int replaceIndex = nextNop ? setLoadoutReplaceIdx + 3 : setLoadoutReplaceIdx + 2;
        var c = code.Skip(replaceIndex).Take(3).ToList();
        if (// ldarg.0
            c[0].opcode == OpCodes.Ldarg_0 &&
            // call CombatExtended.ITab_Inventory::get_SelPawnForGear()
            c[1].opcode == OpCodes.Call &&
            // call  CombatExtended.Utility_Loadouts::GetLoadout(class ['Assembly-CSharp']Verse.Pawn)
            c[2].Is(OpCodes.Call, getLoadout))
        {
            code.RemoveRange(replaceIndex, 3);
            code.Insert(replaceIndex, new CodeInstruction(OpCodes.Ldloc_S, loadoutLocal));
            // File.WriteAllLines("a:/before.txt", instructions.Select(x => x.ToString()));
            // File.WriteAllLines("a:/after.txt", code.Select(x => x.ToString()));
            return code;
        }
            
        Log.Error($"Outdated transpiler ITab_Inventory.FillTab");
        return instructions;
    }
}