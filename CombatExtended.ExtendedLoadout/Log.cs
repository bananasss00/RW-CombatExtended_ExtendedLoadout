using System.Diagnostics;

namespace CombatExtended.ExtendedLoadout
{
    public class DbgLog
    {
        [Conditional("DEBUG")]
        public static void Msg(string message) => Verse.Log.Message(message);

        [Conditional("DEBUG")]
        public static void Err(string message) => Verse.Log.Error(message);

        [Conditional("DEBUG")]
        public static void Wrn(string message) => Verse.Log.Warning(message);
    }
}