using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;

public class PlayerJobAction {

    //public PlayerJobData parentData { get; protected set; }
    //public Minion minion { get; protected set; }
    public INTERVENTION_ABILITY abilityType { get; protected set; }
    public string name { get { return PlayerManager.Instance.allInterventionAbilitiesData[abilityType].name; } }
    public string description { get { return PlayerManager.Instance.allInterventionAbilitiesData[abilityType].description; } }
    public int tier { get; protected set; }
    public int abilityRadius { get; protected set; } //0 means single target
    public virtual string dynamicDescription { get { return description; } }
    public int cooldown { get; protected set; } //cooldown in ticks
    //public Character assignedCharacter { get; protected set; }
    public JOB_ACTION_TARGET[] targetTypes { get; protected set; } //what sort of object does this action target
    public bool isActive { get; protected set; }
    public int ticksInCooldown { get; private set; } //how many ticks has this action been in cooldown?
    public int level { get; protected set; }
    //public List<ABILITY_TAG> abilityTags { get; protected set; }
    public bool hasSecondPhase { get; protected set; }
    public bool isInSecondPhase { get; protected set; }

    #region getters/setters
    public string worldObjectName {
        get { return name; }
    }
    public bool isInCooldown {
        get { return ticksInCooldown != cooldown; } //check if the ticks this action has been in cooldown is the same as cooldown
    }
    #endregion



    //public void SetParentData(PlayerJobData data) {
    //    parentData = data;
    //}
    //public void SetMinion(Minion minion) {
    //    this.minion = minion;
    //}

