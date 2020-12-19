using HarmonyLib;

namespace CombatExtended.ExtendedLoadout
{
    [HarmonyPatch(typeof(LoadoutManager))]
    public static class LoadoutManager_ExposeData_Patch
    {
        [HarmonyPatch(nameof(LoadoutManager.ExposeData))]
        [HarmonyPostfix]
        public static void ExposeData()
        {
            LoadoutMulti_Manager.ExposeData();
        }
    }
}