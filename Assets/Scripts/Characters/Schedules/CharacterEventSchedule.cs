using ECS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterEventSchedule {

    private Dictionary<DateRange, GameEvent> eventSchedule;
    private Character owner;

    public GameEvent this[GameDate key] {
        get {
            return GetEventForDate(key);
        }
    }

    public CharacterEventSchedule(Character owner) {
        this.owner = owner;
        eventSchedule = new Dictionary<DateRange, GameEvent>();
    }
    public void AddElement(DateRange key, GameEvent value) {
        if (eventSchedule.ContainsKey(key)) {
            throw new System.Exception("There is already a key " + key.ToString() + " in " + owner.name + "'s event schedule!");
        }
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
        if (eventSchedule.Count == 0) { //if there are no events in the schedule
            //schedule the next event a few ticks after today
            GameDate today = GameManager.Instance.Today();
            today.AddDays(10);
            return today;
        } else {
            //check the length from today to the earliest schedule, if the event (given its duration), can fit, schedule it there?
            KeyValuePair<DateRange, GameEvent> firstElement = eventSchedule.ElementAt(0);
            GameDate firstEventStartDate = firstElement.Key.startDate;
            DateRange availableRange = new DateRange(GameManager.Instance.Today(), firstEventStartDate);
            //if the available range is greater than the event duration in ticks + 1/2 day worth of ticks then use that range
            if (availableRange.rangeInTicks  >= gameEvent.GetEventDurationRoughEstimateInTicks() + (GameManager.hoursPerDay/2)) {
                GameDate freeDate = GameManager.Instance.Today();
                freeDate.AddDays(GameManager.hoursPerDay/2);
                return freeDate;
            }
        }
        
        int counter = 0;
        //check each entry in the schedule
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {//for each schedule element
            DateRange currRange = kvp.Key;
            GameEvent currEvent = kvp.Value;
            if (counter + 1 == eventSchedule.Count) { //check if this is the last element of the schedule
                //if it is, schedule the event anywhere after this element's endDate
                GameDate nextFreeDate = currRange.endDate;
                //schedule the next event 1 day after
                nextFreeDate.AddMonths(1);
                return nextFreeDate;
            } else { //if this element is not the last element 
                KeyValuePair<DateRange, GameEvent> nextElement = eventSchedule.ElementAt(counter + 1);
                DateRange nextElementRange = nextElement.Key;
                DateRange availableRange = new DateRange(currRange.endDate, nextElement.Key.startDate); //check this element's endDate and the next elements startDate
                if (availableRange.rangeInTicks >= gameEvent.GetEventDurationRoughEstimateInTicks() + GameManager.hoursPerDay) {
                    //if the distance between the 2 dates can fit the event (given it's duration) + 1 day worth of ticks, schedule the event in between the 2 dates
                    GameDate nextFreeDate = availableRange.startDate;
                    nextFreeDate.AddDays(GameManager.hoursPerDay);
                    return nextFreeDate;
                } else {
                    //if not, continue to the next element in the schedule.
                    counter++;
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

    public bool HasEventOfType(GAME_EVENT eventType) {
        foreach (KeyValuePair<DateRange, GameEvent> kvp in eventSchedule) {
            if (kvp.Value.type == eventType) {
                return true;
            }
        }
        return false;
    }
    
}
