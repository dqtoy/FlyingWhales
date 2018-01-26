using UnityEngine;
using System.Collections;

[System.Serializable]
public struct QuestTypeSetup {
    public QUEST_TYPE questType;
    public bool isHarmful;
    public bool canBeAcceptedOutsideFaction;
    public int declareWarWeight;
}
