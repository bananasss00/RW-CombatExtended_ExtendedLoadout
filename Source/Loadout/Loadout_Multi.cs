using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public class Loadout_Multi : Loadout, IExposable, ILoadReferenceable
    {
        public new int uniqueID;
        
        public Loadout Loadout1 = LoadoutManager.DefaultLoadout;
        public Loadout Loadout2 = LoadoutManager.DefaultLoadout;

        public new int SlotCount => Loadout1._slots.Concat(Loadout2._slots).Count();
        public new List<LoadoutSlot> Slots => Loadout1._slots.Concat(Loadout2._slots).ToList();

        public Loadout_Multi()
        {
            uniqueID = LoadoutMulti_Manager.GetUniqueLoadoutID();
        }

        public new string GetUniqueLoadID()
        {
            return "LoadoutMulti_" + uniqueID;
        }

        public new void ExposeData()
        {
            Scribe_Values.Look(ref uniqueID, "uniqueID");
            Scribe_References.Look(ref Loadout1, "Loadout1");
            Scribe_References.Look(ref Loadout2, "Loadout2");
        }
    }
}