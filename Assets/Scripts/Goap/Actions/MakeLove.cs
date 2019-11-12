using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class MakeLove : GoapAction {

    public Character targetCharacter { get; private set; }

    public MakeLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.MAKE_LOVE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Flirt_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            Bed bed = poiTarget as Bed;
            poiTargetAlterEgo = targetCharacter.currentAlterEgo;
            if (bed.GetActiveUserCount() == 0 && targetCharacter.currentParty == actor.ownParty) {
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
        if(currentState.name == "Make Love Success") {
            actor.AdjustDoNotGetLonely(-1);
            targetCharacter.AdjustDoNotGetLonely(-1);
        }
        RemoveTraitFrom(targetCharacter, "Wooed");
        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        //if (currentState.name == "Make Love Success") {
        //    if (actor.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER)) {
        //        AddTraitTo(actor, "Satisfied", targetCharacter);
        //        AddTraitTo(targetCharacter, "Satisfied", actor);
        //    } else if (actor.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR)) {
        //        AddTraitTo(actor, "Ashamed", targetCharacter);
        //        AddTraitTo(targetCharacter, "Ashamed", actor);
        //    }
        //}
        //actor.ownParty.RemoveCharacter(targetCharacter);
        //RemoveTraitFrom(targetCharacter, "Wooed");

        //targetCharacter.RemoveTargettedByAction(this);
        //if (targetCharacter.currentAction == this) {
        //    targetCharacter.SetCurrentAction(null);
        //}
    }
    public override bool CanReactToThisCrime(Character character) {
        //do not allow actor and target character to react
        return character != actor && character != targetCharacter;
    }
    public override bool IsTarget(IPointOfInterest poi) {
        return targetCharacter == poi || poiTarget == poi;
    }
    public override void OnStopActionWhileTravelling() {
        base.OnStopActionWhileTravelling();
        actor.ownParty.RemoveCharacter(targetCharacter);
        RemoveTraitFrom(targetCharacter, "Wooed");
        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
        actor.DropPlan(parentPlan, true);
    }
    public override void DoAction() {
        base.DoAction();
        targetCharacter.AddTargettedByAction(this);
    }
    #endregion

    #region Effects
    private void PreMakeLoveSuccess() {
        if (actor.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
            SetCommittedCrime(CRIME.INFIDELITY, new Character[] { actor, targetCharacter });
        }
        actor.AdjustDoNotGetLonely(1);
        targetCharacter.AdjustDoNotGetLonely(1);

        targetCharacter.SetCurrentAction(this);
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.SetIntelReaction(MakeLoveSuccessReactions);
    }
    private void PerTickMakeLoveSuccess() {
        //**Per Tick Effect 1 * *: Actor's Happiness Meter +500
        int actorHappinessAmount = 500;
        int targetHappinessAmount = 500;
        if (actor.traitContainer.GetNormalTrait("Lustful") != null) {
            actorHappinessAmount += 100;
        } else if (actor.traitContainer.GetNormalTrait("Chaste") != null) {
            actorHappinessAmount -= 100;
        }

        if (targetCharacter.traitContainer.GetNormalTrait("Lustful") != null) {
            targetHappinessAmount += 100;
        } else if (targetCharacter.traitContainer.GetNormalTrait("Chaste") != null) {
            targetHappinessAmount -= 100;
        }

        actor.AdjustHappiness(actorHappinessAmount);
        //**Per Tick Effect 2**: Target's Happiness Meter +500
        targetCharacter.AdjustHappiness(targetHappinessAmount);
    }
    private void AfterMakeLoveSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        targetCharacter.AdjustDoNotGetLonely(-1);

        //**After Effect 1**: If Actor and Target are Lovers, they both gain Cheery trait. If Actor and Target are Paramours, they both gain Ashamed trait.
        if (actor is SeducerSummon) {
            //kill the target character
            targetCharacter.Death("seduced", this, actor);
        }

        //Plagued chances
        Plagued actorPlagued = actor.traitContainer.GetNormalTrait("Plagued") as Plagued;
        Plagued targetPlagued = targetCharacter.traitContainer.GetNormalTrait("Plagued") as Plagued;
        if ((actorPlagued == null || targetPlagued == null) && (actorPlagued != null || targetPlagued != null)) {
            //if either the actor or the target is not yet plagued and one of them is plagued, check for infection chance
            if (actorPlagued != null) {
                //actor has plagued trait
                int roll = Random.Range(0, 100);
                if (roll < actorPlagued.GetMakeLoveInfectChance()) {
                    //target will be infected with plague
                    if (AddTraitTo(targetCharacter, "Plagued", actor)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddLogToInvolvedObjects();
                    }
                }
            } else if (targetPlagued != null) {
                //target has plagued trait
                int roll = Random.Range(0, 100);
                if (roll < targetPlagued.GetMakeLoveInfectChance()) {
                    //actor will be infected with plague
                    if (AddTraitTo(actor, "Plagued", targetCharacter)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddLogToInvolvedObjects();
                    }
                }
            }
        }

        if (actor.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
            AddTraitTo(actor, "Satisfied", targetCharacter);
            AddTraitTo(targetCharacter, "Satisfied", actor);
        } else if (actor.relationshipContainer.HasRelationshipWith(targetCharacter.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
            AddTraitTo(actor, "Ashamed", targetCharacter);
            AddTraitTo(targetCharacter, "Ashamed", actor);
        }
        actor.ownParty.RemoveCharacter(targetCharacter);
        RemoveTraitFrom(targetCharacter, "Wooed");

        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
    }
    private void PreMakeLoveFail() {
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void AfterMakeLoveFail() {
        //**After Effect 1**: Actor gains Annoyed trait.
        AddTraitTo(actor, "Annoyed", actor);
        //actor.ownParty.RemoveCharacter(targetCharacter);

        actor.ownParty.RemoveCharacter(targetCharacter);
        RemoveTraitFrom(targetCharacter, "Wooed");

        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void AfterTargetMissing() {
        actor.ownParty.RemoveCharacter(targetCharacter);
        RemoveTraitFrom(targetCharacter, "Wooed");

        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
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
    private List<string> MakeLoveSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = targetCharacter;
        //RELATIONSHIP_EFFECT recipientRelationshipWithActor = recipient.GetRelationshipEffectWith(actor);
        //RELATIONSHIP_EFFECT recipientRelationshipWithTarget = recipient.GetRelationshipEffectWith(target);
        Relatable actorLover = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Relatable targetLover = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Relatable actorParamour = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        Relatable targetParamour = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);


        bool hasFled = false;
        if (isOldNews) {
            reactions.Add("This is old news.");
            if (status == SHARE_INTEL_STATUS.WITNESSED) {
                hasFled = true;
                recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
            }
        } else {
            //- Recipient is the Actor
            if(recipient == actor) {
                if(targetLover == recipient.currentAlterEgo) {
                    reactions.Add("That's private!");
                } else if (targetParamour == recipient.currentAlterEgo) {
                    reactions.Add("Don't tell anyone. *wink**wink*");
                }
            }
            //- Recipient is the Target
            else if (recipient == target) {
                if (actorLover == recipient.currentAlterEgo) {
                    reactions.Add("That's private!");
                } else if (actorParamour == recipient.currentAlterEgo) {
                    reactions.Add("Don't you dare judge me!");
                }
            }
            //- Recipient is Actor's Lover
            else if (recipient.currentAlterEgo == actorLover) {
                string response = string.Empty;
                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this) ) {
                    response = string.Format("I've had enough of {0}'s shenanigans!", actor.name);
                    recipient.CreateUndermineJobOnly(actor, "informed", status);
                } else {
                    response = string.Format("I'm still the one {0} comes home to.", actor.name);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        hasFled = true;
                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                    }
                }
                if(recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} seduced both of us. {1} must pay for this.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" I already know that {0} is a harlot.", target.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                    }
                }else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" {0} is my blood. Blood is thicker than water.", target.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                    }
                } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" My friendship with {0} is much stronger than this incident.", target.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                    }
                } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
                    response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                    recipient.CreateUndermineJobOnly(target, "informed", status);
                } else if (!recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" I'm not even going to bother myself with {0}.", target.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                    }
                }
                reactions.Add(response);
            }
            //- Recipient is Target's Lover
            else if (recipient.currentAlterEgo == targetLover) {
                string response = string.Empty;
                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    response = string.Format("I've had enough of {0}'s shenanigans!", target.name);
                    recipient.CreateUndermineJobOnly(target, "informed", status);
                } else {
                    response = string.Format("I'm still the one {0} comes home to.", target.name);
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        hasFled = true;
                        recipient.marker.AddAvoidInRange(target, reason: "saw something shameful");
                    }
                }
                if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} seduced both of us. {1} must pay for this.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" I already know that {0} is a harlot.", actor.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(target, reason: "saw something shameful");
                        }
                    }
                } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.RELATIVE)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" {0} is my blood. Blood is thicker than water.", actor.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(target, reason: "saw something shameful");
                        }
                    }
                } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" My friendship with {0} is much stronger than this incident.", actor.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(target, reason: "saw something shameful");
                        }
                    }
                } else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
                    response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                    recipient.CreateUndermineJobOnly(actor, "informed", status);
                } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo)) {
                    if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" I'm not even going to bother myself with {0}.", actor.name);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            hasFled = true;
                            recipient.marker.AddAvoidInRange(target, reason: "saw something shameful");
                        }
                    }
                }
                reactions.Add(response);
            }
            //- Recipient is Actor/Target's Paramour
            else if (recipient.currentAlterEgo == actorParamour || recipient.currentAlterEgo == targetParamour) {
                reactions.Add("I have no right to complain. Bu..but I wish that we could be like that.");
                AddTraitTo(recipient, "Heartbroken");
            }
            //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target.currentAlterEgo) {
                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                    AlterEgoData ego = actorLover as AlterEgoData;
                    reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", actor.name, actorLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    recipient.CreateShareInformationJob(ego.owner, this);
                } else {
                    reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.relatableName));
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        hasFled = true;
                        recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                    }
                }
            }
            //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor.currentAlterEgo) {
                if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    AlterEgoData ego = targetLover as AlterEgoData;
                    reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", target.name, targetLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    recipient.CreateShareInformationJob(ego.owner, this);
                } else {
                    reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.relatableName));
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        hasFled = true;
                        recipient.marker.AddAvoidInRange(target, reason: "saw something shameful");
                    }
                }
            }
            //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target.currentAlterEgo) {
                AlterEgoData ego = actorLover as AlterEgoData;
                reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, actorLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    hasFled = true;
                    recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                }
            }
            //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor.currentAlterEgo) {
                AlterEgoData ego = targetLover as AlterEgoData;
                reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", target.name, targetLover.relatableName, Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(ego.owner.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    hasFled = true;
                    recipient.marker.AddAvoidInRange(target, reason: "saw something shameful");
                }
            }
            //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
            else if (actorLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NONE && actorLover != target.currentAlterEgo) {
                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.relatableName));
                RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
            }
            //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
            else if (targetLover != null && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NONE && targetLover != actor.currentAlterEgo) {
                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.relatableName));
                RelationshipManager.Instance.RelationshipDegradation(target, recipient, this);
            }
            //- Else Catcher
            else {
                reactions.Add("That is none of my business.");
                if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    hasFled = true;
                    recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                }
            }
        }

        if (status == SHARE_INTEL_STATUS.WITNESSED && !hasFled) {
            if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) 
                || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
                recipient.CreateWatchEvent(this, null, actor);
            } else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)
                || recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
                recipient.CreateWatchEvent(this, null, target);
            }
        }
        return reactions;
    }
    #endregion
}

public class MakeLoveData : GoapActionData {
    public MakeLoveData() : base(INTERACTION_TYPE.MAKE_LOVE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }
}
