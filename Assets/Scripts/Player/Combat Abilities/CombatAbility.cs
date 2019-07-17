using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAbility {
    public COMBAT_ABILITY type { get; protected set; }
    public string name { get; protected set; }
    public string description { get; protected set; }
    public int lvl { get; protected set; }
    public int abilityRadius { get; protected set; } //0 means single target
    public int cooldown { get; protected set; } //0 means 1 time use only
    public List<ABILITY_TAG> abilityTags { get; protected set; }

    protected int _currentCooldown;

	public CombatAbility(COMBAT_ABILITY type) {
        this.type = type;
        name = Utilities.NormalizeStringUpperCaseFirstLetters(this.type.ToString());
        lvl = 1;
        OnLevelUp();
        abilityRadius = 0;
        cooldown = 0;
        abilityTags = new List<ABILITY_TAG>();
    }


    public void LevelUp() {
        lvl++;
        lvl = Mathf.Clamp(lvl, 1, 3);
        OnLevelUp();
    }

    public bool IsInCooldown() {
        return _currentCooldown < cooldown;
    }
    public void GoOnCooldown() {
        _currentCooldown = 0;
        Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
        //Messenger.Broadcast(Signals.COMBAT_ABILITY_UPDATE_BUTTON, this);
    }
    private void PerTickCooldown() {
        _currentCooldown++;
        if(_currentCooldown >= cooldown) {
            StopCooldown();
        }
    }
    private void StopCooldown() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
        Messenger.Broadcast(Signals.COMBAT_ABILITY_UPDATE_BUTTON, this);
    }

    #region Virtuals
    protected virtual void OnLevelUp() { }

    //For single target abilities
    public virtual void ActivateAbility(IPointOfInterest targetPOI) {
        GoOnCooldown();
    }

    //For AOE effect abilities
    public virtual void ActivateAbility(List<IPointOfInterest> targetPOI) {
        GoOnCooldown();
    }

    public virtual bool CanTarget(IPointOfInterest targetPOI) { return false; }
    #endregion
}