using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructurePriority {

    public List<StructurePrioritySetting> settings;

    public StructurePriority(List<StructurePrioritySetting> settings) : this() {
        this.settings = settings;
    }
    public StructurePriority() {
        this.settings = new List<StructurePrioritySetting>();
    }

    public void AddSettings(StructurePrioritySetting setting) {
        if (!settings.Contains(setting)) {
            settings.Add(setting);
        }
    }
    public void RemoveSettings(StructurePrioritySetting setting) {
        settings.Remove(setting);
    }
}
