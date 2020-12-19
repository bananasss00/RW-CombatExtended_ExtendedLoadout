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
            var h = new Harmony("PirateBY.CombatExtended.ExtendedLoadout");
            h.PatchAll();
            if (BPC.Active)
                BPC.Patch(h);
            Log.Message("[CombatExtended.ExtendedLoadout] Initialized");
        }
    }
}
