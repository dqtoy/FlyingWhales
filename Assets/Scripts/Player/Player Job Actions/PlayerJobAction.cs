using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJobAction {

    public PlayerJobData parentData { get; protected set; }
    public string actionName { get; protected set; }
	public int cooldown { get; protected set; } //cooldown in days
    public Character assignedCharacter { get; protected set; }
    public List<JOB_ACTION_TARGET> targettableTypes { get; protected set; } //what sort of objects can this action target
    public bool isActive { get; protected set; }
    public string btnSubText { get; protected set; }

    public int daysInCooldown { get; private set; } //how many days has this action been in cooldown?

    public bool isInCooldown {
        get { return daysInCooldown != cooldown; } //check if the days this action has been in cooldown is the same as cooldown
    }

    public void SetParentData(PlayerJobData data) {
        parentData = data;
    }

    #region Virtuals
    public virtual void ActivateAction(Character assignedCharacter) { //this is called when the actions button is pressed
        if (this.isActive) { //if this action is still active, deactivate it first
            DeactivateAction();
        }
        this.assignedCharacter = assignedCharacter;
        isActive = true;
        ActivateCooldown();
        parentData.SetActiveAction(this);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
    }
    public virtual void ActivateAction(Character assignedCharacter, Character targetCharacter) { //this is called when the actions button is pressed
        ActivateAction(assignedCharacter);
    }
    public virtual void ActivateAction(Character assignedCharacter, Area targetArea) { //this is called when the actions button is pressed
        ActivateAction(assignedCharacter);
    }
    public virtual void DeactivateAction() { //this is typically called when the character is assigned to another action or the assigned character dies
        this.assignedCharacter = null;
        isActive = false;
        parentData.SetActiveAction(null);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
    }
    protected virtual void OnCharacterDied(Character characterThatDied) {
        if (assignedCharacter != null && characterThatDied.id == assignedCharacter.id) {
            DeactivateAction();
            ResetCooldown(); //only reset cooldown if the assigned character dies
        }
    }
    protected virtual void OnCharacterUnassignedFromJob(JOB job, Character character) {
        if (character.id == assignedCharacter.id) {
            DeactivateAction();
        }
    }
    public virtual bool ShouldButtonBeInteractable() {
        if (isInCooldown) {
            return false;
        }
        return true;
    }
    public virtual bool ShouldButtonBeInteractable(Character character, object obj) {
        if (obj is Character) {
            return ShouldButtonBeInteractable(character, obj as Character);
        } else if (obj is Area) {
            return ShouldButtonBeInteractable(character, obj as Area);
        }
        return ShouldButtonBeInteractable();
    }
    protected virtual bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        return ShouldButtonBeInteractable();
    }
    protected virtual bool ShouldButtonBeInteractable(Character character, Area targetCharacter) {
        return ShouldButtonBeInteractable();
    }
    #endregion

    #region Cooldown
    protected void SetDefaultCooldownTime(int cooldown) {
        this.cooldown = cooldown;
        daysInCooldown = cooldown;
    }
    private void ActivateCooldown() {
        daysInCooldown = 0;
        parentData.SetLockedState(true);
        Messenger.AddListener(Signals.TICK_ENDED, CheckForCooldown);
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, this);
    }
    private void CheckForCooldown() {
        if (daysInCooldown == cooldown) {
            //cooldown has been reached!
            OnCooldownDone();
        } else {
            daysInCooldown++;
        }
    }
    private void OnCooldownDone() {
        parentData.SetLockedState(false);
        Messenger.RemoveListener(Signals.TICK_ENDED, CheckForCooldown);
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_DONE, this);
    }
    private void ResetCooldown() {
        daysInCooldown = cooldown;
        parentData.SetLockedState(false);
    }
    #endregion

    #region Utilities
    public void SetSubText(string subText) {
        btnSubText = subText;
        Messenger.Broadcast(Signals.JOB_ACTION_SUB_TEXT_CHANGED, this);
    }
    #endregion
}
