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
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        //if (actor.isStarving) {
            AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget }, HasUnconsciousOrRestingTarget);
        //}
        if (actor.GetNormalTrait("Vampiric") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
        }
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            Character target = poiTarget as Character;
            //SetState("Drink Success");

            if (target.GetNormalTrait("Unconscious", "Resting") != null) {
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
        if(actor != poiTarget) {
            return true;
        }
        return false;
    }
    protected bool RequirementOnBuildGoapTree() {
        //if (!actor.isStarving) {
        //    Character target = poiTarget as Character;
        //    return target.GetNormalTrait("Unconscious", "Resting") != null;
        //}
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
        currentState.SetIntelReaction(DrinkBloodSuccessIntelReaction);
    }
    private void PerTickDrinkSuccess() {
        actor.AdjustFullness(12);
    }
    private void AfterDrinkSuccess() {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        actor.AdjustDoNotGetHungry(-1);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 70) {
            Lethargic lethargic = new Lethargic();
            AddTraitTo(poiTarget, lethargic, actor);
        } else {
            Vampiric vampiric = new Vampiric();
            AddTraitTo(poiTarget, vampiric, actor);
        }
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
    private List<string> DrinkBloodSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;
        bool isRecipientVampire = recipient.GetNormalTrait("Vampiric") != null;

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
                    reactions.Add("I know what I did.");
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    if (isRecipientVampire) {
                        //- Recipient is a Vampire
                        reactions.Add(string.Format("{0} must be the one that turned me into this...", actor.name));
                    } else {
                        //- Recipient is NOT a Vampire
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add("Vampires are cursed beings that must be destroyed!");
                            } else {
                                reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("I knew something is off with that {0}!", actor.name));
                        } else {
                            reactions.Add("Vampires are cursed beings that must be destroyed!");
                        }
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (isRecipientVampire) {
                        //- Recipient is a Vampire
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
                                if(status != SHARE_INTEL_STATUS.WITNESSED) {
                                    recipient.CreateKnockoutJob(actor);
                                }
                            } else {
                                reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
                            if (status != SHARE_INTEL_STATUS.WITNESSED) {
                                recipient.CreateKnockoutJob(actor);
                            }
                        } else {
                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
                            if (status != SHARE_INTEL_STATUS.WITNESSED) {
                                recipient.CreateKnockoutJob(actor);
                            }
                        }
                    } else {
                        //- Recipient is NOT a Vampire
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add("Vampires are cursed beings that must be destroyed!");
                            } else {
                                reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("I knew something is off with that {0}!", actor.name));
                        } else {
                            reactions.Add("Vampires are cursed beings that must be destroyed!");
                        }
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    if (isRecipientVampire) {
                        //- Recipient is a Vampire
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            if (recipient.marker.inVisionPOIs.Contains(actor)) {
                                recipient.marker.AddAvoidInRange(actor);
                            }
                        }
                        RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                        if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
                        } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("I may dislike {0} but I can't report a fellow vampire.", actor.name));
                        } else {
                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
                        }
                    } else {
                        //- Recipient is NOT a Vampire
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add("Vampires are cursed beings that must be destroyed!");
                            } else {
                                reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add("Vampires are cursed beings that must be destroyed!");
                        } else {
                            reactions.Add("Vampires are cursed beings that must be destroyed!");
                        }
                    }
                }
                //- Recipient Has No Relationship with Target
                else {
                    if (isRecipientVampire) {
                        //- Recipient is a Vampire
                        RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                        if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
                        } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("I may dislike {0} but I can't report a fellow vampire.", actor.name));
                        } else {
                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
                        }
                    } else {
                        //- Recipient is NOT a Vampire
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add("Vampires are cursed beings that must be destroyed!");
                            } else {
                                reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add("Vampires are cursed beings that must be destroyed!");
                        } else {
                            reactions.Add("Vampires are cursed beings that must be destroyed!");
                        }
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}
