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
        public new int SlotCount => Slots?.Count ?? 0;
        public new List<LoadoutSlot> Slots { get; private set; }
        private Loadout _loadout1, _loadout2;

        public Loadout Loadout1
        {
            get => _loadout1;
            set
            {
                _loadout1 = value;
                // concat if another loadouts not null
                Slots = _loadout2 == null ? _loadout1.Slots : _loadout1.Slots.Concat(_loadout2.Slots).ToList();
            }
        }

        public Loadout Loadout2
        {
            get => _loadout2;
            set
            {
                _loadout2 = value;
                // concat if another loadouts not null
                Slots = _loadout1 == null ? _loadout2.Slots : _loadout1.Slots.Concat(_loadout2.Slots).ToList();
            }
        }

        public Loadout_Multi()
        {
            Loadout1 = LoadoutManager.DefaultLoadout;
            Loadout2 = LoadoutManager.DefaultLoadout;
            uniqueID = LoadoutMulti_Manager.GetUniqueLoadoutID();
        }

        public new string GetUniqueLoadID()
        {
            return "LoadoutMulti_" + uniqueID;
        }

        public new void ExposeData()
        {
            Scribe_Values.Look(ref uniqueID, "uniqueID");
            Scribe_References.Look(ref _loadout1, "Loadout1");
            Scribe_References.Look(ref _loadout2, "Loadout2");
        }

        /// <summary>
        /// Find loadout with ThingDef in Slots
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Loadout FindLoadoutWithThingDef(ThingDef t)
        {
            foreach (var slot in Loadout1.Slots)
                if (slot.thingDef == t) return Loadout1;
            foreach (var slot in Loadout2.Slots)
                if (slot.thingDef == t) return Loadout2;
            return null;
        }
    }
}