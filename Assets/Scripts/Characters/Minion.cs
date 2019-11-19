using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Traits;

public class Minion {
    public const int MAX_INTERVENTION_ABILITY_SLOT = 5;

    public Character character { get; private set; }
    public int exp { get; private set; }
    public int indexDefaultSort { get; private set; }
    public CombatAbility combatAbility { get; private set; }
    public List<string> traitsToAdd { get; private set; }
    public Region assignedRegion { get; private set; } //the landmark that this minion is currently invading. NOTE: This is set on both settlement and non settlement landmarks
    public DeadlySin deadlySin { get { return CharacterManager.Instance.GetDeadlySin(_assignedDeadlySinName); } }
    public bool isAssigned { get { return assignedRegion != null; } } //true if minion is already assigned somewhere else, maybe in construction or research spells
    public List<INTERVENTION_ABILITY> interventionAbilitiesToResearch { get; private set; } //This is a list not array because the abilities here are consumable
    public int spellExtractionCount { get; private set; } //the number of times a spell was extracted from this minion.

    private string _assignedDeadlySinName;

    public Log busyReasonLog { get; private set; } //The reason that this minion is busy

    public Minion(Character character, bool keepData) {
        this.character = character;
        this.exp = 0;
        traitsToAdd = new List<string>();
        //SetUnlockedInterventionSlots(0);
        character.SetMinion(this);
        SetLevel(1);
        //character.characterToken.SetObtainedState(true);
        SetAssignedDeadlySinName(character.characterClass.className);
        character.ownParty.icon.SetVisualState(true);
        if (!keepData) {
            character.SetName(RandomNameGenerator.Instance.GenerateMinionName());
        }
        //SetRandomResearchInterventionAbilities(CharacterManager.Instance.Get3RandomResearchInterventionAbilities(deadlySin));
    }
    public Minion(SaveDataMinion data) {
        this.character = CharacterManager.Instance.GetCharacterByID(data.characterID);
        this.exp = data.exp;
        traitsToAdd = data.traitsToAdd;
        interventionAbilitiesToResearch = data.interventionAbilitiesToResearch;
        SetIndexDefaultSort(data.indexDefaultSort);
        //SetUnlockedInterventionSlots(data.unlockedInterventionSlots);
        character.SetMinion(this);
        character.ownParty.icon.SetVisualState(true);
        SetAssignedDeadlySinName(character.characterClass.className);
        spellExtractionCount = data.spellExtractionCount;
    }
    public void SetAssignedDeadlySinName(string name) {
        _assignedDeadlySinName = name;
    }
    //public void SetEnabledState(bool state) {
    //    if (character.IsInOwnParty()) {
    //        //also set enabled state of other party members
    //        for (int i = 0; i < character.ownParty.characters.Count; i++) {
    //            Character otherChar = character.ownParty.characters[i];
    //            if (otherChar.id != character.id && otherChar.minion != null) {
    //                otherChar.minion.SetEnabledState(state);
    //                if (state) {
    //                    //Since the otherChar will be removed from the party when he is not the owner and state is true, reduce loop count so no argument exception error will be called
    //                    i--;
    //                }
    //            }
    //        }
    //    } else {
    //        //If character is not own party and is enabled, automatically put him in his own party so he can be used again
    //        if (state) {
    //            character.currentParty.RemoveCharacter(character);
    //        }
    //    }
    //    _isEnabled = state;
    //    minionItem.SetEnabledState(state);
    //}
    public void SetRandomResearchInterventionAbilities(List<INTERVENTION_ABILITY> abilities) {
        interventionAbilitiesToResearch = abilities;
    }
    public void RemoveInterventionAbilityToResearch(INTERVENTION_ABILITY abilityType) {
        interventionAbilitiesToResearch.Remove(abilityType);
    }
    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        //character.SetPlayerCharacterItem(item);
    }
    public void AdjustExp(int amount) {
        exp += amount;
        if(exp >= 100) {
            LevelUp();
            exp = 0;
        }else if (exp < 0) {
            exp = 0;
        }
        //_characterItem.UpdateMinionItem();
    }
    public void SetLevel(int level) {
        character.SetLevel(level);
    }
    public void LevelUp() {
        character.LevelUp();
    }
    public void LevelUp(int amount) {
        character.LevelUp(amount);
    }
    public void SetIndexDefaultSort(int index) {
        indexDefaultSort = index;
    }
    public void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null) {
        if (!character.isDead) {
            Area deathLocation = character.ownParty.specificLocation;
            LocationStructure deathStructure = character.currentStructure;
            LocationGridTile deathTile = character.gridTileLocation;

            character.SetIsDead(true);
            character.SetPOIState(POI_STATE.INACTIVE);
            CombatManager.Instance.ReturnCharacterColorToPool(character.characterColor);

            if (character.currentParty.specificLocation == null) {
                throw new Exception("Specific location of " + character.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
            } 
            //else if (character.stateComponent.stateToDo != null) {
            //    character.stateComponent.SetStateToDo(null);
            //}
            character.ForceCancelAllJobsTargettingThisCharacter(false, "target is already dead");
            Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, character, "target is already dead");
            if (character.currentActionNode != null) {
                character.currentActionNode.StopActionNode(false);
            }
            if (character.ownParty.specificLocation != null && character.isHoldingItem) {
                character.DropAllTokens(character.ownParty.specificLocation, character.currentStructure, deathTile, true);
            }

            //clear traits that need to be removed
            character.traitsNeededToBeRemoved.Clear();

            bool wasOutsideSettlement = false;
            if (character.currentRegion != null) {
                wasOutsideSettlement = true;
                character.currentRegion.RemoveCharacterFromLocation(this.character);
            }

            if (!character.IsInOwnParty()) {
                character.currentParty.RemoveCharacter(character);
            }
            character.ownParty.PartyDeath();

            if (character.role != null) {
                character.role.OnDeath(character);
            }

            character.traitContainer.RemoveAllTraitsByType(character, TRAIT_TYPE.CRIMINAL); //remove all criminal type traits

            for (int i = 0; i < character.traitContainer.allTraits.Count; i++) {
                character.traitContainer.allTraits[i].OnDeath(character);
            }

            character.traitContainer.RemoveAllNonPersistentTraits(character);

            character.marker?.OnDeath(deathTile, wasOutsideSettlement);
            //character.SetNumWaitingForGoapThread(0); //for raise dead
            Dead dead = new Dead();
            dead.AddCharacterResponsibleForTrait(responsibleCharacter);
            character.traitContainer.AddTrait(character, dead, gainedFromDoing: deathFromAction);
            PlayerManager.Instance.player.RemoveMinion(this);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, character);

            character.CancelAllJobs();
            StopInvasionProtocol(PlayerManager.Instance.player.currentAreaBeingInvaded);

            //Debug.Log(GameManager.Instance.TodayLogString() + character.name + " died of " + cause);
            Log deathLog;
            if (_deathLog == null) {
                deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
                deathLog.AddToFillers(this, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    deathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        deathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                character.AddHistory(deathLog);
                PlayerManager.Instance.player.ShowNotification(deathLog);
            } else {
                deathLog = _deathLog;
            }
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Minion Died: " +  Utilities.LogReplacer(deathLog), null);
        }
    }

    #region Intervention Abilities
    //public void SetUnlockedInterventionSlots(int amount) {
    //    unlockedInterventionSlots = amount;
    //    unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    //}
    //public void AdjustUnlockedInterventionSlots(int amount) {
    //    unlockedInterventionSlots += amount;
    //    unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    //}
    //public void GainNewInterventionAbility(PlayerJobAction ability, bool showNewAbilityUI = false) {
    //    int currentInterventionAbilityCount = GetCurrentInterventionAbilityCount();
    //    if(currentInterventionAbilityCount < unlockedInterventionSlots) {
    //        for (int i = 0; i < interventionAbilities.Length; i++) {
    //            if (interventionAbilities[i] == null) {
    //                interventionAbilities[i] = ability;
    //                ability.SetMinion(this);
    //                Messenger.Broadcast(Signals.MINION_LEARNED_INTERVENE_ABILITY, this, ability);
    //                if (showNewAbilityUI) {
    //                    PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(this, ability);
    //                }
    //                break;
    //            }
    //        }
    //    } else {
    //        //Broadcast intervention ability is full, must open UI whether player wants to replace ability or discard it
    //        PlayerUI.Instance.replaceUI.ShowReplaceUI(GeAllInterventionAbilities(), ability, ReplaceAbility, RejectAbility);
    //    }
    //}
    //private void ReplaceAbility(object objToReplace, object objToAdd) {
    //    PlayerJobAction replace = objToReplace as PlayerJobAction;
    //    PlayerJobAction add = objToAdd as PlayerJobAction;
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if (interventionAbilities[i] == replace) {
    //            interventionAbilities[i] = add;
    //            add.SetMinion(this);
    //            replace.SetMinion(null);
    //            Messenger.Broadcast(Signals.MINION_LEARNED_INTERVENE_ABILITY, this, add);
    //            break;
    //        }
    //    }
    //}
    //private void RejectAbility(object rejectedObj) { }
    //public void AddInterventionAbility(INTERVENTION_ABILITY ability, bool showNewAbilityUI = false) {
    //    GainNewInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(ability), showNewAbilityUI);
    //}
    //public int GetCurrentInterventionAbilityCount() {
    //    int count = 0;
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if (interventionAbilities[i] != null) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    //public List<PlayerJobAction> GeAllInterventionAbilities() {
    //    List<PlayerJobAction> all = new List<PlayerJobAction>();
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if (interventionAbilities[i] != null) {
    //            all.Add(interventionAbilities[i]);
    //        }
    //    }
    //    return all;
    //}
    //public void ResetInterventionAbilitiesCD() {
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if(interventionAbilities[i] != null) {
    //            interventionAbilities[i].InstantCooldown();
    //        }
    //    }
    //}
    #endregion

    #region Combat Ability
    public void SetCombatAbility(CombatAbility combatAbility, bool showNewAbilityUI = false) {
        if (this.combatAbility == null) {
            this.combatAbility = combatAbility;
            if (combatAbility != null && showNewAbilityUI) {
                PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(this, combatAbility);
            }
            Messenger.Broadcast(Signals.MINION_CHANGED_COMBAT_ABILITY, this);
        } else {
            PlayerUI.Instance.replaceUI.ShowReplaceUI(new List<CombatAbility>() { this.combatAbility }, combatAbility, ReplaceCombatAbility, RejectCombatAbility);
        }
    }
    public void SetCombatAbility(COMBAT_ABILITY combatAbility, bool showNewAbilityUI = false) {
        SetCombatAbility(PlayerManager.Instance.CreateNewCombatAbility(combatAbility), showNewAbilityUI);
    }
    private void ReplaceCombatAbility(object objToReplace, object objToAdd) {
        CombatAbility newAbility = objToAdd as CombatAbility;
        this.combatAbility = newAbility;
    }
    private void RejectCombatAbility(object objToReplace) {

    }
    public void ResetCombatAbilityCD() {
        combatAbility.StopCooldown();
    }
    #endregion

    #region Invasion
    public void StartInvasionProtocol(Area area) {
        AddPendingTraits();
        Messenger.AddListener(Signals.TICK_STARTED, PerTickInvasion);
        Messenger.AddListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, character.OnOtherCharacterDied);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, character.OnCharacterEndedState);
        SetAssignedRegion(area.coreTile.region);
    }
    public void StopInvasionProtocol(Area area) {
        if(area != null && assignedRegion != null && assignedRegion.area == area) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInvasion);
            Messenger.RemoveListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, character.OnOtherCharacterDied);
            Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, character.OnCharacterEndedState);
            SetAssignedRegion(null);
        }
    }
    private void PerTickInvasion() {
        if (character.isDead) {
            return;
        }
        if (!character.IsInOwnParty() || character.ownParty.icon.isTravelling || character.doNotDisturb > 0) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if (character.stateComponent.currentState != null /*|| character.stateComponent.stateToDo != null*/ || character.marker == null) {
            return;
        }
        GoToWorkArea();
    }
    private void GoToWorkArea() {
        LocationStructure structure = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
        LocationGridTile tile = structure.GetRandomTile();
        character.marker.GoTo(tile);
    }
    private void OnSucceedInvadeArea(Area area) {
        if (character.stateComponent.currentState != null) {
            character.stateComponent.currentState.OnExitThisState();
            //This call is doubled so that it will also exit the previous major state if there's any
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
            }
        }
        //else if (character.stateComponent.currentState != null) {
        //    character.stateComponent.SetStateToDo(null);
        //}

        if (character.currentParty.icon.isTravelling) {
            character.marker.StopMovement();
        }
        character.StopCurrentActionNode(false);

        character.specificLocation.RemoveCharacterFromLocation(character);
        //character.marker.ClearAvoidInRange(false);
        //character.marker.ClearHostilesInRange(false);
        //character.marker.ClearPOIsInVisionRange();
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(character);
        character.ClearAllAwareness();
        character.CancelAllJobs();
        character.traitContainer.RemoveAllNonPersistentTraits(character);
        character.ResetToFullHP();
        if (character.isDead) {
            character.SetIsDead(false);
            character.SetPOIState(POI_STATE.ACTIVE);
            if (character.ownParty == null) {
                character.CreateOwnParty();
                character.ownParty.CreateIcon();
            }
            character.traitContainer.RemoveTrait(character, "Dead");
        }
        character.DestroyMarker();
        SchedulingManager.Instance.ClearAllSchedulesBy(this.character);
    }
    public void SetAssignedRegion(Region region) {
        assignedRegion = region;
        UpdateBusyReason();
        Messenger.Broadcast(Signals.MINION_CHANGED_ASSIGNED_REGION, this, assignedRegion);
    }
    #endregion

    #region Traits
    /// <summary>
    /// Add trait function for minions. Added handling for when a minion gains a trait while outside of an area map. All traits are stored and will be added once the minion is placed at an area map.
    /// </summary>
    public bool AddTrait(string traitName, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
        if (InteriorMapManager.Instance.isAnAreaMapShowing) {
            return character.traitContainer.AddTrait(character, traitName, characterResponsible, gainedFromDoing);
        } else {
            traitsToAdd.Add(traitName);
            return true;
        }
    }
    private void AddPendingTraits() {
        for (int i = 0; i < traitsToAdd.Count; i++) {
            character.traitContainer.AddTrait(character, traitsToAdd[i]);
        }
        traitsToAdd.Clear();
    }
    #endregion

    #region Utilities
    private void UpdateBusyReason() {
        if (assignedRegion != null) {
            if (assignedRegion.mainLandmark.specificLandmarkType.IsPlayerLandmark() || assignedRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
                //the region that this minion is assigned to is a player landmark
                Log log = new Log(GameManager.Instance.Today(), "Character", "Minion", "busy_" + assignedRegion.mainLandmark.specificLandmarkType.ToString());
                log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(assignedRegion, assignedRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                SetBusyReason(log);
            } else {
                //the region that this minion is assigned to is a normal landmark
                if (assignedRegion.activeEvent != null && assignedRegion.eventData.interferingCharacter == this.character) {
                    //this minion is interferring with an event 
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Minion", "busy_interfere");
                    log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, assignedRegion.activeEvent.name, LOG_IDENTIFIER.STRING_1);
                    log.AddToFillers(assignedRegion, assignedRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                    SetBusyReason(log);
                } else {
                    //this minion is invading
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Minion", "busy_invade");
                    log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(assignedRegion, assignedRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                    SetBusyReason(log);
                }
            }
        } else {
            SetBusyReason(null);
        }
    }
    private void SetBusyReason(Log log) {
        busyReasonLog = log;
    }
    public void AdjustSpellExtractionCount(int amount) {
        spellExtractionCount += amount;
    }
    #endregion
}

[System.Serializable]
public struct UnsummonedMinionData {
    public string minionName;
    public string className;
    public COMBAT_ABILITY combatAbility;
    public List<INTERVENTION_ABILITY> interventionAbilitiesToResearch;

    public override bool Equals(object obj) {
        if (obj is UnsummonedMinionData) {
            return Equals((UnsummonedMinionData)obj);
        }
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    private bool Equals(UnsummonedMinionData data) {
        return this.minionName == data.minionName && this.className == data.className && this.combatAbility == data.combatAbility && this.interventionAbilitiesToResearch.Equals(data.interventionAbilitiesToResearch);
    }
}