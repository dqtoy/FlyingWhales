﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Actionables;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class Minion {
    public const int MAX_INTERVENTION_ABILITY_SLOT = 5;

    public Character character { get; private set; }
    public int exp { get; private set; }
    public int indexDefaultSort { get; private set; }
    public CombatAbility combatAbility { get; private set; }
    public List<string> traitsToAdd { get; private set; }
    public Region assignedRegion { get; private set; } //the landmark that this minion is currently invading. NOTE: This is set on both settlement and non settlement landmarks
    public DeadlySin deadlySin => CharacterManager.Instance.GetDeadlySin(_assignedDeadlySinName);
    public bool isAssigned => assignedRegion != null; //true if minion is already assigned somewhere else, maybe in construction or research spells
    public int spellExtractionCount { get; private set; } //the number of times a spell was extracted from this minion.

    private string _assignedDeadlySinName;

    public Log busyReasonLog { get; private set; } //The reason that this minion is busy

    public Minion(Character character, bool keepData) {
        this.character = character;
        this.exp = 0;
        traitsToAdd = new List<string>();
        character.SetMinion(this);
        SetLevel(1);
        SetAssignedDeadlySinName(character.characterClass.className);
        character.ownParty.icon.SetVisualState(true);
        if (!keepData) {
            character.SetName(RandomNameGenerator.GenerateMinionName());
        }
        // RemoveInvalidPlayerActions();
        character.needsComponent.SetFullnessForcedTick(0);
        character.needsComponent.SetTirednessForcedTick(0);
        character.behaviourComponent.AddBehaviourComponent(typeof(DefaultMinion));
    }
    public Minion(SaveDataMinion data) {
        this.character = CharacterManager.Instance.GetCharacterByID(data.characterID);
        this.exp = data.exp;
        traitsToAdd = data.traitsToAdd;
        SetIndexDefaultSort(data.indexDefaultSort);
        character.SetMinion(this);
        character.ownParty.icon.SetVisualState(true);
        SetAssignedDeadlySinName(character.characterClass.className);
        spellExtractionCount = data.spellExtractionCount;
        // RemoveInvalidPlayerActions();
    }
    public void SetAssignedDeadlySinName(string name) {
        _assignedDeadlySinName = name;
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
    public void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, 
        Log _deathLog = null, LogFiller[] deathLogFillers = null) {
        if (!character.isDead) {
            Region deathLocation = character.currentRegion;
            LocationStructure deathStructure = character.currentStructure;
            LocationGridTile deathTile = character.gridTileLocation;

            character.SetIsDead(true);
            character.SetPOIState(POI_STATE.INACTIVE);

            if (character.currentRegion == null) {
                throw new Exception(
                    $"Specific location of {character.name} is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.ExitCurrentState();
            }
            if (character.currentSettlement != null && character.isHoldingItem) {
                character.DropAllItems(deathTile);
            }

            //clear traits that need to be removed
            character.traitsNeededToBeRemoved.Clear();

            Character carrier = character.isBeingCarriedBy;
            if (carrier != null) {
                carrier.UncarryPOI(character);
            }
            character.ownParty.PartyDeath();
            character.currentRegion?.RemoveCharacterFromLocation(character);
            character.SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            character.SetCurrentStructureLocation(deathStructure, false);

            // character.role?.OnDeath(character);
            character.traitContainer.RemoveAllTraitsByName(character, "Criminal"); //remove all criminal type traits

            for (int i = 0; i < character.traitContainer.allTraits.Count; i++) {
                if (character.traitContainer.allTraits[i].OnDeath(character)) {
                    i--;
                }
            }

            character.traitContainer.RemoveAllNonPersistentTraits(character);
            character.marker?.OnDeath(deathTile);
            
            // Dead dead = new Dead();
            // dead.AddCharacterResponsibleForTrait(responsibleCharacter);
            // character.traitContainer.AddTrait(character, dead, gainedFromDoing: deathFromAction);
            // PlayerManager.Instance.player.RemoveMinion(this);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, character);

            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character as IPointOfInterest, "target is already dead");
            character.CancelAllJobs();
            // StopInvasionProtocol(PlayerManager.Instance.player.currentSettlementBeingInvaded);

            Log deathLog;
            if (_deathLog == null) {
                deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}");
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
                character.logComponent.AddHistory(deathLog);
                PlayerManager.Instance.player.ShowNotificationFrom(character, deathLog);
            } else {
                deathLog = _deathLog;
            }
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(),
                $"Minion Died: {UtilityScripts.Utilities.LogReplacer(deathLog)}", null);
            Unsummon();
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
    public void StartInvasionProtocol(Settlement settlement) {
        //TODO:
        // AddPendingTraits();
        // Messenger.AddListener(Signals.TICK_STARTED, PerTickInvasion);
        // Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        // Messenger.AddListener<Settlement>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
        // Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, character.OnOtherCharacterDied);
        // Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, character.OnCharacterEndedState);
        // SetAssignedRegion(settlement.region);
    }
    public void StopInvasionProtocol(Settlement settlement) {
        //TODO:
        // if(settlement != null && assignedRegion != null && assignedRegion.settlement == settlement) {
        //     Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInvasion);
        //     Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        //     Messenger.RemoveListener<Settlement>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
        //     Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, character.OnOtherCharacterDied);
        //     Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, character.OnCharacterEndedState);
        //     SetAssignedRegion(null);
        // }
    }
    private void PerTickInvasion() {
        if (character.isDead) {
            return;
        }
        if (!character.isInCombat) {
            character.HPRecovery(0.0025f);
            if (character.IsInOwnParty() && character.marker && character.canPerform && !character.ownParty.icon.isTravelling) {
                GoToWorkArea();
            }
        }
        //if (!character.IsInOwnParty() || character.ownParty.icon.isTravelling || character.doNotDisturb) {
        //    return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
        //}
        //if (character.stateComponent.currentState != null /*|| character.stateComponent.stateToDo != null*/ || character.!marker) {
        //    return;
        //}
        //GoToWorkArea();
    }
    private void OnTickEnded() {
        character.stateComponent.OnTickEnded();
        character.EndTickPerformJobs();
    }
    private void OnTickStarted() {
        if (character.CanPlanGoap()) {
            character.PerStartTickActionPlanning();
        }
    }
    private void GoToWorkArea() {
        LocationStructure structure = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
        LocationGridTile tile = structure.GetRandomTile();
        character.marker.GoTo(tile);
    }
    public void SetAssignedRegion(Region region) {
        assignedRegion = region;
        UpdateBusyReason();
        Messenger.Broadcast(Signals.MINION_CHANGED_ASSIGNED_REGION, this, assignedRegion);
    }
    #endregion

    #region Traits
    /// <summary>
    /// Add trait function for minions. Added handling for when a minion gains a trait while outside of an settlement map. All traits are stored and will be added once the minion is placed at an settlement map.
    /// </summary>
    public bool AddTrait(string traitName, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
        if (InnerMapManager.Instance.isAnInnerMapShowing) {
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
                Log log = new Log(GameManager.Instance.Today(), "Character", "Minion",
                    $"busy_{assignedRegion.mainLandmark.specificLandmarkType}");
                log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(assignedRegion, assignedRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                SetBusyReason(log);
            } else {
                //this minion is invading
                Log log = new Log(GameManager.Instance.Today(), "Character", "Minion", "busy_invade");
                log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(assignedRegion, assignedRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                SetBusyReason(log);
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

    #region Summoning
    public void Summon(Inner_Maps.Location_Structures.ThePortal portalStructure) {
        character.CreateMarker();

        int minX = portalStructure.tiles.Min(t => t.localPlace.x);
        int maxX = portalStructure.tiles.Max(t => t.localPlace.x);
        int minY = portalStructure.tiles.Min(t => t.localPlace.y);
        int maxY = portalStructure.tiles.Max(t => t.localPlace.y);

        int differenceX = (maxX - minX) + 1;
        int differenceY = (maxY - minY) + 1;

        int centerX = minX + (differenceX / 2);
        int centerY = minY + (differenceY / 2);

        LocationGridTile centerTile = portalStructure.location.innerMap.map[centerX, centerY];
        // Vector3 pos = centerTile.worldLocation;

        character.marker.InitialPlaceMarkerAt(centerTile);
        character.SetIsDead(false);

        Vector2Int tileToGoToCoords = new Vector2Int(character.gridTileLocation.localPlace.x, character.gridTileLocation.localPlace.y - 3);
        LocationGridTile tileToGoTo = portalStructure.location.innerMap.map[tileToGoToCoords.x, tileToGoToCoords.y];
        character.marker.GoTo(tileToGoTo);
        
        PlayerManager.Instance.player.AdjustMana(-EditableValuesManager.Instance.summonMinionManaCost);
        
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
    }
    private void Unsummon() {
        character.SetHP(0);
        Messenger.AddListener(Signals.TICK_ENDED, UnsummonedHPRecovery);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
    }
    private void UnsummonedHPRecovery() {
        this.character.AdjustHP((int)(character.maxHP * 0.02f), ELEMENTAL_TYPE.Normal);
        if (character.currentHP >= character.maxHP) {
            //minion can be summoned again
            Messenger.RemoveListener(Signals.TICK_ENDED, UnsummonedHPRecovery);
        }
    }
    public void OnSeize() {
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
    }
    public void OnUnseize() {
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
    }
    #endregion

    #region Player Action Target
    private void RemoveInvalidPlayerActions() {
        List<PlayerAction> currentActions = new List<PlayerAction>(character.actions); 
        for (int i = 0; i < currentActions.Count; i++) {
            PlayerAction action = currentActions[i];
            if (action.actionName != "Seize") {
                character.RemovePlayerAction(action);    
            }
        }
    }
    #endregion
    
    #region Jobs
    public void NoPathToDoJob(JobQueueItem job) {
        if (job.jobType == JOB_TYPE.ROAM_AROUND_CORRUPTION) {
            character.jobComponent.TriggerRoamAroundTile();
        } else if (job.jobType == JOB_TYPE.ROAM_AROUND_PORTAL) {
            character.jobComponent.TriggerRoamAroundTile();
        } else if (job.jobType == JOB_TYPE.RETURN_PORTAL) {
            character.jobComponent.TriggerRoamAroundTile();
        } else if (job.jobType == JOB_TYPE.ROAM_AROUND_TILE) {
            character.jobComponent.TriggerMonsterStand();
        }
    }
    #endregion
}

[System.Serializable]
public struct UnsummonedMinionData {
    public string minionName;
    public string className;
    public COMBAT_ABILITY combatAbility;
    public List<SPELL_TYPE> interventionAbilitiesToResearch;

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