using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
    public class Root_Play_Start_Patch
    {
        [HarmonyPrefix]
        [UsedImplicitly]
        public static void OnNewGame()
        {
            CE_LoadoutExtended.ClearData();
        }
    }
}