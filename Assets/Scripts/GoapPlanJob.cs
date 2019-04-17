using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlanJob : JobQueueItem {

    public GoapEffect targetEffect { get; protected set; }
    public GoapPlan assignedPlan { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }

    //interaction type version
    public INTERACTION_TYPE targetInteractionType { get; protected set; } //Only used if the plan to be created uses interaction type
    public object[] otherData { get; protected set; } //Only used if the plan to be created uses interaction type

    public GoapPlanJob(string name, GoapEffect targetEffect) : base(name) {
        this.targetEffect = targetEffect;
        this.targetPOI = targetEffect.targetPOI;
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType, object[] otherData) : base(name) {
        //this.targetEffect = targetEffect;
        //this.targetPOI = targetEffect.targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = otherData;
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI) : base(name) {
        //this.targetEffect = targetEffect;
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        //this.otherData = otherData;
    }

    #region Overrides 
    public override void UnassignJob() {
        base.UnassignJob();
        if (assignedPlan != null && assignedCharacter != null) {
            Character character = assignedCharacter;
            character.AdjustIsWaitingForInteraction(1);
            if (character.currentAction != null && character.currentAction.parentPlan == assignedPlan) {
                if(character.currentParty.icon.isTravelling && character.currentParty.icon.travelLine == null) {
                    character.marker.StopMovementOnly();
                }
                if (character.currentAction.isPerformingActualAction && !character.currentAction.isDone) {
                    character.currentAction.currentState.EndPerTickEffect();
                }
                character.SetCurrentAction(null);
                character.DropPlan(assignedPlan);
            } else {
                character.DropPlan(assignedPlan);
            }
            character.AdjustIsWaitingForInteraction(-1);
            SetAssignedCharacter(null);
            SetAssignedPlan(null);
        }
    }
    public override void OnAddJobToQueue() {
        if (this.targetPOI is Character) {
            Character target = this.targetPOI as Character;
            target.AddJobTargettingThisCharacter(this);
        }
    }
    public override bool OnRemoveJobFromQueue() {
        if (this.targetPOI is Character) {
            Character target = this.targetPOI as Character;
            return target.RemoveJobTargettingThisCharacter(this);
        }
        return false;
    }
    protected override bool CanTakeJob(Character character) {
        if(targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character target = targetPOI as Character;
            if(target.IsInOwnParty() && !target.isDead) {
                return true;
            }
            return false;
        }
        return base.CanTakeJob(character);
    }
    #endregion

    public void SetAssignedPlan(GoapPlan plan) {
        if (assignedPlan != null) {
            assignedPlan.SetJob(null);
        }
        if (plan != null) {
            plan.SetJob(this);
        }
        assignedPlan = plan;
    }
}
