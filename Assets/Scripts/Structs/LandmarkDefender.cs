using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DefenderSetting  {
    public string className;
    public bool includeInFirstWeight; //should this element be included when generating the first defender?
}

[System.Serializable]
public struct AreaCharacterClass {
    public string className;

    public AreaCharacterClass(string className) {
        this.className = className;
    }

    public override string ToString() {
        return className;
    }
}

[System.Serializable]
public class RaceAreaDefenderSetting {
    public string className;
    public int weight;
}
