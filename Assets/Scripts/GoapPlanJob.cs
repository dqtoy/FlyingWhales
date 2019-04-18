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

    //forced interactions per effect
    public Dictionary<GoapEffect, INTERACTION_TYPE> forcedActions { get; private set; }


    public GoapPlanJob(string name, GoapEffect targetEffect) : base(name) {
        this.targetEffect = targetEffect;
        this.targetPOI = targetEffect.targetPOI;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType, object[] otherData) : base(name) {
        //this.targetEffect = targetEffect;
        //this.targetPOI = targetEffect.targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = otherData;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI) : base(name) {
        //this.targetEffect = targetEffect;
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        //this.otherData = otherData;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
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
                } else {
                    character.SetCurrentAction(null);
                    character.DropPlan(assignedPlan);
                }
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
        }else if (this.targetPOI is SpecialToken) {
            SpecialToken target = this.targetPOI as SpecialToken;
            target.AddJobTargettingThis(this);
        }
    }
    public override bool OnRemoveJobFromQueue() {
        if (this.targetPOI is Character) {
            Character target = this.targetPOI as Character;
            return target.RemoveJobTargettingThisCharacter(this);
        }else if (this.targetPOI is SpecialToken) {
            SpecialToken target = this.targetPOI as SpecialToken;
            return target.RemoveJobTargettingThis(this);
        }
        return false;
    }
    protected override bool CanTakeJob(Character character) {
        if(targetPOI == null) {
            Debug.Log("null target");
            return true;
        }
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

    #region Forced Actions
    /// <summary>
    /// Add a forced action to this job.
    /// Forced actions are used for plan generation, when a certain action in the plan has a precondition that is in the dictionary,
    /// the plan generation must use the specified action type here.
    /// </summary>
    /// <param name="precondition"></param>
    /// <param name="forcedAction"></param>
    public void AddForcedInteraction(GoapEffect precondition, INTERACTION_TYPE forcedAction) {
        if (!forcedActions.ContainsKey(precondition)) {
            forcedActions.Add(precondition, forcedAction);
        }
    }
    #endregion
}

public class ForcedActionsComparer : IEqualityComparer<GoapEffect> {

    public bool Equals(GoapEffect x, GoapEffect y) {
        if (x.conditionType == y.conditionType) {
            if (string.IsNullOrEmpty(x.conditionString()) || string.IsNullOrEmpty(y.conditionString())) {
                return true;
            } else {
                return x.conditionString() == y.conditionString();
            }
        }
        return false;
    }

    public int GetHashCode(GoapEffect obj) {
        return obj.GetHashCode();
    }
}
