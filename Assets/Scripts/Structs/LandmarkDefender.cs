using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LandmarkDefender  {
    public string className;
    public int armyCount;
    public bool includeInFirstWeight; //should this element be included when generating the first defender?
}
