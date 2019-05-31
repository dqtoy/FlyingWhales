using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeLove : GoapAction {

    public Character targetCharacter { get; private set; }

    public MakeLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.MAKE_LOVE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        targetCharacter.OnTargettedByAction(this);
        if (!isTargetMissing) {
            poiTargetAlterEgo = targetCharacter.currentAlterEgo;
            if (targetCharacter.currentParty == actor.ownParty && !targetCharacter.isStarving && !targetCharacter.isExhausted 
                && targetCharacter.GetTrait("Annoyed") == null) {
                SetState("Make Love Success");
            } else {
                SetState("Make Love Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    protected override void AddDefaultObjectsToLog(Log log) {
        base.AddDefaultObjectsToLog(log);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public override void OnStopActionDuringCurrentState() {
        actor.ownParty.RemoveCharacter(targetCharacter);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
    }
    public override bool CanReactToThisCrime(Character character) {
        //do not allow actor and target character to react
        return character != actor && character != targetCharacter;
    }
    public override bool IsTarget(IPointOfInterest poi) {
        return targetCharacter == poi || poiTarget == poi;
    }
    #endregion

    #region Effects
    private void PreMakeLoveSuccess() {
        targetCharacter.SetCurrentAction(this);
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.SetIntelReaction(State1Reactions);
    }
    private void PerTickMakeLoveSuccess() {
        //**Per Tick Effect 1 * *: Actor's Happiness Meter +10
        actor.AdjustHappiness(10);
        //**Per Tick Effect 2**: Target's Happiness Meter +10
        targetCharacter.AdjustHappiness(10);
    }
    private void AfterMakeLoveSuccess() {
        //**After Effect 1**: If Actor and Target are Lovers, they both gain Cheery trait. If Actor and Target are Paramours, they both gain Ashamed trait.
        if (actor.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER)) {
            AddTraitTo(actor, "Cheery", targetCharacter);
            AddTraitTo(targetCharacter, "Cheery", actor);
        } else {
            AddTraitTo(actor, "Ashamed", targetCharacter);
            AddTraitTo(targetCharacter, "Ashamed", actor);
            SetCommittedCrime(CRIME.INFIDELITY, new Character[] { actor, targetCharacter });
        }

        actor.ownParty.RemoveCharacter(targetCharacter);
        RemoveTraitFrom(targetCharacter, "Wooed");
    }
    private void PreMakeLoveFail() {
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void AfterMakeLoveFail() {
        //**After Effect 1**: Actor gains Annoyed trait.
        AddTraitTo(actor, "Annoyed", actor);
        actor.ownParty.RemoveCharacter(targetCharacter);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    //#region Requirements
    //protected bool Requirement() {
    //    return poiTarget.state != POI_STATE.INACTIVE;
    //}
    //#endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }

    #region Intel Reactions
    private List<string> State1Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character target = targetCharacter;

        RELATIONSHIP_EFFECT recipientWithActor = recipient.GetRelationshipEffectWith(actor);
        RELATIONSHIP_EFFECT recipientWithTarget = recipient.GetRelationshipEffectWith(target);
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);

        //Recipient and Actor are the same or Recipient and Target are the same:
        if (recipient == actor || recipient == targetCharacter) {
            //- **Recipient Response Text**: "I know what I've done!"
            reactions.Add(string.Format("I know what I've done!", actor.name));
            //-**Recipient Effect**:  no effect
        }
        //Recipient considers Actor as a Lover:
        else if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "[Actor Name] is cheating on me!?"
            reactions.Add(string.Format("{0} is cheating on me!?", actor.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between and Recipient and Target. 
            CharacterManager.Instance.RelationshipDegradation(poiTargetAlterEgo, recipient, this);
            //Add an Undermine Job to Recipient versus Target (choose at random). 
            recipient.ForceCreateUndermineJob(target);
            //Add a Breakup Job to Recipient versus Actor.
            recipient.CreateBreakupJob(actor);
        }
        //Recipient considers Target as a Lover:
        else if (recipient.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "[Target Name] is cheating on me!?"
            reactions.Add(string.Format("{0} is cheating on me!?", targetCharacter.name));
            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between Recipient and Actor. 
            CharacterManager.Instance.RelationshipDegradation(actorAlterEgo, recipient, this);
            //Add an Undermine Job to Recipient versus Actor.
            recipient.ForceCreateUndermineJob(actor);
            //Add a Breakup Job to Recipient versus Target.
            recipient.CreateBreakupJob(target);
        }
        //Recipient considers Actor as a Paramour and Actor and Target are Lovers:
        else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.PARAMOUR) && actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "I will make sure that [Actor Name] leaves [Target Name] soon!"
            reactions.Add(string.Format("I will make sure that {0} leaves {1} soon!", actor.name, targetCharacter.name));
            //-**Recipient Effect * *: Add a Destroy Love Job to Recipient versus Actor or Target(choose at random).
            Character randomTarget = actor;
            if (Random.Range(0, 2) == 1) {
                randomTarget = target;
            }
            GoapPlanJob job = new GoapPlanJob("Destroy Love", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = randomTarget },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { randomTarget } }, });
            job.SetCannotOverrideJob(true);
            job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            recipient.jobQueue.AddJobInQueue(job, true, false);
        }
        //Recipient considers Target as a Paramour and Actor and Target are Lovers:
        else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
            //- **Recipient Response Text**: "I will make sure that [Target Name] leaves [Actor Name] soon!"
            reactions.Add(string.Format("I will make sure that {0} leaves {1} soon!", targetCharacter.name, actor.name));
            //-**Recipient Effect * *: Add a Destroy Love Job to Recipient versus Actor or Target(choose at random).
            Character randomTarget = actor;
            if (Random.Range(0, 2) == 1) {
                randomTarget = target;
            }
            GoapPlanJob job = new GoapPlanJob("Destroy Love", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = randomTarget },
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.NONE, new object[] { randomTarget } }, });
            job.SetCannotOverrideJob(true);
            job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            recipient.jobQueue.AddJobInQueue(job, true, false);
        }
        //Actor and Target are Paramours. Recipient has no positive relationship with either of them. Actor has a Lover and Recipient has a positive relationship with Actor's Lover:
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipientWithActor != RELATIONSHIP_EFFECT.POSITIVE && recipientWithTarget != RELATIONSHIP_EFFECT.POSITIVE
            && actorLover != null && recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: "[Actor's Lover Name] must know about [Actor Name]'s affair with [Target Name]!"
            reactions.Add(string.Format("{0} must know about {1}'s affair with {2}!", actorLover.name, actor.name, targetCharacter.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Actor's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actorAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }
        }
        //Actor and Target are Paramours. Recipient has no positive relationship with either of them. Target has a Lover and Recipient has a positive relationship with Target's Lover:
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipientWithActor != RELATIONSHIP_EFFECT.POSITIVE && recipientWithTarget != RELATIONSHIP_EFFECT.POSITIVE
            && targetLover != null && recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE) {
            //- **Recipient Response Text**: "[Target's Lover Name] must know about [Target Name]'s affair with [Actor Name]!"
            reactions.Add(string.Format("{0} must know about {1}'s affair with {2}!", targetLover.name, targetCharacter.name, actor.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Target's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, poiTargetAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }
        }
        //Actor and Target are Paramours. Recipient has no positive relationship with either of them. Recipient is from the same faction as either one of them.
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipientWithActor != RELATIONSHIP_EFFECT.POSITIVE && recipientWithTarget != RELATIONSHIP_EFFECT.POSITIVE 
            && (recipient.faction == actor.faction || recipient.faction == target.faction)) {
            //- **Recipient Response Text**: "I'm not interested in meddling in other people's private lives."
            reactions.Add("I'm not interested in meddling in other people's private lives.");
            //-**Recipient Effect * *: No effect
        }
        //Actor and Target are Paramours. Recipient considers Actor as an Enemy. Actor has a Lover and Recipient does not consider Actor's Lover an Enemy.
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY) && actorLover != null 
            && !recipient.HasRelationshipOfTypeWith(actorLover, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "I can't wait to let [Actor's Lover's Name] find out about this affair."
            reactions.Add(string.Format("I can't wait to let {0} find out about this affair.", actorLover.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Actor's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, actorAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, actorLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }
        }
        //Actor and Target are Paramours. Recipient considers Target as an Enemy. Target has a Lover and Recipient does not consider Target's Lover an Enemy.
        else if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR) && recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY) && targetLover != null
            && !recipient.HasRelationshipOfTypeWith(targetLover, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "I can't wait to let [Target's Lover's Name] find out about this affair."
            reactions.Add(string.Format("I can't wait to let {0} find out about this affair.", targetLover.name));
            //- **Recipient Effect**: Add a Report Crime Job to Recipient targeting Target's Lover.
            //GoapPlanJob job = new GoapPlanJob("Report Crime", INTERACTION_TYPE.REPORT_CRIME, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
            //        { INTERACTION_TYPE.REPORT_CRIME, new object[] { committedCrime, poiTargetAlterEgo, this }}
            //});
            if (!recipient.jobQueue.HasJobWithOtherData("Share Information", this)) {
                GoapPlanJob job = new GoapPlanJob("Share Information", INTERACTION_TYPE.SHARE_INFORMATION, targetLover, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.SHARE_INFORMATION, new object[] { this }}
                        });
                //job.SetCannotOverrideJob(true);
                job.SetCancelOnFail(true);
                recipient.jobQueue.AddJobInQueue(job, true, false);
            }
        }
        return reactions;
    }
    #endregion
}
