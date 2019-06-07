﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeState : CharacterState {

    public FleeState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Flee State";
        characterState = CHARACTER_STATE.FLEE;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        //duration = 288;
        actionIconString = GoapActionStateDB.Flee_Icon;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartFleeMovement();
    }
    protected override void PerTickInState() {
        //stateComponent.character.marker.RedetermineFlee();
        //base.PerTickInState();
        //if the character is away from home and is at an edge tile, go to home location
        if (stateComponent.character.homeArea != null && stateComponent.character.homeArea != stateComponent.character.specificLocation && stateComponent.character.gridTileLocation.IsAtEdgeOfWalkableMap()) {
            OnExitThisState();
            stateComponent.character.currentParty.GoToLocation(stateComponent.character.homeArea, PATHFINDING_MODE.NORMAL, stateComponent.character.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS), null, null, null, null);
        }
        
    }
    //protected override void EndState() {
    //    base.EndState();
    //    OnExitThisState();
    //}
    public override void OnExitThisState() {
        if (stateComponent.character.marker.hasFleePath) {
            //the character still has a current flee path
            //stop his current movement
            stateComponent.character.marker.StopMovementOnly();
        }
        stateComponent.character.currentParty.icon.SetIsTravelling(false);
        stateComponent.character.marker.SetHasFleePath(false);
        if (stateComponent.character.IsHostileWith(targetCharacter)) {
            if (!targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery")) {
                stateComponent.character.marker.AddTerrifyingCharacter(targetCharacter);
            }
        }
        if (stateComponent.character.IsHostileOutsider(targetCharacter)) {
            if (stateComponent.character.role.roleType == CHARACTER_ROLE.LEADER || stateComponent.character.role.roleType == CHARACTER_ROLE.NOBLE || stateComponent.character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                int numOfJobs = 3 - targetCharacter.GetNumOfJobsTargettingThisCharacter(JOB_TYPE.ASSAULT);
                if (numOfJobs > 0) {
                    stateComponent.character.CreateAssaultJobs(targetCharacter, numOfJobs);
                }
            } else {
                if (!(targetCharacter.isDead || targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery") || targetCharacter.isAtHomeArea)) {
                    if (stateComponent.character.isAtHomeArea) {
                        if (!stateComponent.character.jobQueue.HasJobWithOtherData(JOB_TYPE.REPORT_HOSTILE, targetCharacter)) {
                            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPORT_HOSTILE, INTERACTION_TYPE.REPORT_HOSTILE, new Dictionary<INTERACTION_TYPE, object[]>() {
                                { INTERACTION_TYPE.REPORT_HOSTILE, new object[] { targetCharacter }}
                            });
                            job.SetCannotOverrideJob(true);
                            job.SetCancelOnFail(true);
                            stateComponent.character.jobQueue.AddJobInQueue(job, false);
                        }
                    }
                }
            }
        }
        base.OnExitThisState();
    }
    #endregion

    private void StartFleeMovement() {
        stateComponent.character.marker.OnStartFlee();
    }

    public void CheckForEndState() {
        if (stateComponent.character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            //if the character has a negative disabler trait, end this state
            OnExitThisState();
        } else {
            if (stateComponent.character.marker.GetNearestValidHostile() == null && stateComponent.character.marker.GetNearestValidAvoid() == null) {
                //can end flee
                OnExitThisState();
            } else {
                //redetermine flee path
                stateComponent.character.marker.RedetermineFlee();
            }
        }
    }
}
