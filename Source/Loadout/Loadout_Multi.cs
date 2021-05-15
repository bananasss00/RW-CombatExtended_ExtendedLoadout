using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public class Loadout_Multi : Loadout, IExposable, ILoadReferenceable
    {
        // Init only
        private static int? _columnsCount;
        public static int ColumnsCount
        {
            get => _columnsCount ?? throw new Exception("ColumnsCount not initialized!");
            set
            {
                if (_columnsCount != null)
                    throw new Exception("ColumnsCount can be setted one time!");
                _columnsCount = value;
            }
        }

        public new int uniqueID;
        public new int SlotCount => Slots.Count;
        public new List<LoadoutSlot> Slots { get; private set; } = new();
        private List<Loadout> _loadouts;

        public Loadout_Multi()
        {
            _loadouts = Enumerable.Repeat(LoadoutManager.DefaultLoadout, ColumnsCount).ToList();
            NotifyLoadoutChanged();
            uniqueID = LoadoutMulti_Manager.GetUniqueLoadoutID();
        }

        public List<Loadout> Loadouts => _loadouts;

        public Loadout this[int index]
        {
            get => _loadouts[index];
            set
            {
                _loadouts[index] = value;
                NotifyLoadoutChanged();
            }
        }

        /// <summary>
        /// Update Slots cache
        /// </summary>
        public void NotifyLoadoutChanged()
        {
            Slots = _loadouts.Where(x => x != null).SelectMany(x => x.Slots).ToList();
            DbgLog.Msg("Loadout_Multi.NotifyLoadoutChanged");
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
                // fix changed columns count
                int sizeDelta = ColumnsCount - _loadouts.Count;
                if (sizeDelta > 0)
                {
                    Log.Warning($"[Loadout_Multi] Fix loadouts list. Count difference: {sizeDelta}");
                    for (int i = 0; i < sizeDelta; i++)
                    {
                        _loadouts.Add(LoadoutManager.DefaultLoadout);
                    }
                }
                else if (sizeDelta < 0)
                {
                    Log.Warning($"[Loadout_Multi] Fix loadouts list. Count difference: {sizeDelta}");
                    for (int i = 0; i < Math.Abs(sizeDelta); i++)
                    {
                        _loadouts.RemoveAt(_loadouts.Count - 1);
                    }
                }
                // fix removed loadouts
                for (int i = 0; i < _loadouts.Count; i++)
                {
                    if (_loadouts[i] == null)
                    {
                        Log.Warning($"[Loadout_Multi] Fix removed loadout id: {uniqueID}");
                        _loadouts[i] = LoadoutManager.DefaultLoadout;
                    }
                }
                NotifyLoadoutChanged();
            }
        }

        /// <summary>
        /// Find loadout with ThingDef in Slots
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Loadout? FindLoadoutWithThingDef(ThingDef t)
        {
            foreach (var loadout in _loadouts)
            {
                foreach (var slot in loadout.Slots)
                {
                    if (slot.thingDef == t)
                    {
                        return loadout;
                    }
                }
            }
            return null;
        }
    }
}