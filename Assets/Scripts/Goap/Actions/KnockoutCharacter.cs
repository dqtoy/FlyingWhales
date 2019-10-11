using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockoutCharacter : GoapAction {

    public KnockoutCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.KNOCKOUT_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        doesNotStopTargetCharacter = true;
        actionIconString = GoapActionStateDB.Stealth_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //rather than checking location check if the character is not in anyone elses party and is still active
        if (!isTargetMissing) {
            if (poiTarget.GetNormalTrait("Vigilant") != null) {
                SetState("Knockout Fail");
            } else {
                SetState("Knockout Success");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    public override int GetArrangedLogPriorityIndex(string priorityID) {
        if (priorityID == "description") {
            return 0;
        } else if (priorityID == "unconscious") {
            return 1;
        }
        return base.GetArrangedLogPriorityIndex(priorityID);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        //if(currentState.name == "Knockout Fail") {
        //    if (poiTarget is Character) {
        //        Character targetCharacter = poiTarget as Character;
        //        targetCharacter.marker.AddHostileInRange(actor, false);
        //    }
        //}
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget && actor.GetNormalTrait("Serial Killer") != null;
    }
    #endregion

    #region State Effects
    private void PreKnockoutSuccess() {
        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        currentState.SetIntelReaction(KnockoutSuccessIntelReaction);
    }
    private void AfterKnockoutSuccess() {
        poiTarget.AddTrait("Unconscious", actor, gainedFromDoing: this);
    }
    private void PreKnockoutFail() {
        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        currentState.SetIntelReaction(KnockoutFailIntelReaction);
    }
    private void AfterKnockoutFail() {
        if(poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            if (!targetCharacter.ReactToCrime(committedCrime, this, actorAlterEgo, SHARE_INTEL_STATUS.WITNESSED)) {
                CharacterManager.Instance.RelationshipDegradation(actor, targetCharacter, this);
                targetCharacter.marker.AddHostileInRange(actor, false);
                //NOTE: Adding hostile in range is done after the action is done processing fully, See OnResultReturnedToActor
            }
        }
    }
    #endregion

    #region Intel Reactions
    private List<string> KnockoutSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("Do not tell anybody, please!");
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    reactions.Add(string.Format("I'm embarrassed that {0} was able to do that to me!", actor.name));
                }
                //- Recipient Has Positive Relationship with Actor
                else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                    RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
                    if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateAttemptToStopCurrentActionAndJob(actor, parentPlan.job);
                        }
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
                    } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
                    } else {
                        reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
                    }
                }
                //- Recipient Has Negative Relationship with Actor
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
                    if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
                        }
                        reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
                    } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add("My enemies fighting each other. What a happy day!");
                    } else {
                        recipient.ReactToCrime(CRIME.ABERRATION, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
                        }
                        reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
                    }
                }
                //- Recipient Has No Relationship with Actor
                else {
                    RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
                    if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        }
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
                    } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
                    } else {
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add(string.Format("I am not fond of {0} at all so I don't care what happens to {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    }
                }
            }
        }
        return reactions;
    }
    private List<string> KnockoutFailIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("Do not tell anybody, please!");
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    reactions.Add(string.Format("{0} failed. Anyone that tries to do that will also fail.", actor.name));
                }
                //- Recipient Has Positive Relationship with Actor
                else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                    RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
                    if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            //Add Attempt to Stop Job
                            //recipient.CreateKnockoutJob(actor);
                        }
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
                    } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
                    } else {
                        reactions.Add(string.Format("I'm sure there's a reason {0} did that.", actor.name));
                    }
                }
                //- Recipient Has Negative Relationship with Actor
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
                    if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
                        }
                        reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
                    } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add("My enemies fighting each other. What a happy day!");
                    } else {
                        recipient.ReactToCrime(CRIME.ABERRATION, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed", SHARE_INTEL_STATUS.INFORMED);
                        }
                        reactions.Add(string.Format("{0} is such a vile creature!", actor.name));
                    }
                }
                //- Recipient Has No Relationship with Actor
                else {
                    RELATIONSHIP_EFFECT relationshipWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);
                    if (relationshipWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        }
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
                    } else if (relationshipWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add(string.Format("{0} shouldn't have done that to {1}!", actor.name, targetCharacter.name));
                    } else {
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor);
                        }
                        reactions.Add(string.Format("I am not fond of {0} at all so I don't care what happens to {1}.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class KnockoutCharacterData : GoapActionData {
    public KnockoutCharacterData() : base(INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && actor.GetNormalTrait("Serial Killer") != null;
    }
}
