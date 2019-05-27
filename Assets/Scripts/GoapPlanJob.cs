using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlanJob : JobQueueItem {

    public GoapEffect targetEffect { get; protected set; }
    public GoapPlan assignedPlan { get; protected set; }
    public GoapPlan targetPlan { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    public bool willImmediatelyBeDoneAfterReceivingPlan { get; protected set; }

    //interaction type version
    public INTERACTION_TYPE targetInteractionType { get; protected set; } //Only used if the plan to be created uses interaction type
    public Dictionary<INTERACTION_TYPE, object[]> otherData { get; protected set; } //Only used if the plan to be created uses interaction type

    //forced interactions per effect
    public Dictionary<GoapEffect, INTERACTION_TYPE> forcedActions { get; private set; }

    //plan constructor
    public System.Func<GoapPlan> planConstructor { get; private set; } //if this is set, the job will execute this when creating a plan instead of using the normal behaviour

    //misc
    public bool allowDeadTargets { get; private set; }

    public List<object> allOtherData { get; private set; }

    public GoapPlanJob(string name, GoapEffect targetEffect) : base(name) {
        this.targetEffect = targetEffect;
        this.targetPOI = targetEffect.targetPOI;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        allowDeadTargets = false;
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType) : base(name) {
        //this.targetEffect = targetEffect;
        //this.targetPOI = targetEffect.targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = null;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        allowDeadTargets = false;
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType, Dictionary<INTERACTION_TYPE, object[]> otherData) : base(name) {
        //this.targetEffect = targetEffect;
        //this.targetPOI = targetEffect.targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = otherData;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        allowDeadTargets = false;
        if(otherData != null) {
            allOtherData = new List<object>();
            foreach (object[] data in otherData.Values) {
                if(data != null) {
                    for (int i = 0; i < data.Length; i++) {
                        allOtherData.Add(data[i]);
                    }
                }
            }
        }
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI) : base(name) {
        //this.targetEffect = targetEffect;
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        //this.otherData = otherData;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        allowDeadTargets = false;
    }
    public GoapPlanJob(string name, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData) : base(name) {
        //this.targetEffect = targetEffect;
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = otherData;
        forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        allowDeadTargets = false;
        if (otherData != null) {
            allOtherData = new List<object>();
            foreach (object[] data in otherData.Values) {
                if (data != null) {
                    for (int i = 0; i < data.Length; i++) {
                        allOtherData.Add(data[i]);
                    }
                }
            }
        }
    }
    //public GoapPlanJob(string name, GoapPlan targetPlan) : base(name) {
    //    //this.targetEffect = targetEffect;
    //    this.targetPlan = targetPlan;
    //    this.targetPOI = targetPlan.target;
    //    forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    //}

    #region Overrides 
    public override void UnassignJob(bool shouldDoAfterEffect = true) {
        base.UnassignJob(shouldDoAfterEffect);
        if (assignedPlan != null && assignedCharacter != null) {
            Character character = assignedCharacter;
            character.AdjustIsWaitingForInteraction(1);
            if (character.currentAction != null && character.currentAction.parentPlan == assignedPlan) {
                if (character.currentParty.icon.isTravelling) {
                    if (character.currentParty.icon.travelLine == null) {
                        character.marker.StopMovementOnly();
                    } else {
                        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                    }
                }
                character.StopCurrentAction(shouldDoAfterEffect);
                if (character.currentAction != null) {
                    character.SetCurrentAction(null);
                }
                character.DropPlan(assignedPlan);
            } else {
                character.DropPlan(assignedPlan);
            }
            character.AdjustIsWaitingForInteraction(-1);
            //SetAssignedCharacter(null);
            //SetAssignedPlan(null);
        } else if (assignedCharacter != null) {
            //Has assigned character but has no plan yet, the assumption for this is that the assigned character is still processing the plan for this job
            /*Just remove the assigned character and when the plan is received from goap thread, there is a checker there that will check if the assigned character is no longer he/she,
            /that character will scrap the plan that was made*/
            SetAssignedCharacter(null);
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
            if (target.IsInOwnParty()) {
                if (!allowDeadTargets && target.isDead) {
                    return false;
                }
            } else {
                //Must not take job if the target is in another party
                return false;
            }
        }
        //if(character.HasTraitOf(TRAIT_TYPE.CRIMINAL) || character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
        //    return false;
        //}
        return base.CanTakeJob(character);
    }
    public override bool CanCharacterTakeThisJob(Character character) {
        if (_canTakeThisJob != null) {
            if (_canTakeThisJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        } else if (_canTakeThisJobWithTarget != null && targetPOI != null && targetPOI is Character) {
            if (_canTakeThisJobWithTarget(character, targetPOI as Character, this)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
    }
    public override void OnCharacterAssignedToJob(Character character) {
        base.OnCharacterAssignedToJob(character);
        
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
    private void OnArriveAtLocationStopMovement() {
        if(assignedCharacter != null) {
            assignedCharacter.OnArriveAtAreaStopMovement();
        }
    }
    public void SetWillImmediatelyBeDoneAfterReceivingPlan(bool state) {
        willImmediatelyBeDoneAfterReceivingPlan = state;
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

    #region Plan Constructor
    public void SetPlanConstructor(System.Func<GoapPlan> planConstructor) {
        this.planConstructor = planConstructor;
    }
    #endregion

    #region Misc
    public void AllowDeadTargets() {
        allowDeadTargets = true;
    }
    #endregion
}

public class ForcedActionsComparer : IEqualityComparer<GoapEffect> {

    public bool Equals(GoapEffect x, GoapEffect y) {
        bool matchKeys = false;
        if(x.conditionKey is int && y.conditionKey is int) {
            int effectInt = (int) x.conditionKey;
            int preconditionInt = (int) y.conditionKey;
            matchKeys = effectInt >= preconditionInt;
        } else {
            matchKeys = x.conditionKey == y.conditionKey;
        }
        return x.conditionType == y.conditionType && x.targetPOI == y.targetPOI && matchKeys;
        //if (x.conditionType == y.conditionType) {
        //    if (string.IsNullOrEmpty(x.conditionString()) || string.IsNullOrEmpty(y.conditionString())) {
        //        return true;
        //    } else {
        //        return x.conditionString() == y.conditionString();
        //    }
        //}
        //return false;
    }

    public int GetHashCode(GoapEffect obj) {
        return obj.GetHashCode();
    }
}
