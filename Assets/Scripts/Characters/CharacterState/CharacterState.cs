using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState {
    public CharacterStateComponent stateComponent { get; protected set; }
    public string stateName { get; protected set; }
    public CHARACTER_STATE characterState { get; protected set; }
    public CHARACTER_STATE_CATEGORY stateCategory { get; protected set; }
    public int duration { get; protected set; } // 0 means no duration - end state immediately
    public int currentDuration { get; protected set; }
    public bool isDone { get; protected set; }
    public bool hasStarted { get; protected set; }
    public bool isPaused { get; protected set; }
    public Log thoughtBubbleLog { get; protected set; }
    public CharacterStateJob job { get; protected set; }
    public string actionIconString { get; protected set; }
    public GoapAction currentlyDoingAction { get; protected set; }

    public Character targetCharacter { get; protected set; } //Target character of current state
    public Area targetArea { get; protected set; }
    public bool isUnending { get; protected set; } //is this state unending?
    //public CharacterState parentMajorState { get; protected set; }

    public System.Action startStateAction { get; protected set; }
    public System.Action endStateAction { get; protected set; }

    public CharacterState(CharacterStateComponent characterComp) {
        this.stateComponent = characterComp;
        actionIconString = GoapActionStateDB.No_Icon;
        isUnending = false;
        AddDefaultListeners();
    }

    #region Virtuals
    //Starts a state and its movement behavior, can be overridden
    protected virtual void StartState() {
        hasStarted = true;
        stateComponent.SetStateToDo(null, false, false);
        stateComponent.SetCurrentState(this);
        currentDuration = 0;
        StartStatePerTick();

        CreateStartStateLog();
        CreateThoughtBubbleLog();
        DoMovementBehavior();
        Messenger.Broadcast(Signals.CHARACTER_STARTED_STATE, stateComponent.character, this);
        InVisionPOIsOnStartState();
        if(startStateAction != null) {
            startStateAction();
        }
    }
    /// <summary>
    /// End this state. This is called after <see cref="OnExitThisState"/>.
    /// </summary>
    protected virtual void EndState() {
        if (currentlyDoingAction != null) {
            if (currentlyDoingAction.isPerformingActualAction && !currentlyDoingAction.isDone) {
                currentlyDoingAction.SetEndAction(FakeEndAction);
                currentlyDoingAction.currentState.EndPerTickEffect(false);
            }
            stateComponent.character.SetCurrentAction(null);
            SetCurrentlyDoingAction(null);
        }
        isDone = true;
        StopStatePerTick();
        RemoveDefaultListeners();
        if(job != null) {
            job.jobQueueParent.RemoveJobInQueue(job);
            job.SetAssignedCharacter(null);
            job.SetAssignedState(null);
        }
        if (endStateAction != null) {
            endStateAction();
        }
        Messenger.Broadcast(Signals.CHARACTER_ENDED_STATE, stateComponent.character, this);
    }
    
    //This is called per TICK_ENDED if the state has a duration, can be overriden
    protected virtual void PerTickInState() {
        if (!isPaused && !isUnending) {
            if (currentDuration >= duration) {
                StopStatePerTick();
                OnExitThisState();
            } else if (stateComponent.character.doNotDisturb > 0) {
                //if (!(characterState == CHARACTER_STATE.BERSERKED && stateComponent.character.doNotDisturb == 1 && stateComponent.character.GetNormalTrait("Combat Recovery") != null)) {
                    StopStatePerTick();
                    OnExitThisState();
                //}
            }
            currentDuration++;
        }
    }
    //Character will do the movement behavior of this state, can be overriden
    protected virtual void DoMovementBehavior() {}

    //What happens when you see another point of interest (character, tile objects, etc)
    public virtual bool OnEnterVisionWith(IPointOfInterest targetPOI) { return false; }

    //What happens if there are already point of interest in your vision upon entering the state
    public virtual bool InVisionPOIsOnStartState() {
        if(stateComponent.character.marker.inVisionPOIs.Count > 0) {
            return true;
        }
        return false;
    }

    //This is called for exiting current state, I made it a virtual because some states still requires something before exiting current state
    public virtual void OnExitThisState() {
        stateComponent.ExitCurrentState(this);
    }

    //Typically used if there are other data that is needed to be set for this state when it starts
    //Currently used only in combat state so we can set the character's behavior if attacking or not when it enters the state
    public virtual void SetOtherDataOnStartState(object otherData) { }

    //This is called on ExitCurrentState function in CharacterStateComponent after all exit processing is finished
    public virtual void AfterExitingState() {
        stateComponent.character.marker.UpdateActionIcon();
    }
    public virtual bool CanResumeState() {
        return true;
    }
    #endregion

    private void FakeEndAction(string str, GoapAction action) {
        //This is just a fake holder end action so that the currently doing action will not go to its actual end action (ex. PatrolAgain)
        //This is done because we don't want the GoapActionResult to be called as well as the actual end action
    }

    //Stops the timer of this state
    public void StopStatePerTick() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickInState);
        }
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
    //This is the action that is currently being done while in this state, ex. pick up item
    public void SetCurrentlyDoingAction(GoapAction action) {
        currentlyDoingAction = action;
    }
    //public void SetParentMajorState(CharacterState majorState) {
    //    parentMajorState = majorState;
    //}
    //This is the one must be called to enter and start this state, if it is already done, it cannot start again
    public void EnterState(Area area) {
        if (isDone) {
            return;
        }
        stateComponent.SetStateToDo(this, stopMovement: false);
        targetArea = area;
        if(targetArea == null || targetArea == stateComponent.character.specificLocation) {
            Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name + " targetting " + targetCharacter?.name);
            StartState();
        } else {
            //GameDate dueDate = GameManager.Instance.Today().AddTicks(30);
            //SchedulingManager.Instance.AddEntry(dueDate, () => GoToLocation(targetArea));
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

    //private void GoToLocation(Area targetArea) {
    //    CreateTravellingThoughtBubbleLog(targetArea);
    //    Debug.Log(GameManager.Instance.TodayLogString() + "Travelling to " + targetArea.name + " before entering " + stateName + " for " + stateComponent.character.name);
    //    stateComponent.character.currentParty.GoToLocation(targetArea, PATHFINDING_MODE.NORMAL, null, () => StartState());
    //}
    //This is the one must be called to exit and end this state
    public void ExitState() {
        Debug.Log(GameManager.Instance.TodayLogString() + "Exiting " + stateName + " for " + stateComponent.character.name + " targetting " + targetCharacter?.name ?? "No One");
        EndState();
    }
    //Pauses this state, used in switching states if this is a major state
    public virtual void PauseState() {
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

            PlayerManager.Instance.player.ShowNotificationFrom(log, stateComponent.character, false);
        }
    }
    private void CreateTravellingThoughtBubbleLog(Area targetLocation) {
        if (LocalizationManager.Instance.HasLocalizedValue("CharacterState", this.GetType().ToString(), "thought_bubble_m")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "thought_bubble_m");
            thoughtBubbleLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    public void SetStartStateAction(System.Action action) {
        startStateAction = action;
    }
    public void SetEndStateAction(System.Action action) {
        endStateAction = action;
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
            StopStatePerTick();
            RemoveDefaultListeners();
        }
    }
    #endregion

    #region Utilities
    internal void ChangeDuration(int newDuration) {
        duration = newDuration;
    }
    /// <summary>
    /// Set if this state only has a specific duration, or will it run indefinitely until stopped.
    /// </summary>
    /// <param name="state">If the state should be unending or not.</param>
    public void SetIsUnending(bool state) {
        isUnending = state;
    }
    #endregion

}
