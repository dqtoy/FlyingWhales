using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for World Events. NOTE: This should be coded in way that no special varaibles are needed for the event to work, 
/// since World Event Classes are only created on startup and reused everytime to cut down on memory costs from creating a "new" instance every time. <see cref="StoryEventsManager.LoadWorldEvents"/>
/// </summary>
public class WorldEvent  {

    public int duration { get; protected set; }
    public WORLD_EVENT eventType { get; protected set; }
    public string name { get; private set; }
    public string description { get; private set; }
    public bool isUnique { get; protected set; } //should this event only spawn once?
    private bool hasBeenSpawnedOnce;

    public WorldEvent(WORLD_EVENT eventType) {
        this.eventType = eventType;
        name = Utilities.NormalizeStringUpperCaseFirstLetters(eventType.ToString());
        description = "This is a test description";
    }

    #region Virtuals
    /// <summary>
    /// Spawn an event at a given landmark. Also execute any effects that the event has upon spawn.
    /// </summary>
    /// <param name="landmark">The landmark this event is spawned at.</param>
    /// <param name="afterEffectScheduleID">The schedule id that this event has created for its after effect</param>
    public virtual void Spawn(BaseLandmark landmark, out string afterEffectScheduleID) {
        //once spawned, schedule the after effect of this event to execute after a set amount of ticks (duration). NOTE: This schedule should be cancelled once the landmark it spawned at 
        afterEffectScheduleID = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(duration), () => ExecuteAfterEffect(landmark), this);
        TimerHubUI.Instance.AddItem(this.name + " event at " + landmark.tileLocation.region.name, duration, () => UIManager.Instance.ShowRegionInfo(landmark.tileLocation.region));
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " spawned at " + landmark.tileLocation.region.name);
        //Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "spawn");
        //AddDefaultFillersToLog(log, landmark);
        hasBeenSpawnedOnce = true;
    }
    public virtual void ExecuteAfterEffect(BaseLandmark landmark) {
        landmark.WorldEventFinished(this);
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " after effect executed at " + landmark.tileLocation.region.name);
    }
    public virtual void ExecuteAfterInvasionEffect(BaseLandmark landmark) {
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " after invasion effect executed at " + landmark.tileLocation.region.name);
        if (LocalizationManager.Instance.HasLocalizedValue("WorldEvent", this.GetType().ToString(), "after_invasion_effect")) {
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_invasion_effect");
            AddDefaultFillersToLog(log, landmark);
            log.AddLogToInvolvedObjects();
        }
    }
    public virtual bool CanSpawnEventAt(BaseLandmark landmark) {
        if (this.isUnique && this.hasBeenSpawnedOnce) {
            return false; //if this event is unique and has been spawned once, do not allow it to spawn again
        }
        return true;
    }
    public virtual Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return null;
    }
    #endregion

    protected void AddDefaultFillersToLog(Log log, BaseLandmark landmark) {
        log.AddToFillers(landmark.eventSpawnedBy, landmark.eventSpawnedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(landmark.tileLocation.region, landmark.tileLocation.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
}
