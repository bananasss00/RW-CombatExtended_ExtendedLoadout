using System;
using System.Collections;
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
        public static int ColumnsCount { get; set; }

        public new int uniqueID;
        public new int SlotCount => Slots?.Count ?? 0;
        public new List<LoadoutSlot> Slots { get; private set; }
        private List<Loadout> _loadouts;

        public Loadout_Multi()
        {
            _loadouts = Enumerable.Repeat(LoadoutManager.DefaultLoadout, ColumnsCount).ToList();
            uniqueID = LoadoutMulti_Manager.GetUniqueLoadoutID();
        }

        public List<Loadout> Loadouts => _loadouts;
        
        public Loadout this[int index]
        {
            get => _loadouts[index];
            set
            {
                _loadouts[index] = value;
                Slots = _loadouts.Where(x => x != null).SelectMany(x => x.Slots).ToList();
            }
        }

        public new string GetUniqueLoadID()
        {
            return "LoadoutMulti_" + uniqueID;
        }

        public new void ExposeData()
        {
            Scribe_Values.Look(ref uniqueID, "uniqueID");
            Scribe_Collections.Look(ref _loadouts, "loadouts", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                int sizeDelta = ColumnsCount - _loadouts.Count;
                if (sizeDelta > 0)
                {
                    Log.Warning($"[Loadout_Multi] Fix loadouts list. Count difference: {sizeDelta}");
                    for (int i = 0; i < sizeDelta; i++) _loadouts.Add(LoadoutManager.DefaultLoadout);
                }
                else if (sizeDelta < 0)
                {
                    Log.Warning($"[Loadout_Multi] Fix loadouts list. Count difference: {sizeDelta}");
                    for (int i = 0; i < Math.Abs(sizeDelta); i++) _loadouts.RemoveAt(_loadouts.Count - 1);
                }
            }
        }

        /// <summary>
        /// Find loadout with ThingDef in Slots
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Loadout FindLoadoutWithThingDef(ThingDef t)
        {
            foreach (var loadout in _loadouts)
            {
                foreach (var slot in loadout.Slots)
                    if (slot.thingDef == t) return loadout;
            }
            return null;
        }
    }
}