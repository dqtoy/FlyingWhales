using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Minion {
    public const int MAX_INTERVENTION_ABILITY_SLOT = 5;

    public Character character { get; private set; }
    public int exp { get; private set; }
    public int indexDefaultSort { get; private set; }
    public int unlockedInterventionSlots { get; private set; }
    //public bool isEnabled { get; private set; }
    public PlayerJobAction[] interventionAbilities { get; private set; }
    public CombatAbility combatAbility { get; private set; }

    public List<string> traitsToAdd { get; private set; }

    public Minion(Character character, bool keepData) {
        this.character = character;
        this.exp = 0;
        this.interventionAbilities = new PlayerJobAction[MAX_INTERVENTION_ABILITY_SLOT];
        traitsToAdd = new List<string>();
        SetUnlockedInterventionSlots(0);
        character.SetMinion(this);
        SetLevel(1);
        //character.characterToken.SetObtainedState(true);
        character.ownParty.icon.SetVisualState(true);

        if (!keepData) {
            character.SetName(RandomNameGenerator.Instance.GenerateMinionName());
        }
    }
    public Minion(SaveDataMinion data) {
        this.character = CharacterManager.Instance.GetCharacterByID(data.characterID);
        this.exp = data.exp;
        traitsToAdd = data.traitsToAdd;
        this.interventionAbilities = new PlayerJobAction[MAX_INTERVENTION_ABILITY_SLOT];
        SetIndexDefaultSort(data.indexDefaultSort);
        SetUnlockedInterventionSlots(data.unlockedInterventionSlots);
        character.SetMinion(this);
        character.ownParty.icon.SetVisualState(true);
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
    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        character.SetPlayerCharacterItem(item);
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
    public void Death(string cause = "normal", GoapAction deathFromAction = null, Character responsibleCharacter = null) {
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
            } else if (character.stateComponent.stateToDo != null) {
                character.stateComponent.SetStateToDo(null);
            }
            character.CancelAllJobsTargettingThisCharacter("target is already dead", false);
            Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, character, "target is already dead");
            if (character.currentAction != null && !character.currentAction.cannotCancelAction) {
                character.currentAction.StopAction();
            }
            if (character.ownParty.specificLocation != null && character.isHoldingItem) {
                character.DropAllTokens(character.ownParty.specificLocation, character.currentStructure, deathTile, true);
            }

            //clear traits that need to be removed
            character.traitsNeededToBeRemoved.Clear();

            if (!character.IsInOwnParty()) {
                character.currentParty.RemoveCharacter(character);
            }
            character.ownParty.PartyDeath();

            if (character.role != null) {
                character.role.OnDeath(character);
            }

            character.RemoveAllTraitsByType(TRAIT_TYPE.CRIMINAL); //remove all criminal type traits

            for (int i = 0; i < character.normalTraits.Count; i++) {
                character.normalTraits[i].OnDeath(character);
            }

            character.RemoveAllNonPersistentTraits();

            character.marker.OnDeath(deathTile);
            character.SetNumWaitingForGoapThread(0); //for raise dead
            Dead dead = new Dead();
            dead.SetCharacterResponsibleForTrait(responsibleCharacter);
            character.AddTrait(dead, gainedFromDoing: deathFromAction);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, character);

            character.CancelAllJobsAndPlans();

            Debug.Log(GameManager.Instance.TodayLogString() + character.name + " died of " + cause);
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            character.AddHistory(log);
            character.specificLocation.AddHistory(log);
        }
    }

    #region Intervention Abilities
    public void SetUnlockedInterventionSlots(int amount) {
        unlockedInterventionSlots = amount;
        unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    }
    public void AdjustUnlockedInterventionSlots(int amount) {
        unlockedInterventionSlots += amount;
        unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    }
    public void AddInterventionAbility(PlayerJobAction ability, bool showNewAbilityUI = false) {
        int currentInterventionAbilityCount = GetCurrentInterventionAbilityCount();
        if(currentInterventionAbilityCount < unlockedInterventionSlots) {
            for (int i = 0; i < interventionAbilities.Length; i++) {
                if (interventionAbilities[i] == null) {
                    interventionAbilities[i] = ability;
                    ability.SetMinion(this);
                    Messenger.Broadcast(Signals.MINION_LEARNED_INTERVENE_ABILITY, this, ability);
                    if (showNewAbilityUI) {
                        PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(this, ability);
                    }
                    break;
                }
            }
        } else {
            //Broadcast intervention ability is full, must open UI whether player wants to replace ability or discard it
            PlayerUI.Instance.replaceUI.ShowReplaceUI(GeAllInterventionAbilities(), ability, ReplaceAbility, RejectAbility);
        }
    }
    private void ReplaceAbility(object objToReplace, object objToAdd) {
        PlayerJobAction replace = objToReplace as PlayerJobAction;
        PlayerJobAction add = objToAdd as PlayerJobAction;
        for (int i = 0; i < interventionAbilities.Length; i++) {
            if (interventionAbilities[i] == replace) {
                interventionAbilities[i] = add;
                add.SetMinion(this);
                replace.SetMinion(null);
                Messenger.Broadcast(Signals.MINION_LEARNED_INTERVENE_ABILITY, this, add);
                break;
            }
        }
    }
    private void RejectAbility(object rejectedObj) { }
    public void AddInterventionAbility(INTERVENTION_ABILITY ability, bool showNewAbilityUI = false) {
        AddInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(ability), showNewAbilityUI);
    }
    public int GetCurrentInterventionAbilityCount() {
        int count = 0;
        for (int i = 0; i < interventionAbilities.Length; i++) {
            if (interventionAbilities[i] != null) {
                count++;
            }
        }
        return count;
    }
    public List<PlayerJobAction> GeAllInterventionAbilities() {
        List<PlayerJobAction> all = new List<PlayerJobAction>();
        for (int i = 0; i < interventionAbilities.Length; i++) {
            if (interventionAbilities[i] != null) {
                all.Add(interventionAbilities[i]);
            }
        }
        return all;
    }
    #endregion

    #region Combat Ability
    public void SetCombatAbility(CombatAbility combatAbility, bool showNewAbilityUI = false) {
        if (this.combatAbility == null) {
            this.combatAbility = combatAbility;
            if (combatAbility != null && showNewAbilityUI) {
                PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(this, combatAbility);
            }
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
    #endregion

    #region Invasion
    public void StartInvasionProtocol() {
        AddPendingTraits();
        Messenger.AddListener(Signals.TICK_STARTED, PerTickInvasion);
        Messenger.AddListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
    }
    public void StopInvasionProtocol() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInvasion);
        Messenger.RemoveListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
    }
    private void PerTickInvasion() {
        if (character.isDead) {
            return;
        }
        if (!character.IsInOwnParty() || character.ownParty.icon.isTravelling || character.doNotDisturb > 0 || character.isWaitingForInteraction > 0) {
            return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        }
        if (character.stateComponent.currentState != null || character.stateComponent.stateToDo != null || character.marker == null) {
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
        } else if (character.stateComponent.currentState != null) {
            character.stateComponent.SetStateToDo(null);
        }

        if (character.currentParty.icon.isTravelling) {
            character.marker.StopMovement();
        }
        character.AdjustIsWaitingForInteraction(1);
        character.StopCurrentAction(false);
        character.AdjustIsWaitingForInteraction(-1);

        character.specificLocation.RemoveCharacterFromLocation(character);
        //character.marker.ClearAvoidInRange(false);
        //character.marker.ClearHostilesInRange(false);
        //character.marker.ClearPOIsInVisionRange();
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(character);
        character.ClearAllAwareness();
        character.CancelAllJobsAndPlans();
        character.RemoveAllNonPersistentTraits();
        character.ResetToFullHP();
        if (character.isDead) {
            character.SetIsDead(false);
            character.SetPOIState(POI_STATE.ACTIVE);
            if (character.ownParty == null) {
                character.CreateOwnParty();
                character.ownParty.CreateIcon();
            }
            character.RemoveTrait("Dead");
        }
        character.DestroyMarker();
        SchedulingManager.Instance.ClearAllSchedulesBy(this.character);
    }
    #endregion

    #region Traits
    /// <summary>
    /// Add trait function for minions. Added handling for when a minion gains a trait while outside of an area map. All traits are stored and will be added once the minion is placed at an area map.
    /// </summary>
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (InteriorMapManager.Instance.isAnAreaMapShowing) {
            return character.AddTrait(traitName, characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        } else {
            traitsToAdd.Add(traitName);
            return true;
        }
    }
    private void AddPendingTraits() {
        for (int i = 0; i < traitsToAdd.Count; i++) {
            character.AddTrait(traitsToAdd[i]);
        }
        traitsToAdd.Clear();
    }
    #endregion
}
