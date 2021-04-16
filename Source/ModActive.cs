using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public class ModActive
    {
        private static bool? _betterPawnControl;

        public static bool BetterPawnControl
        {
            get
            {
                return _betterPawnControl ??= ModLister.GetActiveModWithIdentifier("VouLT.BetterPawnControl") != null;
            }
        }
    }
}