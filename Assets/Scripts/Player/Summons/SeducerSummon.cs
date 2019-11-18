﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeducerSummon : Summon {

    public int seduceChance;
    private List<Character> doneCharacters; //list of characters that the succubus has invited to make love with, regardless of success
    public override int ignoreHostility {
        get {
            if (currentActionNode != null && currentActionNode.goapType.IsHostileAction()) {
                return 0; //allow hostility checking
            } else if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT || stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED)) {
                return 0; //if in combat or berserked state allow hostility checking
            }
            return 1; //default is that succubi won't be seen as hostiles.
        }
    }

    private bool hasSucceeded;

    public SeducerSummon(SUMMON_TYPE type, GENDER gender) : base(type, CharacterRole.MINION, RACE.DEMON, gender) {
        seduceChance = 25;
        doneCharacters = new List<Character>();
        AddInteractionType(INTERACTION_TYPE.INVITE);
        AddInteractionType(INTERACTION_TYPE.MAKE_LOVE);
    }
    public SeducerSummon(SaveDataCharacter data) : base(data) {
        seduceChance = 25;
        doneCharacters = new List<Character>();
    }


    #region Overrides
    //public override void UnsubscribeSignals() {
    //    base.UnsubscribeSignals();
    //    Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
    //}
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        hasSucceeded = false;
        //Messenger.AddListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration);
        AdjustIgnoreHostilities(1);
    }
    public override List<ActualGoapNode> ThisCharacterSaw(IPointOfInterest target) {
        if (traitContainer.GetNormalTrait("Unconscious", "Resting") != null) {
            return null;
        }
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnSeePOI(target, this);
        }
        return null;
    }
    protected override void PerStartTickActionPlanning() {
        if (_hasAlreadyAskedForPlan) {
            return;
        }
        SetHasAlreadyAskedForPlan(true);
        if (hasSucceeded) {
            //disappear
            Disappear();
        } else if (!jobQueue.HasJob(JOB_TYPE.SEDUCE)){
            //pick a random character that is sexually compatible with this character, to seduce. Exclude characters that this succubus has already invited.
            List<Character> choices = specificLocation.charactersAtLocation.Where(x => x.faction != this.faction
            && !doneCharacters.Contains(x)
            && RelationshipManager.Instance.IsSexuallyCompatibleOneSided(x, this)
            && !x.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)).ToList();
            List<TileObject> validBeds = specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.INN).GetTileObjectsOfType(TILE_OBJECT_TYPE.BED);
            if (choices.Count > 0 && validBeds.Count > 0) {
                Character chosenCharacter = choices[Random.Range(0, choices.Count)];
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SEDUCE, INTERACTION_TYPE.INVITE, chosenCharacter);
                job.SetCannotOverrideJob(true);
                job.SetCannotCancelJob(true);
                jobQueue.AddJobInQueue(job);
            } else {
                //just enter berserked mode.
                stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
                SetHasAlreadyAskedForPlan(false);
            }
        }
        
    }
    public override void OnAfterActionStateSet(string stateName, ActualGoapNode node) {
        if (action.actor == this && action.goapType == INTERACTION_TYPE.INVITE) {
            doneCharacters.Add(action.poiTarget as Character);
        } else if (action.actor == this && action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
            if (state.status == InteractionManager.Goap_State_Success) {
                hasSucceeded = true;
            }
        }
    }
    public override void LevelUp() {
        base.LevelUp();
        seduceChance += 25;
    }
    public override bool CanBeInstructedByPlayer() {
        bool canBeInstructed = base.CanBeInstructedByPlayer();
        if (canBeInstructed) {
            if (ignoreHostility > 0) {
                canBeInstructed = false;
            }
        }
        return canBeInstructed;
    }
    #endregion

    private void Disappear() {
        LocationGridTile disappearTile = gridTileLocation;
        if (stateComponent.currentState != null) {
            stateComponent.currentState.OnExitThisState();
            //This call is doubled so that it will also exit the previous major state if there's any
            if (stateComponent.currentState != null) {
                stateComponent.currentState.OnExitThisState();
            }
        }
        //else if (stateComponent.currentState != null) {
        //    stateComponent.SetStateToDo(null);
        //}

        if (currentParty.icon.isTravelling) {
            marker.StopMovement();
        }
        //AdjustIsWaitingForInteraction(1);
        //StopCurrentAction(false);
        //AdjustIsWaitingForInteraction(-1);
        specificLocation.RemoveCharacterFromLocation(this);
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(this);
        ownParty.SetSpecificLocation(PlayerManager.Instance.player.playerArea);
        ClearAllAwareness();
        CancelAllJobsAndPlans();
        traitContainer.RemoveAllNonPersistentTraits(this);
        ResetToFullHP();
        UnsubscribeSignals();
        DestroyMarker(disappearTile);
        SchedulingManager.Instance.ClearAllSchedulesBy(this);
    }
}   

