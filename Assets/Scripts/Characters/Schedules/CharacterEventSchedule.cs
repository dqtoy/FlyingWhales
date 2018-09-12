using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEventSchedule {

    private readonly Dictionary<string, EventAction> eventSchedule;

    public EventAction this[GameDate key] {
        get {
            return eventSchedule[key.GetDayAndTicksString()];
        }
    }

    public CharacterEventSchedule() {
        eventSchedule = new Dictionary<string, EventAction>();
    }

    public bool ContainsKey(GameDate key) {
        return eventSchedule.ContainsKey(key.GetDayAndTicksString());
    }
    public void AddElement(GameDate key, EventAction value) {
        eventSchedule.Add(key.GetDayAndTicksString(), value);
    }
    public bool HasScheduledAction(GameDate date) {
        return ContainsKey(date);
    }
    
}
