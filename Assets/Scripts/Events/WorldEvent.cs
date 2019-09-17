using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public WORLD_EVENT_EFFECT[] eventEffects { get; protected set; }
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
    /// <param name="region">The region this event is spawned at.</param>
    /// <param name="afterEffectScheduleID">The schedule id that this event has created for its after effect</param>
    public virtual void Spawn(Region region, Character spawner, IWorldEventData eventData, out string afterEffectScheduleID) {
        GameDate startDate = GameManager.Instance.Today();
        GameDate endDate = GameManager.Instance.Today().AddTicks(duration);

        eventData.SetStartDate(startDate);
        eventData.SetEndDate(endDate);

        //once spawned, schedule the after effect of this event to execute after a set amount of ticks (duration). NOTE: This schedule should be cancelled once the landmark it spawned at 
        afterEffectScheduleID = SchedulingManager.Instance.AddEntry(endDate, () => TryExecuteAfterEffect(region, spawner), this);
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " spawned at " + region.name);
        //Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "spawn");
        //AddDefaultFillersToLog(log, landmark);
        isCurrentlySpawned = true;
        if (PlayerManager.Instance.player.HasMinionAssignedTo(LANDMARK_TYPE.THE_EYE)) {
            //notify player
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", "Generic", "spawned");
            log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(region, region.name, LOG_IDENTIFIER.LANDMARK_1);
            PlayerManager.Instance.player.ShowNotification(log);
            TimerHubUI.Instance.AddItem(this.name + " event at " + region.name, duration, () => UIManager.Instance.ShowHextileInfo(region.coreTile));
        }
    }
    public virtual void Load(Region region, Character spawner, IWorldEventData eventData, out string afterEffectScheduleID) {
        GameDate startDate = GameManager.Instance.Today();
        GameDate endDate = eventData.endDate;

        int ticksDiff = GameManager.Instance.GetTicksDifferenceOfTwoDates(endDate, startDate);
        //once spawned, schedule the after effect of this event to execute after a set amount of ticks (duration). NOTE: This schedule should be cancelled once the landmark it spawned at 
        afterEffectScheduleID = SchedulingManager.Instance.AddEntry(endDate, () => TryExecuteAfterEffect(region, spawner), this);
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " spawned at " + region.name);
        //Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "spawn");
        //AddDefaultFillersToLog(log, landmark);
        isCurrentlySpawned = true;
        if (PlayerManager.Instance.player.HasMinionAssignedTo(LANDMARK_TYPE.THE_EYE)) {
            //notify player
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", "Generic", "spawned");
            log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(region, region.name, LOG_IDENTIFIER.LANDMARK_1);
            PlayerManager.Instance.player.ShowNotification(log);
            TimerHubUI.Instance.AddItem(this.name + " event at " + region.name, ticksDiff, () => UIManager.Instance.ShowHextileInfo(region.coreTile));
        }
    }
    private void TryExecuteAfterEffect(Region region, Character spawner) {
        if (region.eventData.interferingCharacter != null) {
            //there is an interfering character.
            ExecuteFailEffect(region, spawner);
        } else {
            //there are no interfering characters.
            ExecuteAfterEffect(region, spawner);
        }
    }
    protected virtual void ExecuteFailEffect(Region region, Character spawner) {
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " fail effect executed at " + region.name);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", "Generic", "failed");
        log.AddToFillers(null, this.name, LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(region, region.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddToFillers(region.eventData.interferingCharacter, region.eventData.interferingCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        region.WorldEventFailed(this);
    }
    protected virtual void ExecuteAfterEffect(Region region, Character spawner) {
        region.WorldEventFinished(this);
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " after effect executed at " + region.name);
        hasSuccessfullyExecutedOnce = true;
    }
    public virtual void ExecuteAfterInvasionEffect(Region region) {
        Debug.Log(GameManager.Instance.TodayLogString() + this.name + " after invasion effect executed at " + region.name);
        if (LocalizationManager.Instance.HasLocalizedValue("WorldEvent", this.GetType().ToString(), "after_invasion_effect")) {
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_invasion_effect");
            AddDefaultFillersToLog(log, region);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
        }
    }
    public virtual bool CanSpawnEventAt(Region region, Character spawner) {
        if (this.isUnique && (this.hasSuccessfullyExecutedOnce || this.isCurrentlySpawned)) {
            return false; //if this event is unique and has been spawned once or is currently spawned, do not allow it to spawn again
        }
        return true;
    }
    public virtual void OnDespawn(Region region) {
        isCurrentlySpawned = false;
    }
    public virtual IWorldEventData ConstructEventDataForLandmark(Region region) {
        IWorldEventData data = new DefaultWorldEventData() {
            spawner = region.eventSpawnedBy
        };
        return data;
    }
    #endregion

    #region Utilities
    protected void AddDefaultFillersToLog(Log log, Region region) {
        log.AddToFillers(region.eventSpawnedBy, region.eventSpawnedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(region, region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public bool CanProvideNeededEffects(WORLD_EVENT_EFFECT[] neededEffects) {
        if (neededEffects != null && eventEffects != null) {
            for (int i = 0; i < neededEffects.Length; i++) {
                if (eventEffects.Contains(neededEffects[i])) {
                    return true;
                }
            }
            return false;
        }
        return true;
    }
    #endregion

}

//This is base class where data for each individual landmark is stored.
public interface IWorldEventData {
    Character interferingCharacter { get; }
    Character[] involvedCharacters { get; }
    GameDate endDate { get; }
    GameDate startDate { get; }

    void SetEndDate(GameDate date);
    void SetStartDate(GameDate date);
    void SetInterferingCharacter(Character character);
}

public class DefaultWorldEventData : IWorldEventData {
    public Character spawner;
    public Character[] involvedCharacters { get { return new Character[] { spawner }; } } //does not include interfering character
    public Character interferingCharacter { get; set; } //the character interfering with this event. This should be a minion.

    public GameDate endDate { get; private set; }
    public GameDate startDate { get; private set; }

    public void SetEndDate(GameDate date) {
        endDate = date;
    }
    public void SetStartDate(GameDate date) {
        startDate = date;
    }

    public void SetInterferingCharacter(Character character) {
        interferingCharacter = character;
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
        if (interferingCharacterID != -1) {
            worldEventData.interferingCharacter = CharacterManager.Instance.GetCharacterByID(interferingCharacterID);
        }
        worldEventData.SetEndDate(new GameDate(endMonth, endDay, endYear, endTick));
        worldEventData.SetStartDate(new GameDate(currentMonth, currentDay, currentYear, currentTick));

        return worldEventData;
    }
}

[System.Serializable]
public class SaveDataWorldEventData {
    public int[] involvedCharactersID;
    public int interferingCharacterID;

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
        if (eventData.interferingCharacter != null) {
            interferingCharacterID = eventData.interferingCharacter.id;
        } else {
            interferingCharacterID = -1;
        }
       
    }
    public virtual IWorldEventData Load() {
        return null;
    }
}
