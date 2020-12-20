using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
    public class Root_Play_Start_Patch
    {
        static bool Prepare() => ConfigDefOf.Config.useMultiLoadouts;

        [HarmonyPrefix]
        [UsedImplicitly]
        public static void OnNewGame()
        {
            CE_LoadoutExtended.ClearData();
            LoadoutMulti_Manager.ClearData();
            if (BPC.Active)
                BPC_AssignLink_Manager.ClearData();
            DbgLog.Msg($"Data cleaned on MapComponentsInitializing");
        }
    }
}