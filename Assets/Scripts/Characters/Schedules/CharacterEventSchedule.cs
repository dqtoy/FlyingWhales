using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEventSchedule {

    private readonly Dictionary<string, CharacterAction> eventSchedule;

    public CharacterAction this[GameDate key] {
        get {
            return eventSchedule[key.GetDayAndTicksString()];
        }
    }

    public bool ContainsKey(GameDate key) {
        return eventSchedule.ContainsKey(key.GetDayAndTicksString());
    }
    public void AddElement(GameDate key, CharacterAction value) {
        eventSchedule.Add(key.GetDayAndTicksString(), value);
    }
    public bool HasScheduledAction(GameDate date) {
        return ContainsKey(date);
    }
    
}
