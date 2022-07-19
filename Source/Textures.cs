

using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[StaticConstructorOnStartup]
public class Textures
{
    public static readonly Texture2D PersonalLoadout = ContentFinder<Texture2D>.Get("UI/personalLoadout");
    public static readonly Texture2D OptimizeApparel = ContentFinder<Texture2D>.Get("UI/optimizeApparel");
}

[StaticConstructorOnStartup]
public class Strings
{
    public static readonly string OptimizeApparelDesc = "Settings.OptimizeApparel.Desc".Translate();
}