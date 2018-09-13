using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEventSchedule {

    private readonly Dictionary<DateRange, GameEvent> eventSchedule;

    public GameEvent this[GameDate key] {
        get {
            return GetEventForDate(key);
        }
    }

    public CharacterEventSchedule() {
        eventSchedule = new Dictionary<DateRange, GameEvent>();
    }

    //public bool ContainsKey(GameDate key) {
    //    return eventSchedule.ContainsKey(key.GetDayAndTicksString());
    //}
    public void AddElement(DateRange key, GameEvent value) {
        eventSchedule.Add(key, value);
    }
    //public bool HasScheduledAction(GameDate date) {
    //    return ContainsKey(date);
    //}

    private GameEvent GetEventForDate(GameDate date) {
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {
            DateRange currRange = kvp.Key;
            GameEvent currEvent = kvp.Value;
            if (currRange.IsInRange(date)) {
                return currEvent;
            }
        }
        return null;
    }
    
}
