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
    private bool hasSuccessfullyExecutedOnce;
    private bool isCurrentlySpawned;

    public WorldEvent(WORLD_EVENT eventType) {
        this.eventType = eventType;
        name = Utilities.NormalizeStringUpperCaseFirstLetters(eventType.ToString());
        description = "This is a test description";
        duration = 6 * GameManager.ticksPerHour;
    }

    #region Virtuals
    /// <summary>
    /// Spawn an event at a given landmark. Also execute any effects that the event has upon spawn.
    /// </summary>
    /// <param name="landmark">The landmark this event is spawned at.</param>
    /// <param name="afterEffectScheduleID">The schedule id that this event has created for its after effect</param>
    public virtual void Spawn(BaseLandmark landmark, IWorldEventData eventData, out string afterEffectScheduleID) {
        GameDate startDate = GameManager.Instance.Today();
        GameDate endDate = GameManager.Instance.Today().AddTicks(duration);

        eventData.SetStartDate(startDate);
        eventData.SetEndDate(endDate);

        //once spawned, schedule the after effect of this event to execute after a set amount of ticks (duration). NOTE: This schedule should be cancelled once the landmark it spawned at 
        afterEffectScheduleID = SchedulingManager.Instance.AddEntry(endDate, () => ExecuteAfterEffect(landmark), this);
        TimerHubUI.Instance.AddItem(this.name + " event at " + landmark.tileLocation.region.name, duration, () => UIManager.Instance.ShowHextileInfo(landmark.tileLocation));
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " spawned at " + landmark.tileLocation.region.name);
        //Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "spawn");
        //AddDefaultFillersToLog(log, landmark);
        isCurrentlySpawned = true;
    }
    public virtual void Load(BaseLandmark landmark, IWorldEventData eventData, out string afterEffectScheduleID) {
        GameDate startDate = GameManager.Instance.Today();
        GameDate endDate = eventData.endDate;

        int ticksDiff = GameManager.Instance.GetTicksDifferenceOfTwoDates(endDate, startDate);
        //once spawned, schedule the after effect of this event to execute after a set amount of ticks (duration). NOTE: This schedule should be cancelled once the landmark it spawned at 
        afterEffectScheduleID = SchedulingManager.Instance.AddEntry(endDate, () => ExecuteAfterEffect(landmark), this);
        TimerHubUI.Instance.AddItem(this.name + " event at " + landmark.tileLocation.region.name, ticksDiff, () => UIManager.Instance.ShowHextileInfo(landmark.tileLocation));
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " spawned at " + landmark.tileLocation.region.name);
        //Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "spawn");
        //AddDefaultFillersToLog(log, landmark);
        isCurrentlySpawned = true;
    }
    public virtual void ExecuteAfterEffect(BaseLandmark landmark) {
        landmark.WorldEventFinished(this);
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " after effect executed at " + landmark.tileLocation.region.name);
        hasSuccessfullyExecutedOnce = true;
    }
    public virtual void ExecuteAfterInvasionEffect(BaseLandmark landmark) {
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " after invasion effect executed at " + landmark.tileLocation.region.name);
        if (LocalizationManager.Instance.HasLocalizedValue("WorldEvent", this.GetType().ToString(), "after_invasion_effect")) {
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_invasion_effect");
            AddDefaultFillersToLog(log, landmark);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
        }
    }
    public virtual bool CanSpawnEventAt(BaseLandmark landmark) {
        if (this.isUnique && (this.hasSuccessfullyExecutedOnce || this.isCurrentlySpawned)) {
            return false; //if this event is unique and has been spawned once or is currently spawned, do not allow it to spawn again
        }
        return true;
    }
    public virtual Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return null;
    }
    public virtual void OnDespawn(BaseLandmark landmark) {
        isCurrentlySpawned = false;
    }
    public virtual IWorldEventData ConstructEventDataForLandmark(BaseLandmark landmark) {
        IWorldEventData data = new DefaultWorldEventData() {
            spawner = landmark.eventSpawnedBy
        };
        return data;
    }
    public virtual bool IsBasicEvent() {
        return false;
    }
    #endregion

    protected void AddDefaultFillersToLog(Log log, BaseLandmark landmark) {
        log.AddToFillers(landmark.eventSpawnedBy, landmark.eventSpawnedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(landmark.tileLocation.region, landmark.tileLocation.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
}

//This is base class where data for each individual landmark is stored.
public interface IWorldEventData {
    Character[] involvedCharacters { get; }
    GameDate endDate { get; }
    GameDate startDate { get; }

    void SetEndDate(GameDate date);
    void SetStartDate(GameDate date);

}

public class DefaultWorldEventData : IWorldEventData {
    public Character spawner;
    public Character[] involvedCharacters { get { return new Character[] { spawner }; } }

    public GameDate endDate { get; private set; }
    public GameDate startDate { get; private set; }

    public void SetEndDate(GameDate date) {
        endDate = date;
    }
    public void SetStartDate(GameDate date) {
        startDate = date;
    }
}

public class SaveDataDefaultWorldEventData : SaveDataWorldEventData {
    public int spawnerID;

    public override void Save(IWorldEventData eventData) {
        base.Save(eventData);
        if(eventData is DefaultWorldEventData) {
            DefaultWorldEventData data = (DefaultWorldEventData) eventData;
            if(data.spawner != null) {
                spawnerID = data.spawner.id;
            } else {
                spawnerID = -1;
            }
        }
    }
    public override IWorldEventData Load() {
        DefaultWorldEventData worldEventData = new DefaultWorldEventData() {
            spawner = CharacterManager.Instance.GetCharacterByID(spawnerID),
        };
        worldEventData.SetEndDate(new GameDate(endMonth, endDay, endYear, endTick));
        worldEventData.SetStartDate(new GameDate(currentMonth, currentDay, currentYear, currentTick));

        return worldEventData;
    }
}

[System.Serializable]
public class SaveDataWorldEventData {
    public int[] involvedCharactersID;

    public int currentMonth;
    public int currentDay;
    public int currentYear;
    public int currentTick;

    public int endMonth;
    public int endDay;
    public int endYear;
    public int endTick;

    public virtual void Save(IWorldEventData eventData) {
        currentMonth = eventData.startDate.month;
        currentDay = eventData.startDate.day;
        currentYear = eventData.startDate.year;
        currentTick = eventData.startDate.tick;

        endMonth = eventData.endDate.month;
        endDay = eventData.endDate.day;
        endYear = eventData.endDate.year;
        endTick = eventData.endDate.tick;

        involvedCharactersID = new int[eventData.involvedCharacters.Length];
        for (int i = 0; i < eventData.involvedCharacters.Length; i++) {
            involvedCharactersID[i] = eventData.involvedCharacters[i].id;
        }
    }
    public virtual IWorldEventData Load() {
        return null;
    }
}
