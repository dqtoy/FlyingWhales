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
    public void AddElement(DateRange key, GameEvent value) {
        eventSchedule.Add(key, value);
    }
    public bool HasConflictingSchedule(DateRange newRange) {
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {
            if (kvp.Key.HasConflictWith(newRange)) {
                return true;
            }
        }
        return false;
    }
    private GameEvent GetEventForDate(GameDate date) {
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {
            DateRange currRange = kvp.Key;
            GameEvent currEvent = kvp.Value;
            if (currRange.IsDateInRange(date)) {
                return currEvent;
            }
        }
        return null;
    }

    public GameDate GetNextFreeDate() {
        return new GameDate();
    }
    
}
