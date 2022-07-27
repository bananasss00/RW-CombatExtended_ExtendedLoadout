using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Serialization;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[Serializable]
public class LoadoutConfig
{
    public string label;
    public LoadoutSlotConfig[] slots;
}

[Serializable]
public class LoadoutSlotConfig
{
    public bool isGenericDef;
    public string defName;
    public int count;
}

[Serializable]
public class LoadoutConfigs
{
    public LoadoutConfig[] configs;
}

public static class LoadUtil
{
    public static LoadoutSlotConfig ToConfig(this LoadoutSlot loadoutSlot)
    {
        return new LoadoutSlotConfig
        {
            isGenericDef = loadoutSlot._type == typeof(LoadoutGenericDef),
            defName = loadoutSlot._def.defName,
            count = loadoutSlot._count
        };
    }

    public static LoadoutConfigs ToConfig(this IEnumerable<Loadout> loadouts)
    {
        return new LoadoutConfigs { configs = loadouts.Select(x => x.ToConfig()).ToArray() };
    }

    public static Loadout[] FromConfig(LoadoutConfigs loadoutConfig, out List<string> unloadableDefNames)
    {
        var result = new List<Loadout>();
        unloadableDefNames = new List<string>();
        foreach (var cfg in loadoutConfig.configs)
        {
            result.Add(FromConfig(cfg, out List<string> defs));
            unloadableDefNames.AddRange(defs);
        }
        return result.ToArray();
    }

    public static LoadoutConfig ToConfig(this Loadout loadout)
    {
        List<LoadoutSlotConfig> loadoutSlotConfigList = new List<LoadoutSlotConfig>();

        foreach (LoadoutSlot loadoutSlot in loadout._slots)
        {
            loadoutSlotConfigList.Add(loadoutSlot.ToConfig());
        }
        
        return new LoadoutConfig
        {
            label = loadout.label,
            slots = loadoutSlotConfigList.ToArray()
        };
    }

    static bool IsUniqueLoadoutLabel(this string label)
    {
        LoadoutManager manager = Current.Game.GetComponent<LoadoutManager>();
        // For consistency with the 'GetUniqueLabel' behavior
        if (manager == null)
        {
            return false;
        }
        return !manager._loadouts.Any(l => l.label == label);
    }

    public static LoadoutSlot FromConfig(LoadoutSlotConfig loadoutSlotConfig)
    {
        if (loadoutSlotConfig.isGenericDef)
        {
            LoadoutGenericDef genericThingDef = DefDatabase<LoadoutGenericDef>.GetNamed(loadoutSlotConfig.defName, false);
            return genericThingDef == null ? null : new LoadoutSlot(genericThingDef, loadoutSlotConfig.count);
        }

        var thingDef = DefDatabase<ThingDef>.GetNamed(loadoutSlotConfig.defName, false);
        return thingDef == null ? null : new LoadoutSlot(thingDef, loadoutSlotConfig.count);
    }

    public static Loadout FromConfig(LoadoutConfig loadoutConfig, out List<string> unloadableDefNames)
    {
        // Create the new loadout, preventing name clashes if the loadout already exists
        string uniqueLabel = loadoutConfig.label.IsUniqueLoadoutLabel()
            ? loadoutConfig.label
            : LoadoutManager.GetUniqueLabel(loadoutConfig.label);
        
        Loadout loadout = new Loadout(uniqueLabel);
        
        unloadableDefNames = new List<string>();
        
        // Now create each of the slots
        foreach (LoadoutSlotConfig loadoutSlotConfig in loadoutConfig.slots)
        {
            LoadoutSlot loadoutSlot = FromConfig(loadoutSlotConfig);
            // If the LoadoutSlot could not be loaded then continue loading the others as this most likely means
            // that the current game does not have the mod loaded that was used to create the initial loadout.
            if (loadoutSlot == null)
            {
                unloadableDefNames.Add(loadoutSlotConfig.defName);
                continue;
            }
            loadout.AddSlot(FromConfig(loadoutSlotConfig));		        
        }
        
        return loadout;
    }
}

[HarmonyPatch(typeof(Dialog_ManageLoadouts))]
[HotSwappable]
public static class Dialog_ManageLoadouts_SaveLoad
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

    [HarmonyPatch(nameof(Dialog_ManageLoadouts.DoWindowContents))]
    [HarmonyPostfix]
    public static void Postfix(Dialog_ManageLoadouts __instance, Rect canvas)
    {
        // save, load, loadall
        var saveRect = canvas.RightPartPixels((Dialog_ManageLoadouts._topAreaHeight + Dialog_ManageLoadouts._margin) * 3);
        saveRect.height = saveRect.width = Dialog_ManageLoadouts._topAreaHeight;
        var loadRect = new Rect(saveRect.xMax + Dialog_ManageLoadouts._margin, 0f, Dialog_ManageLoadouts._topAreaHeight, Dialog_ManageLoadouts._topAreaHeight);
        var loadAllRect = new Rect(loadRect.xMax + Dialog_ManageLoadouts._margin, 0f, Dialog_ManageLoadouts._topAreaHeight, Dialog_ManageLoadouts._topAreaHeight);

        if (Widgets.ButtonImage(saveRect, Textures.LoadoutSave))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoadoutConfigs));
            using TextWriter writer = new StreamWriter(new FileInfo("loadouts.xml").FullName);
            xmlSerializer.Serialize(writer, LoadoutManager.Loadouts.Where(x => !x.defaultLoadout).ToConfig());
        }
        if (Widgets.ButtonImage(loadRect, Textures.LoadoutLoad))
        {
            var mySerializer = new XmlSerializer(typeof(LoadoutConfigs));
            using var myFileStream = new FileStream(new FileInfo("loadouts.xml").FullName, FileMode.Open);
            LoadoutConfigs loadoutConfigs = (LoadoutConfigs)mySerializer.Deserialize(myFileStream);
            var loadouts = LoadUtil.FromConfig(loadoutConfigs, out List<string> unloadableDefNames);
            // Report any LoadoutSlots (i.e. ThingDefs) that could not be loaded.
            if (unloadableDefNames.Count > 0)
            {
                Messages.Message(
                    "CE_MissingLoadoutSlots".Translate(String.Join(", ", unloadableDefNames)), 
                    null, MessageTypeDefOf.RejectInput);
            }
            foreach (var loudout in loadouts)
            {
                LoadoutManager.AddLoadout(loudout);
            }
        }
        if (Widgets.ButtonImage(loadAllRect, Textures.LoadoutLoadAll))
        {
            Log.Error("LoadoutLoadAll");
        }
    }
}
