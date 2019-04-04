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

    public CharacterState(CharacterStateComponent characterComp) {
        this.stateComponent = characterComp;
    }

    #region Virtuals
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
    protected virtual void EndState() {
        isDone = true;
        StopStatePerTick();
    }
    protected virtual void PerTickInState() {
        if(currentDuration > duration) {
            StopStatePerTick();
            stateComponent.ExitCurrentState(this);
        }
        currentDuration++;
    }
    protected virtual void DoMovementBehavior() {

    }
    #endregion
    public void StopStatePerTick() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickInState);
        }
    }
    public void StartStatePerTick() {
        if (duration > 0) {
            Messenger.AddListener(Signals.TICK_ENDED, PerTickInState);
        }
    }
    public void SetTargetCharacter(Character target) {
        targetCharacter = target;
    }
    public void EnterState() {
        if (isDone) {
            return;
        }
        Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name);
        StartState();
    }
    public void ExitState() {
        Debug.Log(GameManager.Instance.TodayLogString() + "Exiting " + stateName + " for " + stateComponent.character.name);
        EndState();
    }
    public void PauseState() {
        isPaused = true;
        StopStatePerTick();
    }
    public void ResumeState() {
        isPaused = false;
        StartStatePerTick();
        DoMovementBehavior();
    }
}
