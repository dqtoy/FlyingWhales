using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterEventSchedule {

    private Dictionary<DateRange, GameEvent> eventSchedule;

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
        OrganizeSchedule();
    }
    /*
     This will organize the scedule from earliest to latest
         */
    private void OrganizeSchedule() {
        eventSchedule = eventSchedule.OrderBy(x => x.Key.GetStartDateValue()).ToDictionary(k => k.Key, k => k.Value);
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
    public string GetEventScheduleSummary() {
        string summary = string.Empty;
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {
            summary += "(" + kvp.Key.ToString() + ")" + kvp.Value.name + "\n";
        }
        return summary;
    }

    public GameEvent GetNextEvent() {
        if (eventSchedule.Count > 0) {
            return eventSchedule.ElementAt(0).Value;
        }
        return null;
    }
    
}
