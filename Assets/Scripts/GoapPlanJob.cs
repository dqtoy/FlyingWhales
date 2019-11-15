using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlanJob : JobQueueItem {
    public GoapEffect[] goals { get; protected set; }
    public GoapPlan assignedPlan { get; protected set; }
    //public GoapPlan targetPlan { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    //public bool willImmediatelyBeDoneAfterReceivingPlan { get; protected set; }

    //interaction type version
    public INTERACTION_TYPE targetInteractionType { get; protected set; } //Only used if the plan to be created uses interaction type

    //if INTERACTION_TYPE is NONE, it means that it is used by all
    public Dictionary<INTERACTION_TYPE, object[]> otherData { get; protected set; } //TODO: Further optimize this by moving this dictionary to the actor itself

    //forced interactions per effect
    //public Dictionary<GoapEffect, INTERACTION_TYPE> forcedActions { get; private set; }

    //plan constructor
    //public System.Func<GoapPlan> planConstructor { get; private set; } //if this is set, the job will execute this when creating a plan instead of using the normal behaviour

    //misc
    //public bool allowDeadTargets { get; private set; }

    public List<object> allOtherData { get; private set; }

    public GoapPlanJob(JOB_TYPE jobType, IPointOfInterest targetPOI, params GoapEffect[] goals) : base(jobType) {
        this.goals = goals;
        this.targetPOI = targetPOI;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
    }
    public GoapPlanJob(JOB_TYPE jobType, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData, params GoapEffect[] goals) : base(jobType) {
        this.goals = goals;
        this.targetPOI = targetPOI;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
        this.otherData = otherData;
        if (otherData != null) {
            isNotSavable = true;
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
    public GoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType) : base(jobType) {
        //this.targetEffect = targetEffect;
        //this.targetPOI = targetEffect.targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = null;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
    }
    public GoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, Dictionary<INTERACTION_TYPE, object[]> otherData) : base(jobType) {
        //this.targetEffect = targetEffect;
        //this.targetPOI = targetEffect.targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = otherData;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
        if(otherData != null) {
            isNotSavable = true;
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
    public GoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI) : base(jobType) {
        //this.targetEffect = targetEffect;
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        //this.otherData = otherData;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
    }
    public GoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData) : base(jobType) {
        //this.targetEffect = targetEffect;
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        this.otherData = otherData;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
        if (otherData != null) {
            isNotSavable = true;
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
    //public GoapPlanJob(JOB_TYPE jobType, GoapPlan targetPlan, IPointOfInterest targetPOI) : base(jobType) {
    //    this.targetPOI = targetPOI;
    //    this.targetPlan = targetPlan;
    //    forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    //    allowDeadTargets = false;
    //    isNotSavable = true;
    //}
    public GoapPlanJob(SaveDataGoapPlanJob data) : base(data) {
        //goals = data.targetEffect.Load();
        //targetInteractionType = data.targetInteractionType;
        //allowDeadTargets = data.allowDeadTargets;
        //if(data.targetPOIID != -1) {
        //    if (data.targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        targetPOI = CharacterManager.Instance.GetCharacterByID(data.targetPOIID);
        //    } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
        //        targetPOI = TokenManager.Instance.GetSpecialTokenByID(data.targetPOIID);
        //    } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //        targetPOI = InteriorMapManager.Instance.GetTileObject(data.targetPOITileObjectType, data.targetPOIID);
        //    }
        //} else {
        //    targetPOI = null;
        //}
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //for (int i = 0; i < data.forcedActionsGoapEffect.Count; i++) {
        //    GoapEffect effect = data.forcedActionsGoapEffect[i].Load();
        //    forcedActions.Add(effect, data.forcedActionsType[i]);
        //}
    }

    #region Overrides 
    public override void UnassignJob(bool shouldDoAfterEffect = true, string reason = "") {
        base.UnassignJob(shouldDoAfterEffect);
        if (assignedPlan != null && assignedCharacter != null) {
            Character character = assignedCharacter;
            character.AdjustIsWaitingForInteraction(1);
            if (character.currentActionNode != null && character.currentActionNode.parentPlan == assignedPlan) {
                if (character.currentParty.icon.isTravelling) {
                    if (character.currentParty.icon.travelLine == null) {
                        character.marker.StopMovement();
                    } else {
                        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                    }
                }
                character.StopCurrentAction(shouldDoAfterEffect, reason);
                if (character.currentActionNode != null) {
                    character.SetCurrentActionNode(null);
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
        if(targetPOI != null) {
            targetPOI.AddJobTargettingThis(this);
        }
    }
    public override bool OnRemoveJobFromQueue() {
        if (!jobQueueParent.isAreaOrQuestJobQueue && id == jobQueueParent.character.sleepScheduleJobID && (assignedCharacter == null && assignedPlan == null)) { //|| jobQueueParent.character.currentSleepTicks == CharacterManager.Instance.defaultSleepTicks
            //If a character's scheduled sleep job is removed from queue before even doing it, consider it as cancelled 
            jobQueueParent.character.SetHasCancelledSleepSchedule(true);
        }
        if (targetPOI != null) {
            return targetPOI.RemoveJobTargettingThis(this);
        }
        return false;
    }
    protected override bool CanTakeJob(Character character) {
        if(targetPOI == null) {
            //Debug.Log(jobType.ToString() + " has null target");
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
        //All jobs that are personal will bypass _canTakeThisJob/_canTakeThisJobWithTarget function checkers if the character parameter is the owner of the job queue
        if(character == jobQueueParent.character) {
            return CanTakeJob(character);
        }
        if (canTakeThisJob != null) {
            if (canTakeThisJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        } else if (canTakeThisJobWithTarget != null && targetPOI != null && targetPOI is Character) {
            if (canTakeThisJobWithTarget(character, targetPOI as Character, this)) {
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
    //public void SetWillImmediatelyBeDoneAfterReceivingPlan(bool state) {
    //    willImmediatelyBeDoneAfterReceivingPlan = state;
    //}

    #region Forced Actions
    /// <summary>
    /// Add a forced action to this job.
    /// Forced actions are used for plan generation, when a certain action in the plan has a precondition that is in the dictionary,
    /// the plan generation must use the specified action type here.
    /// </summary>
    /// <param name="precondition"></param>
    /// <param name="forcedAction"></param>
    //public void AddForcedInteraction(GoapEffect precondition, INTERACTION_TYPE forcedAction) {
    //    if (!forcedActions.ContainsKey(precondition)) {
    //        forcedActions.Add(precondition, forcedAction);
    //    }
    //}
    //public void ClearForcedActions() {
    //    forcedActions.Clear();
    //}
    #endregion

    #region Plan Constructor
    //public void SetPlanConstructor(System.Func<GoapPlan> planConstructor) {
    //    this.planConstructor = planConstructor;
    //}
    #endregion

    #region Misc
    public void AllowDeadTargets() {
        allowDeadTargets = true;
    }
    /// <summary>
    /// Helper function to get what this job is trying to do.
    /// eg: Specify specific trait when it is Remove Trait job, specify specific item when it is Obtain Item job.
    /// </summary>
    /// <returns>string value to represent what the job detail is (eg. Remove Trait Unconscious)</returns>
    public string GetJobDetailString() {
        switch (jobType) {
            case JOB_TYPE.OBTAIN_ITEM:
            case JOB_TYPE.REMOVE_TRAIT:
                return Utilities.NormalizeStringUpperCaseFirstLetters(jobType.ToString()) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(goals.conditionKey.ToString());
            case JOB_TYPE.HUNGER_RECOVERY:
            case JOB_TYPE.HUNGER_RECOVERY_STARVING:
                return "Hunger Recovery";
            case JOB_TYPE.HAPPINESS_RECOVERY:
            case JOB_TYPE.HAPPINESS_RECOVERY_FORLORN:
                return "Happiness Recovery";
            case JOB_TYPE.TIREDNESS_RECOVERY:
            case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
                return "Tiredness Recovery";
            default:
                if (targetInteractionType != INTERACTION_TYPE.NONE) {
                    return Utilities.NormalizeStringUpperCaseFirstLetters(targetInteractionType.ToString());
                } else {
                    return name;
                }
        }
    }
    #endregion
}

//public class ForcedActionsComparer : IEqualityComparer<GoapEffect> {

//    public bool Equals(GoapEffect x, GoapEffect y) {
//        bool matchKeys = false;
//        if(x.conditionKey is int && y.conditionKey is int) {
//            int effectInt = (int) x.conditionKey;
//            int preconditionInt = (int) y.conditionKey;
//            matchKeys = effectInt >= preconditionInt;
//        } else {
//            matchKeys = x.conditionKey == y.conditionKey;
//        }
//        return x.conditionType == y.conditionType && x.targetPOI == y.targetPOI && matchKeys;
//        //if (x.conditionType == y.conditionType) {
//        //    if (string.IsNullOrEmpty(x.conditionString()) || string.IsNullOrEmpty(y.conditionString())) {
//        //        return true;
//        //    } else {
//        //        return x.conditionString() == y.conditionString();
//        //    }
//        //}
//        //return false;
//    }

//    public int GetHashCode(GoapEffect obj) {
//        return obj.GetHashCode();
//    }
//}

public class SaveDataGoapPlanJob : SaveDataJobQueueItem {
    public SaveDataGoapEffect targetEffect;
    public int targetPOIID;
    public POINT_OF_INTEREST_TYPE targetPOIType;
    public TILE_OBJECT_TYPE targetPOITileObjectType;
    public INTERACTION_TYPE targetInteractionType;
    public bool allowDeadTargets;

    public List<SaveDataGoapEffect> forcedActionsGoapEffect;
    public List<INTERACTION_TYPE> forcedActionsType;

    public override void Save(JobQueueItem job) {
        base.Save(job);
        GoapPlanJob goapJob = job as GoapPlanJob;
        allowDeadTargets = goapJob.allowDeadTargets;
        targetInteractionType = goapJob.targetInteractionType;
        if (goapJob.targetPOI != null) {
            targetPOIID = goapJob.targetPOI.id;
            targetPOIType = goapJob.targetPOI.poiType;
            if(goapJob.targetPOI is TileObject) {
                targetPOITileObjectType = (goapJob.targetPOI as TileObject).tileObjectType;
            }
        } else {
            targetPOIID = -1;
        }
        targetEffect = new SaveDataGoapEffect();
        targetEffect.Save(goapJob.goals);

        forcedActionsGoapEffect = new List<SaveDataGoapEffect>();
        forcedActionsType = new List<INTERACTION_TYPE>();
        foreach (KeyValuePair<GoapEffect, INTERACTION_TYPE> kvp in goapJob.forcedActions) {
            SaveDataGoapEffect goapEffect = new SaveDataGoapEffect();
            goapEffect.Save(kvp.Key);
            forcedActionsGoapEffect.Add(goapEffect);

            forcedActionsType.Add(kvp.Value);
        }
    }

    //public override JobQueueItem Load() {
    //    return base.Load();
    //}
}

public interface IGoapJobPremadeNodeCreator {
    IPointOfInterest targetPOI { get; set; }
}

public struct ActionJobPremadeNodeCreator : IGoapJobPremadeNodeCreator {
    public INTERACTION_TYPE actionType;
    public IPointOfInterest targetPOI { get; set; }
}

public struct StateJobPremadeNodeCreator : IGoapJobPremadeNodeCreator {
    public CHARACTER_STATE stateType;
    public IPointOfInterest targetPOI { get; set; }
}