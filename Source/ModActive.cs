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
                if (_betterPawnControl == null)
                    _betterPawnControl = ModLister.GetActiveModWithIdentifier("VouLT.BetterPawnControl") != null;
                return (bool)_betterPawnControl;
            }
        }
    }
}