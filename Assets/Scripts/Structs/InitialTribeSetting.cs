using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct InitialTribeSetting {
    public int largeTribes;
    public int mediumTribes;
    public int smallTribes;

    public Dictionary<FACTION_SIZE, int> GetDictionary() {
        Dictionary<FACTION_SIZE, int> dict = new Dictionary<FACTION_SIZE, int>();
        dict.Add(FACTION_SIZE.LARGE, largeTribes);
        dict.Add(FACTION_SIZE.MEDIUM, mediumTribes);
        dict.Add(FACTION_SIZE.SMALL, smallTribes);
        return dict;
    }
}
