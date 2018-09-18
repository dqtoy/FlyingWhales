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
    public void RemoveElement(GameEvent gameEvent) {
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {
            DateRange currRange = kvp.Key;
            GameEvent currEvent = kvp.Value;
            if (currEvent.id == gameEvent.id) {
                eventSchedule.Remove(kvp.Key);
                break;
            }
        }
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
    public GameDate GetNextFreeDateForEvent(GameEvent gameEvent) {
        //this is only ususally called when there is a conflict in the schedule
        //check the length from today to the earliest schedule, if the event (given its duration), can fit, schedule it there?
        int counter = 0;
        //check each entry in the schedule
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {//for each schedule element
            DateRange currRange = kvp.Key;
            GameEvent currEvent = kvp.Value;
            GameDate nextFreeDate;
            if (counter + 1 == eventSchedule.Count) { //check if this is the last element of the schedule
                //if it is, schedule the event anywhere after this element's endDate
                nextFreeDate = currRange.endDate;
                //schedule the next event 1 day after
                nextFreeDate.AddDays(1);
                counter++;
                return nextFreeDate;
            } else { //if this element is not the last element 
                KeyValuePair<DateRange, GameEvent> nextElement = eventSchedule.ElementAt(counter + 1);
                DateRange nextElementRange = nextElement.Key;
                DateRange availableRange = new DateRange(currRange.endDate, nextElement.Key.startDate); //check this element's endDate and the next elements startDate
                if (availableRange.rangeInTicks + GameManager.hoursPerDay >= gameEvent.GetEventDurationRoughEstimateInTicks()) {
                    //if the distance between the 2 dates can fit the event (given it's duration), schedule the event in between the 2 dates
                    nextFreeDate = availableRange.startDate;
                    nextFreeDate.AddDays(1);
                    return nextFreeDate;
                } else {
                    //if not, continue to the next element in the schedule.
                    continue;
                }
            }
        }
        Debug.LogWarning("A character cannot find a date to schedule new event!");
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

    public DateRange GetDateRangeForEvent(GameEvent gameEvent) {
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {
            if (kvp.Value.id == gameEvent.id) {
                return kvp.Key;
            }
        }
        throw new System.Exception("There is no game event " + gameEvent.name + " in the character's schedule!");
    }
    
}