    public PlayerJobAction(INTERVENTION_ABILITY abilityType) {
        this.abilityType = abilityType;
        //this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.abilityType.ToString());
        //abilityTags = new List<ABILITY_TAG>();
        this.level = 1;
        this.tier = PlayerManager.Instance.GetInterventionAbilityTier(abilityType);
        this.abilityRadius = 0;
        hasSecondPhase = false;
        OnLevelUp();
    }

    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
        OnLevelUp();
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
        OnLevelUp();
    }

    #region Virtuals
    public virtual void ActivateAction() { //this is called when the actions button is pressed
        if (this.isActive) { //if this action is still active, deactivate it first
            DeactivateAction();
        }
        //this.assignedCharacter = assignedCharacter;
        isActive = true;
        //parentData.SetActiveAction(this);
        //ActivateCooldown();
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
        PlayerManager.Instance.player.ConsumeAbility(this);
    }
    public virtual void ActivateAction(IPointOfInterest targetPOI) { //this is called when the actions button is pressed
        ActivateAction();
    }
    public virtual void ActivateAction(Area targetArea) { //this is called when the actions button is pressed
        ActivateAction();
    }
    public virtual void ActivateAction(LocationGridTile targetTile) { 
        ActivateAction();
    }
    public virtual void ActivateAction(List<IPointOfInterest> targetPOIs) {
        ActivateAction();
    }
    public virtual void DeactivateAction() { //this is typically called when the character is assigned to another action or the assigned character dies
        isActive = false;
        //parentData.SetActiveAction(null);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.RemoveListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
    }
    protected virtual void OnCharacterDied(Character characterThatDied) {
        //if (assignedCharacter != null && characterThatDied.id == assignedCharacter.id) {
        //    DeactivateAction();
        //    //ResetCooldown(); //only reset cooldown if the assigned character dies
        //}
    }
    protected virtual void OnCharacterUnassignedFromJob(JOB job, Character character) {
        //if (character.id == assignedCharacter.id) {
        //    DeactivateAction();
        //}
    }
    /// <summary>
    /// Can this action currently be performed.
    /// </summary>
    /// <returns>True or False</returns>
    public virtual bool CanPerformAction() {
        if (isInCooldown) {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Can this action be performed this instant? This considers cooldown.
    /// </summary>
    /// <param name="character">The character that will perform the action (Minion).</param>
    /// <param name="obj">The target object.</param>
    /// <returns>True or False.</returns>
    public bool CanPerformActionTowards(object obj) {
        if (obj is Character) {
            return CanPerformActionTowards(obj as Character);
        } else if (obj is Area) {
            return CanPerformActionTowards(obj as Area);
        } else if (obj is IPointOfInterest) {
            return CanPerformActionTowards(obj as IPointOfInterest);
        } else if (obj is LocationGridTile) {
            return CanPerformActionTowards(obj as LocationGridTile);
        }
        return CanPerformAction();
    }
    protected virtual bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Blessed") != null) {
            return false;
        }
        //Quick fix only, remove this later
        if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        return CanPerformAction();
    }
    protected virtual bool CanPerformActionTowards(Area targetCharacter) {
        return CanPerformAction();
    }
    protected virtual bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        return CanPerformAction();
    }
    protected virtual bool CanPerformActionTowards(LocationGridTile tile) {
        return CanPerformAction();
    }
    public virtual bool CanPerformActionTowards(List<IPointOfInterest> targetPOIs) {
        return CanPerformAction();
    }
    /// <summary>
    /// Function that determines whether this action can target the given character or not.
    /// Regardless of cooldown state.
    /// </summary>
    /// <param name="poi">The target poi</param>
    /// <returns>true or false</returns>
    public virtual bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi.traitContainer.GetNormalTrait<Trait>("Blessed") != null) {
            hoverText = "Blessed characters cannot be targetted.";
            return false;
        }
        //Quick fix only, remove this later
        if (poi is Character) {
            Character targetCharacter = poi as Character;
            if(targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
                return false;
            }
        }
        hoverText = string.Empty;
        return true;
    }
    public virtual bool CanTarget(LocationGridTile tile) {
        return true;
    }
    public virtual bool CanTarget(List<IPointOfInterest> targetPOIs) {
        return true;
    }
    public virtual string GetActionName(Character target) {
        return name;
    }
    protected virtual void OnLevelUp() { }
    public virtual void SecondPhase() { }
    /// <summary>
    /// If the ability has a range, override this to show that range. <see cref="CursorManager.Update"/>
    /// </summary>
    public virtual void ShowRange(LocationGridTile tile) { }
    public virtual void HideRange(LocationGridTile tile) { }
    #endregion

    #region Cooldown
    protected void SetDefaultCooldownTime(int cooldown) {
        this.cooldown = cooldown;
        ticksInCooldown = cooldown;
    }
    private void ActivateCooldown() {
        ticksInCooldown = 0;
        //parentData.SetLockedState(true);
        //Messenger.AddListener(Signals.TICK_ENDED, CheckForCooldown); //IMPORTANT NOTE: Cooldown will start but will not actually finish because this line of code is removed. This is removed this so that the ability can only be used once. Upon every enter of the area map, all cooldowns of intervention abilities must be reset
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
    public void InstantCooldown() {
        ticksInCooldown = cooldown;
        OnCooldownDone();
    }
    private void OnCooldownDone() {
        //parentData.SetLockedState(false);
        //Messenger.RemoveListener(Signals.TICK_ENDED, CheckForCooldown);
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_DONE, this);
    }
    private void ResetCooldown() {
        ticksInCooldown = cooldown;
        //parentData.SetLockedState(false);
    }
    #endregion
}

public class PlayerJobActionData {
    public virtual string name { get { return string.Empty; } }
    public virtual string description { get { return string.Empty; } }
    public virtual INTERVENTION_ABILITY_CATEGORY category { get { return INTERVENTION_ABILITY_CATEGORY.NONE; } }
    public virtual int manaCost { get { return PlayerManager.Instance.player.GetManaCostForInterventionAbility(name.ToUpper().Replace(' ', '_')); } }
}

public class PlayerJobActionSlot {
    public int level;
    public PlayerJobAction ability;

    public PlayerJobActionSlot() {
        level = 1;
        ability = null;
    }

    public void SetAbility(PlayerJobAction ability) {
        this.ability = ability;
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
    }

    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_INTERVENE_LEVEL, this);
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_INTERVENE_LEVEL, this);
    }
}