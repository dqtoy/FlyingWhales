using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformToWolfForm : GoapAction {

    public TransformToWolfForm(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Transform Success");
    }
    protected override int GetCost() {
        return 5;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Stroll Fail");
    //}
    #endregion

    #region State Effects
    public void AfterTransformSuccess() {
        Lycanthropy lycanthropy = actor.GetTrait("Lycanthropy") as Lycanthropy;
        lycanthropy.TurnToWolf();
        SetCommittedCrime(CRIME.ABERRATION);
        currentState.SetIntelReaction(TransformSuccessIntelReaction);
    }
    #endregion

    #region Intel Reactions
    private List<string> TransformSuccessIntelReaction(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        if (recipient == actor) {
            return reactions; //return empty list if same actor
        }
        //TODO: Recipient and Actor are from the same faction and are lovers or paramours

        //Recipient and Actor are from the same faction and are friends:
        if (actor.faction == recipient.faction && actor.HasRelationshipOfTypeWith(recipient, RELATIONSHIP_TRAIT.FRIEND)) {
            //- **Recipient Response Text**: I cannot be friends with a lycanthrope but I will not report this to the others as my last act of friendship.
            reactions.Add("I cannot be friends with a lycanthrope but I will not report this to the others as my last act of friendship.");
            //- **Recipient Effect**: Recipient and actor will no longer be friends
            CharacterManager.Instance.RemoveRelationshipBetween(actor, recipient, RELATIONSHIP_TRAIT.FRIEND);
        }
        //Recipient and Actor are from the same faction and have no relationship or are enemies:
        //Ask Marvin if actor and recipient must also have the same home location and they must be both at their home location
        else if (actor.faction == recipient.faction && (!actor.HasRelationshipWith(recipient) || actor.HasRelationshipOfTypeWith(recipient, RELATIONSHIP_TRAIT.ENEMY))) {
            //- **Recipient Response Text**: Lycanthropes are not welcome here. [Actor Name] must be restrained!
            reactions.Add(string.Format("Lycanthropes are not welcome here. {0} must be restrained!", actor.name));
            //-**Recipient Effect**: If soldier, noble or faction leader, brand Actor with Aberration crime (add Apprehend job). Otherwise, add a personal Report Crime job to the Recipient.
            if (recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                //Add Aberration Crime
                actor.AddCriminalTrait(CRIME.ABERRATION);
                GoapPlanJob job = actor.CreateApprehendJobForThisCharacter();
                //if (job != null) {
                //    recipient.homeArea.jobQueue.AssignCharacterToJob(job, this);
                //}
            } else {
                GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actor }}
                });
                job.SetCannotOverrideJob(true);
                recipient.jobQueue.AddJobInQueue(job);
            }
        }
        //Recipient and Actor are from the same faction (catches all other situations):
        else if (actor.faction == recipient.faction) {
            //- **Recipient Response Text**: Lycanthropes are not welcome here. [Actor Name] must be restrained!
            reactions.Add(string.Format("Lycanthropes are not welcome here. {0} must be restrained!", actor.name));
            //-**Recipient Effect**: If soldier, noble or faction leader, brand Actor with Aberration crime (add Apprehend job). Otherwise, add a personal Report Crime job to the Recipient.
            if (recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                //Add Aberration Crime
                actor.AddCriminalTrait(CRIME.ABERRATION);
                GoapPlanJob job = actor.CreateApprehendJobForThisCharacter();
                //if (job != null) {
                //    recipient.homeArea.jobQueue.AssignCharacterToJob(job, this);
                //}
            } else {
                GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, new Dictionary<INTERACTION_TYPE, object[]>() {
                    { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actor }}
                });
                job.SetCannotOverrideJob(true);
                recipient.jobQueue.AddJobInQueue(job);
            }
        }
        //Recipient and Actor are from a different faction and have a positive relationship:
        else if (recipient.faction != actor.faction && actor.HasRelationshipOfTypeWith(recipient, RELATIONSHIP_TRAIT.FRIEND)) {
            //- **Recipient Response Text**: I cannot be friends with a lycanthrope.
            reactions.Add("I cannot be friends with a lycanthrope.");
            //- **Recipient Effect**: Recipient and actor will no longer be friends
            CharacterManager.Instance.RemoveRelationshipBetween(actor, recipient, RELATIONSHIP_TRAIT.FRIEND);
        }
        //Recipient and Actor are from a different faction and are enemies:
        else if (recipient.faction != actor.faction && actor.HasRelationshipOfTypeWith(recipient, RELATIONSHIP_TRAIT.FRIEND)) {
            //- **Recipient Response Text**: I knew there was something impure about [Actor Name]!
            reactions.Add(string.Format("I knew there was something impure about {0}!", actor.name));
            //- **Recipient Effect**: no effect
        }
        return reactions;
    }
    #endregion
}
