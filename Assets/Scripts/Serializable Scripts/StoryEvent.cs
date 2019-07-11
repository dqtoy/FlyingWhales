using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryEvent {
    public string name;
    public string text;
    public StoryEventChoice[] choices;
}
[System.Serializable]
public class StoryEventChoice {
    public string text;
    public string reqType;
    public string reqName;
}
[System.Serializable]
public class StoryEventEffect {
    public string effectType;
    public string effectName;
    public int effectValue;
}
