using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DefenderSetting  {
    public string className;
    public bool includeInFirstWeight; //should this element be included when generating the first defender?
}

[System.Serializable]
public class AreaDefenderSetting {
    public string className;
}

[System.Serializable]
public class RaceAreaDefenderSetting {
    public string className;
    public int weight;
}
