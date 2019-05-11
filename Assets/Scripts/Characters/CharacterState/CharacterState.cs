using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState {
    public CharacterStateComponent stateComponent { get; protected set; }
    public string stateName { get; protected set; }
    public CHARACTER_STATE characterState { get; protected set; }
    public CHARACTER_STATE_CATEGORY stateCategory { get; protected set; }
    public int duration { get; protected set; } // 0 means infinite
    public int currentDuration { get; protected set; }
    public bool isDone { get; protected set; }
    public bool hasStarted { get; protected set; }
    public bool isPaused { get; protected set; }
    public Log thoughtBubbleLog { get; protected set; }
    public CharacterStateJob job { get; protected set; }
    public string actionIconString { get; protected set; }

    public Character targetCharacter { get; protected set; } //Target character of current state
    public Area targetArea { get; protected set; }
    //public CharacterState parentMajorState { get; protected set; }

    public CharacterState(CharacterStateComponent characterComp) {
        this.stateComponent = characterComp;
        actionIconString = GoapActionStateDB.No_Icon;
        AddDefaultListeners();
    }

    #region Virtuals
    //Starts a state and its movement behavior, can be overridden
    protected virtual void StartState() {
        hasStarted = true;
        stateComponent.SetStateToDo(null, false);
        stateComponent.SetCurrentState(this);
        currentDuration = 0;
        StartStatePerTick();

        CreateStartStateLog();
        CreateThoughtBubbleLog();
        DoMovementBehavior();
        stateComponent.character.OnCharacterEnteredState(this);
        Messenger.Broadcast(Signals.CHARACTER_STARTED_STATE, stateComponent.character, this);
    }
    //Ends a state, can be overridden
    protected virtual void EndState() {
        isDone = true;
        StopStatePerTick();
        RemoveDefaultListeners();
        if(job != null) {
            job.jobQueueParent.RemoveJobInQueue(job);
            job.SetAssignedCharacter(null);
            job.SetAssignedState(null);
        }
        Messenger.Broadcast(Signals.CHARACTER_ENDED_STATE, stateComponent.character, this);
    }
    
    //This is called per TICK_ENDED if the state has a duration, can be overriden
    protected virtual void PerTickInState() {
        if (currentDuration >= duration) {
            StopStatePerTick();
            OnExitThisState();
        } else if (stateComponent.character.doNotDisturb > 0) {
            if (!(characterState == CHARACTER_STATE.BERSERKED && stateComponent.character.doNotDisturb == 1 && stateComponent.character.GetTrait("Combat Recovery") != null)) {
                StopStatePerTick();
                OnExitThisState();
            }
        }
        currentDuration++;
    }
    //Character will do the movement behavior of this state, can be overriden
    protected virtual void DoMovementBehavior() {}

    //What happens when you see another point of interest (character, tile objects, etc)
    public virtual bool OnEnterVisionWith(IPointOfInterest targetPOI) { return false; }

    //This is called for exiting current state, I made it a virtual because some states still requires something before exiting current state
    public virtual void OnExitThisState() {
        stateComponent.ExitCurrentState(this);
    }
    #endregion

    //Stops the timer of this state
    public void StopStatePerTick() {
        //if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickInState);
        //}
    }
    //Starts the timer of this state
    public void StartStatePerTick() {
        //if(duration > 0) {
            Messenger.AddListener(Signals.TICK_ENDED, PerTickInState);
        //}
    }
    //Sets the target character of this state, if there's any
    public void SetTargetCharacter(Character target) {
        targetCharacter = target;
    }
    //public void SetParentMajorState(CharacterState majorState) {
    //    parentMajorState = majorState;
    //}
    //This is the one must be called to enter and start this state, if it is already done, it cannot start again
    public void EnterState(Area area) {
        if (isDone) {
            return;
        }
        stateComponent.SetStateToDo(this);
        targetArea = area;
        if(targetArea == null || targetArea == stateComponent.character.specificLocation) {
            Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name);
            StartState();
        } else {
            CreateTravellingThoughtBubbleLog(targetArea);
            Debug.Log(GameManager.Instance.TodayLogString() + "Travelling to " + targetArea.name + " before entering " + stateName + " for " + stateComponent.character.name);
            stateComponent.character.currentParty.GoToLocation(targetArea, PATHFINDING_MODE.NORMAL, null, () => StartState());
        }
        //if(characterState == CHARACTER_STATE.EXPLORE) {
        //    //There is a special case for explore state, character must travel to a dungeon-type area first
        //    Area dungeon = LandmarkManager.Instance.GetRandomAreaOfType(AREA_TYPE.DUNGEON);
        //    if(dungeon == stateComponent.character.specificLocation) {
        //        Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name);
        //        StartState();
        //    } else {
        //        CreateTravellingThoughtBubbleLog(dungeon);
        //        Debug.Log(GameManager.Instance.TodayLogString() + "Travelling to " + dungeon.name + " before entering " + stateName + " for " + stateComponent.character.name);
        //        stateComponent.character.currentParty.GoToLocation(dungeon, PATHFINDING_MODE.NORMAL, null, () => StartState());
        //    }
        //} else {
        //    Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name);
        //    StartState();
        //}
    }
    //This is the one must be called to exit and end this state
    public void ExitState() {
        Debug.Log(GameManager.Instance.TodayLogString() + "Exiting " + stateName + " for " + stateComponent.character.name + " targetting " + targetCharacter?.name ?? "No One");
        EndState();
    }
    //Pauses this state, used in switching states if this is a major state
    public void PauseState() {
        Debug.Log(GameManager.Instance.TodayLogString() + "Pausing " + stateName + " for " + stateComponent.character.name);
        isPaused = true;
        StopStatePerTick();
    }
    //Resumes the state and its movement behavior
    public void ResumeState() {
        Debug.Log(GameManager.Instance.TodayLogString() + "Resuming " + stateName + " for " + stateComponent.character.name);
        isPaused = false;
        StartStatePerTick();
        DoMovementBehavior();
    }

    public void SetJob(CharacterStateJob job) {
        this.job = job;
    }

    private void CreateThoughtBubbleLog() {
        if(LocalizationManager.Instance.HasLocalizedValue("CharacterState", this.GetType().ToString(), "thought_bubble")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "thought_bubble");
            thoughtBubbleLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            if (targetCharacter != null) {
                thoughtBubbleLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
            }
        }
    }
    private void CreateStartStateLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("CharacterState", this.GetType().ToString(), "start")) {
            Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "start");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            if (targetCharacter != null) {
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
            }
            if(targetArea != null) {
                log.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
            }
            log.AddLogToInvolvedObjects();

            if (PlayerManager.Instance.player.ShouldShowNotificationFrom(stateComponent.character)) {
                PlayerManager.Instance.player.ShowNotification(log);
            }
        }
    }
    private void CreateTravellingThoughtBubbleLog(Area targetLocation) {
        if (LocalizationManager.Instance.HasLocalizedValue("CharacterState", this.GetType().ToString(), "thought_bubble_m")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "thought_bubble_m");
            thoughtBubbleLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }

    #region Listeners
    private void AddDefaultListeners() {
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void RemoveDefaultListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    //handler for when the character that owns this dies
    private void OnCharacterDied(Character character) {
        if (character.id == stateComponent.character.id) {
            if (duration > 0) {
                StopStatePerTick();
            }
            RemoveDefaultListeners();
        }
    }
    #endregion

}
