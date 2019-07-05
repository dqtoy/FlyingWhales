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
            Bed bed = poiTarget as Bed;
            poiTargetAlterEgo = targetCharacter.currentAlterEgo;
            if (bed.GetActiveUserCount() == 0 && targetCharacter.currentParty == actor.ownParty && !targetCharacter.isStarving && !targetCharacter.isExhausted 
                && targetCharacter.GetNormalTrait("Annoyed") == null) {
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
        RemoveTraitFrom(targetCharacter, "Wooed");
        targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentAction == this) {
            targetCharacter.SetCurrentAction(null);
        }
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        if (endedAtState.name == "Make Love Success") {
            if (actor.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.LOVER)) {
                AddTraitTo(actor, "Cheery", targetCharacter);
                AddTraitTo(targetCharacter, "Cheery", actor);
            } else if (actor.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR)) {
                AddTraitTo(actor, "Ashamed", targetCharacter);
                AddTraitTo(targetCharacter, "Ashamed", actor);
            }
        }
        actor.ownParty.RemoveCharacter(targetCharacter);
        RemoveTraitFrom(targetCharacter, "Wooed");

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
    #endregion

    #region Effects
    private void PreMakeLoveSuccess() {
        targetCharacter.SetCurrentAction(this);
        currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.SetIntelReaction(MakeLoveSuccessReactions);
    }
    private void PerTickMakeLoveSuccess() {
        //**Per Tick Effect 1 * *: Actor's Happiness Meter +10
        actor.AdjustHappiness(10);
        //**Per Tick Effect 2**: Target's Happiness Meter +10
        targetCharacter.AdjustHappiness(10);
    }
    private void AfterMakeLoveSuccess() {
        //**After Effect 1**: If Actor and Target are Lovers, they both gain Cheery trait. If Actor and Target are Paramours, they both gain Ashamed trait.
        if (actor.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.PARAMOUR)) {
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
    private List<string> MakeLoveSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = targetCharacter;
        RELATIONSHIP_EFFECT recipientRelationshipWithActor = recipient.GetRelationshipEffectWith(actor);
        RELATIONSHIP_EFFECT recipientRelationshipWithTarget = recipient.GetRelationshipEffectWith(target);
        Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        Character actorParamour = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        Character targetParamour = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);



        if (isOldNews) {
            reactions.Add("This is old news.");
            //if(status == SHARE_INTEL_STATUS.WITNESSED) {
            //    recipient.marker.AddAvoidInRange(actor);
            //}
        } else {
            //- Recipient is the Actor
            if(recipient == actor) {
                if(targetLover == recipient) {
                    reactions.Add("That's private!");
                } else if (targetParamour == recipient) {
                    reactions.Add("Don't tell anyone. *wink**wink*");
                }
            }
            //- Recipient is the Target
            else if (recipient == target) {
                if (actorLover == recipient) {
                    reactions.Add("That's private!");
                } else if (actorParamour == recipient) {
                    reactions.Add("Don't you dare judge me!");
                }
            }
            //- Recipient is Actor's Lover
            else if (recipient == actorLover) {
                string response = string.Empty;
                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this) ) {
                    response = string.Format("I've had enough of {0}'s shenanigans!", actor.name);
                    recipient.CreateUndermineJobOnly(actor, "informed", status);
                } else {
                    response = string.Format("I'm still the one {0} comes home to.", actor.name);
                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    //    recipient.marker.AddAvoidInRange(actor);
                    //}
                }
                if(recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} seduced both of us. {1} must pay for this.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" I already know that {0} is a harlot.", target.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(actor);
                        //}
                    }
                }else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.RELATIVE)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" {0} is my blood. Blood is thicker than water.", target.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(actor);
                        //}
                    }
                } else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.FRIEND)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" My friendship with {0} is much stronger than this incident.", target.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(actor);
                        //}
                    }
                } else if (recipient.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.ENEMY)) {
                    response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                    recipient.CreateUndermineJobOnly(target, "informed", status);
                } else if (!recipient.HasRelationshipWith(target)) {
                    if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                        response += string.Format(" {0} is a snake. {1} must pay for this!", target.name, Utilities.GetPronounString(target.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(target, "informed", status);
                    } else {
                        response += string.Format(" I'm not even going to bother myself with {0}.", target.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(actor);
                        //}
                    }
                }
                reactions.Add(response);
            }
            //- Recipient is Target's Lover
            else if (recipient == targetLover) {
                string response = string.Empty;
                if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    response = string.Format("I've had enough of {0}'s shenanigans!", target.name);
                    recipient.CreateUndermineJobOnly(target, "informed", status);
                } else {
                    response = string.Format("I'm still the one {0} comes home to.", target.name);
                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    //    recipient.marker.AddAvoidInRange(target);
                    //}
                }
                if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.PARAMOUR)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} seduced both of us. {1} must pay for this.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" I already know that {0} is a harlot.", actor.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(target);
                        //}
                    }
                } else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.RELATIVE)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" {0} is my blood. Blood is thicker than water.", actor.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(target);
                        //}
                    }
                } else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.FRIEND)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake! I can't believe {1} would do this to me.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" My friendship with {0} is much stronger than this incident.", actor.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(target);
                        //}
                    }
                } else if (recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                    response += string.Format(" I always knew that {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                    recipient.CreateUndermineJobOnly(actor, "informed", status);
                } else if (!recipient.HasRelationshipWith(actor)) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        response += string.Format(" {0} is a snake. {1} must pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else {
                        response += string.Format(" I'm not even going to bother myself with {0}.", actor.name);
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    recipient.marker.AddAvoidInRange(target);
                        //}
                    }
                }
                reactions.Add(response);
            }
            //- Recipient is Actor/Target's Paramour
            else if (recipient == actorParamour || recipient == targetParamour) {
                reactions.Add("I have no right to complain. Bu..but I wish that we could be like that.");
                AddTraitTo(recipient, "Heartbroken");
            }
            //- Recipient has a positive relationship with Actor's Lover and Actor's Lover is not the Target
            else if (recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.POSITIVE && actorLover != target) {
                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                    reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", actor.name, actorLover.name, Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    recipient.CreateShareInformationJob(actorLover, this);
                } else {
                    reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.name));
                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    //    recipient.marker.AddAvoidInRange(actor);
                    //}
                }
            }
            //- Recipient has a positive relationship with Target's Lover and Target's Lover is not the Actor
            else if (recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE && targetLover != actor) {
                if (CharacterManager.Instance.RelationshipDegradation(target, recipient, this)) {
                    reactions.Add(string.Format("{0} is cheating on {1}?! I must let {2} know.", target.name, targetLover.name, Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    recipient.CreateShareInformationJob(targetLover, this);
                } else {
                    reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.name));
                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    //    recipient.marker.AddAvoidInRange(target);
                    //}
                }
            }
            //- Recipient has a negative relationship with Actor's Lover and Actor's Lover is not the Target
            else if (recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NEGATIVE && actorLover != target) {
                reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", actor.name, actorLover.name, Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actorLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                //    recipient.marker.AddAvoidInRange(actor);
                //}
            }
            //- Recipient has a negative relationship with Target's Lover and Target's Lover is not the Actor
            else if (recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NEGATIVE && targetLover != actor) {
                reactions.Add(string.Format("{0} is cheating on {1}? {2} got what {3} deserves.", target.name, targetLover.name, Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(targetLover.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                //    recipient.marker.AddAvoidInRange(target);
                //}
            }
            //- Recipient has a no relationship with Actor's Lover and Actor's Lover is not the Target
            else if (recipient.GetRelationshipEffectWith(actorLover) == RELATIONSHIP_EFFECT.NONE && actorLover != target) {
                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", actor.name, actorLover.name));
                CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
            }
            //- Recipient has no relationship with Target's Lover and Target's Lover is not the Actor
            else if (recipient.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.NONE && targetLover != actor) {
                reactions.Add(string.Format("{0} is cheating on {1}? I don't want to get involved.", target.name, targetLover.name));
                CharacterManager.Instance.RelationshipDegradation(target, recipient, this);
            }
            //- Else Catcher
            else {
                reactions.Add("That is none of my business.");
                //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                //    recipient.marker.AddAvoidInRange(actor);
                //}
            }
        }
        return reactions;
    }
    #endregion
}
