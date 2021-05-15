
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClearDataOnNewGame : Attribute {}

    public class Cleaner_GameComponent : GameComponent
	{
        private static List<MethodInfo>? _clearDataMethods;

        public Cleaner_GameComponent(Game game)
		{
            if (_clearDataMethods == null)
            {
                var allTypesInAsm = Assembly.GetExecutingAssembly().GetTypes();
                _clearDataMethods = allTypesInAsm
                    .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .Where(x => x.TryGetAttribute<ClearDataOnNewGame>(out _))
                    .ToList();
                DbgLog.Wrn($"ClearDataMethods: {String.Join("; ", _clearDataMethods.Select(x => $"{x.DeclaringType.Name}:{x.Name}"))}");
            }
            DbgLog.Wrn($"GameComponent_ExtendedLoadout Constructor: {Scribe.mode}");
            _clearDataMethods?.ForEach(x => x.Invoke(null, null));
		}

        public override void StartedNewGame()
        {
            DbgLog.Wrn($"StartedNewGame: {Scribe.mode}");
        }

        public override void LoadedGame()
        {
            DbgLog.Wrn($"LoadedGame: {Scribe.mode}");
        }
    }
}