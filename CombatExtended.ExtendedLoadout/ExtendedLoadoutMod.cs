using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [StaticConstructorOnStartup]
    [UsedImplicitly]
    public class ExtendedLoadoutMod
    {
        static ExtendedLoadoutMod()
        {
            new Harmony("PirateBY.CombatExtended.ExtendedLoadout").PatchAll();
            Log.Message("[CombatExtended.ExtendedLoadout] Initialized");
        }
    }
}
