using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructurePriority {

    public StructurePrioritySetting setting;

    public StructurePriority(StructurePrioritySetting settings) : this() {
        this.setting = settings;
    }
    public StructurePriority() {
        this.setting = new StructurePrioritySetting();
    }

    //public void AddSettings(StructurePrioritySetting setting) {
    //    if (!this.setting.Contains(setting)) {
    //        this.setting.Add(setting);
    //    }
    //}
    //public void RemoveSettings(StructurePrioritySetting setting) {
    //    this.setting.Remove(setting);
    //}
}
