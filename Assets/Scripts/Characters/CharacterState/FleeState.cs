using System.Collections;
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
        if (!targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER)) {
            stateComponent.character.marker.AddTerrifyingCharacter(targetCharacter);
        }
        if(stateComponent.character.role.roleType == CHARACTER_ROLE.LEADER || stateComponent.character.role.roleType == CHARACTER_ROLE.NOBLE || stateComponent.character.role.roleType == CHARACTER_ROLE.SOLDIER) {
            if (targetCharacter.GetNumOfJobsTargettingThisCharacter("Assault") < 3) {
                stateComponent.character.CreateAssaultJobs(targetCharacter, false, 3);
            }
        } else {
            GoapPlanJob job = new GoapPlanJob("Report Hostile", INTERACTION_TYPE.REPORT_HOSTILE, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.REPORT_HOSTILE, new object[] { targetCharacter }}
                });
            job.SetCannotOverrideJob(true);
            job.SetCancelOnFail(true);
            stateComponent.character.jobQueue.AddJobInQueue(job, true, false);
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
            if (stateComponent.character.marker.GetNearestValidHostile() == null) {
                //can end flee
                OnExitThisState();
            } else {
                //redetermine flee path
                stateComponent.character.marker.RedetermineFlee();
            }
        }
    }
}
