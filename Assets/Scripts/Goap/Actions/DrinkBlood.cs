using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinkBlood : GoapAction {
    protected override string failActionState { get { return "Drink Fail"; } }

    public DrinkBlood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DRINK_BLOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructRequirementOnBuildGoapTree() {
        _requirementOnBuildGoapTreeAction = RequirementOnBuildGoapTree;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (actor.isStarving) {
            AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget }, HasUnconsciousOrRestingTarget);
        }
        if (actor.GetNormalTrait("Vampiric") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
        }
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            Character target = poiTarget as Character;
            if(target.GetNormalTrait("Unconscious", "Resting") != null) {
                SetState("Drink Success");
            } else {
                SetState("Drink Fail");
            }
        } else {
            if (!poiTarget.IsAvailable()) {
                SetState("Drink Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        return 1;
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Drink Success") {
            actor.AdjustDoNotGetHungry(-1);
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
    protected bool RequirementOnBuildGoapTree() {
        if (!actor.isStarving) {
            Character target = poiTarget as Character;
            return target.GetNormalTrait("Unconscious", "Resting") != null;
        }
        return true;
    }
    #endregion

    #region Preconditions
    private bool HasUnconsciousOrRestingTarget() {
        Character target = poiTarget as Character;
        return target.GetNormalTrait("Unconscious", "Resting") != null;
    }
    #endregion

    #region Effects
    private void PreDrinkSuccess() {
        SetCommittedCrime(CRIME.ABERRATION, new Character[] { actor });
        //poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AdjustDoNotGetHungry(1);
    }
    private void PerTickDrinkSuccess() {
        actor.AdjustFullness(12);
    }
    private void AfterDrinkSuccess() {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        actor.AdjustDoNotGetHungry(-1);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 95) {
            Lethargic lethargic = new Lethargic();
            AddTraitTo(poiTarget, lethargic, actor);
        } else {
            Vampiric vampiric = new Vampiric();
            AddTraitTo(poiTarget, vampiric, actor);
        }
        currentState.SetIntelReaction(DrinkBloodSuccessIntelReaction);
    }
    //private void PreDrinkFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion

    #region Intel Reactions
    private List<string> DrinkBloodSuccessIntelReaction(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();

        //Recipient and Actor is the same:
        if (recipient == actor) {
            //- **Recipient Response Text**: Please do not tell anyone else about this. I beg you!
            reactions.Add("Please do not tell anyone else about this. I beg you!");
            //-**Recipient Effect * *: no effect
        }
        //Recipient and Actor are from the same faction and are lovers or paramours
        else if (actor.faction == recipient.faction && recipient.HasRelationshipOfTypeWith(actor, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
            //- **Recipient Response Text**: [Actor Name] may be a monster, but I love [him/her] still!
            reactions.Add(string.Format("{0} may be a monster, but I love {1} still!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
            //- **Recipient Effect**: no effect
        }
        //Recipient and Actor are from the same faction and are friends:
        else if (actor.faction == recipient.faction && recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.FRIEND)) {
            //- **Recipient Response Text**: I cannot be friends with a vampire but I will not report this to the others as my last act of friendship.
            reactions.Add("I cannot be friends with a vampire but I will not report this to the others as my last act of friendship.");
            //- **Recipient Effect**: Recipient and actor will no longer be friends
            CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.FRIEND);
        }
        //Recipient and Actor are from the same faction and have no relationship or are enemies:
        //Ask Marvin if actor and recipient must also have the same home location and they must be both at their home location
        else if (actor.faction == recipient.faction && (!recipient.HasRelationshipWith(actor, true) || recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY))) {
            //- **Recipient Response Text**: Vampires are not welcome here. [Actor Name] must be restrained!
            reactions.Add(string.Format("Vampires are not welcome here. {0} must be restrained!", actor.name));
            //-**Recipient Effect**: If soldier, noble or faction leader, brand Actor with Aberration crime (add Apprehend job). Otherwise, add a personal Report Crime job to the Recipient.
            if (recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                actor.AddCriminalTrait(CRIME.ABERRATION, this);
                GoapPlanJob job = recipient.CreateApprehendJobFor(actor);
                //if (job != null) {
                //    recipient.homeArea.jobQueue.AssignCharacterToJob(job, this);
                //}
            } else {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPORT_CRIME, INTERACTION_TYPE.REPORT_CRIME, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actorAlterEgo, this }}
                });
                job.SetCannotOverrideJob(true);
                recipient.jobQueue.AddJobInQueue(job);
            }
        }
        //Recipient and Actor are from the same faction (catches all other situations):
        else if (actor.faction == recipient.faction) {
            //- **Recipient Response Text**: Vampires are not welcome here. [Actor Name] must be restrained!
            reactions.Add(string.Format("Vampires are not welcome here. {0} must be restrained!", actor.name));
            //-**Recipient Effect**: If soldier, noble or faction leader, brand Actor with Aberration crime (add Apprehend job). Otherwise, add a personal Report Crime job to the Recipient.
            if (recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                actor.AddCriminalTrait(CRIME.ABERRATION, this);
                GoapPlanJob job = recipient.CreateApprehendJobFor(actor);
                //if (job != null) {
                //    recipient.homeArea.jobQueue.AssignCharacterToJob(job, this);
                //}
            } else {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPORT_CRIME, INTERACTION_TYPE.REPORT_CRIME, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actorAlterEgo, this }}
                });
                job.SetCannotOverrideJob(true);
                recipient.jobQueue.AddJobInQueue(job);
            }
        }
        //Recipient and Actor are from a different faction and have a positive relationship:
        else if (recipient.faction != actor.faction && recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.FRIEND)) {
            //- **Recipient Response Text**: I cannot be friends with a vampire.
            reactions.Add("I cannot be friends with a vampire.");
            //- **Recipient Effect**: Recipient and actor will no longer be friends
            CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.FRIEND);
        }
        //Recipient and Actor are from a different faction and are enemies:
        else if (recipient.faction != actor.faction && recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.FRIEND)) {
            //- **Recipient Response Text**: I knew there was something unnatural about [Actor Name]!
            reactions.Add(string.Format("I knew there was something unnatural about {0}!", actor.name));
            //- **Recipient Effect**: no effect
        }
        return reactions;
    }
    #endregion
}
