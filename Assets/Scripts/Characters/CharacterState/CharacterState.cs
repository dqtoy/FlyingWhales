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
    public bool isPaused { get; protected set; }
    public Log thoughtBubbleLog { get; protected set; }

    public Character targetCharacter { get; protected set; } //Target character of current state
    //public CharacterState parentMajorState { get; protected set; }

    public CharacterState(CharacterStateComponent characterComp) {
        this.stateComponent = characterComp;
    }

    #region Virtuals
    //Starts a state and its movement behavior, can be overridden
    protected virtual void StartState() {
        currentDuration = 0;
        StartStatePerTick();

        thoughtBubbleLog = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "thought_bubble");
        thoughtBubbleLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if (targetCharacter != null) {
            thoughtBubbleLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
        }

        DoMovementBehavior();
    }
    //Ends a state, can be overridden
    protected virtual void EndState() {
        isDone = true;
        StopStatePerTick();
    }
    
    //This is called per TICK_ENDED if the state has a duration, can be overriden
    protected virtual void PerTickInState() {
        if(currentDuration >= duration) {
            StopStatePerTick();
            OnExitThisState();
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
        if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickInState);
        }
    }
    //Starts the timer of this state
    public void StartStatePerTick() {
        if (duration > 0) {
            Messenger.AddListener(Signals.TICK_ENDED, PerTickInState);
        }
    }
    //Sets the target character of this state, if there's any
    public void SetTargetCharacter(Character target) {
        targetCharacter = target;
    }
    //public void SetParentMajorState(CharacterState majorState) {
    //    parentMajorState = majorState;
    //}
    //This is the one must be called to enter and start this state, if it is already done, it cannot start again
    public void EnterState() {
        if (isDone) {
            return;
        }
        Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name);
        StartState();
    }
    //This is the one must be called to exit and end this state
    public void ExitState() {
        Debug.Log(GameManager.Instance.TodayLogString() + "Exiting " + stateName + " for " + stateComponent.character.name);
        EndState();
    }
    //Pauses this state, used in switching states if this is a major state
    public void PauseState() {
        isPaused = true;
        StopStatePerTick();
    }
    //Resumes the state and its movement behavior
    public void ResumeState() {
        isPaused = false;
        StartStatePerTick();
        DoMovementBehavior();
    }
}
