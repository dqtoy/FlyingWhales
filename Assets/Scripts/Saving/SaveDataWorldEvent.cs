using System.Collections;
using System.Collections.Generic;
using Events.World_Events;
using UnityEngine;

[System.Serializable]
public class SaveDataWorldEvent {
    public int currentMonth;
    public int currentDay;
    public int currentYear;
    public int currentTick;

    public int endMonth;
    public int endDay;
    public int endYear;
    public int endTick;

    public WORLD_EVENT worldEvent;

    public void Save(WorldEvent worldEvent) {

    }
}
