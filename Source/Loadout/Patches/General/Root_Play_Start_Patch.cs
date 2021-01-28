using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
    public class Root_Play_Start_Patch
    {
        static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts || ExtendedLoadoutMod.Instance.useHpAndQualityInLoadouts;

        [HarmonyPrefix]
        [UsedImplicitly]
        public static void OnNewGame()
        {
            CE_LoadoutExtended.ClearData();
            LoadoutMulti_Manager.ClearData();
            if (ModActive.BetterPawnControl)
                BPC_AssignLink_Manager.ClearData();
            DbgLog.Msg($"Data cleaned on MapComponentsInitializing");
        }
    }
}