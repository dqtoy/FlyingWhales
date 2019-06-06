using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJobAction {

    public PlayerJobData parentData { get; protected set; }
    public string name { get; protected set; }
	public int cooldown { get; protected set; } //cooldown in ticks
    public Character assignedCharacter { get; protected set; }
    public List<JOB_ACTION_TARGET> targettableTypes { get; protected set; } //what sort of objects can this action target
    public bool isActive { get; protected set; }
    public string btnSubText { get; protected set; }
    public int ticksInCooldown { get; private set; } //how many ticks has this action been in cooldown?

    public bool isInCooldown {
        get { return ticksInCooldown != cooldown; } //check if the ticks this action has been in cooldown is the same as cooldown
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
        parentData.SetActiveAction(this);
        ActivateCooldown();
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
    }
    public virtual void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) { //this is called when the actions button is pressed
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
        } else if (obj is IPointOfInterest) {
            return ShouldButtonBeInteractable(character, obj as IPointOfInterest);
        }
        return ShouldButtonBeInteractable();
    }
    protected virtual bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        return ShouldButtonBeInteractable();
    }
    protected virtual bool ShouldButtonBeInteractable(Character character, Area targetCharacter) {
        return ShouldButtonBeInteractable();
    }
    protected virtual bool ShouldButtonBeInteractable(Character character, IPointOfInterest targetPOI) {
        return ShouldButtonBeInteractable();
    }
    /// <summary>
    /// Function that determines whether this action can target the given character or not.
    /// Regardless of cooldown state.
    /// </summary>
    /// <param name="character">The target poi</param>
    /// <returns>true or false</returns>
    public virtual bool CanTarget(IPointOfInterest poi) {
        return true;
    }
    public virtual string GetActionName(Character target) {
        return name;
    }
    #endregion

    #region Cooldown
    protected void SetDefaultCooldownTime(int cooldown) {
        this.cooldown = cooldown;
        ticksInCooldown = cooldown;
    }
    private void ActivateCooldown() {
        ticksInCooldown = 0;
        parentData.SetLockedState(true);
        Messenger.AddListener(Signals.TICK_ENDED, CheckForCooldown);
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, this);
    }
    private void CheckForCooldown() {
        if (ticksInCooldown == cooldown) {
            //cooldown has been reached!
            OnCooldownDone();
        } else {
            ticksInCooldown++;
        }
    }
    private void OnCooldownDone() {
        parentData.SetLockedState(false);
        Messenger.RemoveListener(Signals.TICK_ENDED, CheckForCooldown);
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_DONE, this);
    }
    private void ResetCooldown() {
        ticksInCooldown = cooldown;
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
