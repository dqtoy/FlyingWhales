using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealFromCharacter : GoapAction {

    private SpecialToken _targetItem;
    private Character _targetCharacter;

    public StealFromCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STEAL_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //    TIME_IN_WORDS.AFTER_MIDNIGHT,
        //};
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _targetCharacter = poiTarget as Character;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        //if (actor.GetNormalTrait("Kleptomaniac") != null) {
        //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        //}
        Character target = poiTarget as Character;
        for (int i = 0; i < target.items.Count; i++) {
            SpecialToken currItem = target.items[i];
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = currItem.specialTokenType.ToString(), targetPOI = actor });
        }
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            if (_targetCharacter.isHoldingItem) {
                SetState("Steal Success");
            } else {
                SetState("Steal Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            return Utilities.rng.Next(5, 46);
        }
        return Utilities.rng.Next(35, 56);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        //exclude characters that the actor knows has no items.
        Kleptomaniac kleptomaniacTrait = actor.GetNormalTrait("Kleptomaniac") as Kleptomaniac;
        if (kleptomaniacTrait != null && kleptomaniacTrait.noItemCharacters.Contains(poiTarget as Character)) {
            return false;
        }
        if (poiTarget != actor) {
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreStealSuccess() {
        _targetItem = _targetCharacter.items[UnityEngine.Random.Range(0, _targetCharacter.items.Count)];
        //**Note**: This is a Theft crime
        SetCommittedCrime(CRIME.THEFT, new Character[] { actor });
        currentState.AddLogFiller(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1);
        currentState.SetIntelReaction(StealSuccessReactions);
    }
    private void AfterStealSuccess() {
        actor.ObtainTokenFrom(_targetCharacter, _targetItem, false);
        if (actor.GetNormalTrait("Kleptomaniac") != null) {
            actor.AdjustHappiness(60);
        }
    }
    private void PreStealFail() {
        Trait trait = actor.GetNormalTrait("Kleptomaniac");
        if (trait != null) {
            Kleptomaniac kleptomaniac = trait as Kleptomaniac;
            kleptomaniac.AddNoItemCharacter(poiTarget as Character);
        }
        currentState.SetIntelReaction(StealFailReactions);
    }
    #endregion

    #region Intel Reactions
    private List<string> StealSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;
        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                if (recipient == actor) {
                    reactions.Add("Yes, I did that.");
                } else {
                    reactions.Add(string.Format("I already know that {0} stole from me.", actor.name));
                }
            } else {
                //- If Recipient is Not Aware
                //- Recipient is Actor
                CHARACTER_MOOD recipientMood = recipient.currentMoodType;
                if (recipient == actor) {
                    if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
                        //- If Negative Mood: "Are you threatening me?!"
                        reactions.Add("Are you threatening me?!");
                    } else {
                        //- If Positive Mood: "Yes I did that."
                        reactions.Add("Yes I did that.");
                    }
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    if(recipient.faction == actor.faction) {
                        //- Same Faction
                        if (!recipient.HasRelationshipWith(actor)){
                            if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
                                //- No Relationship (Negative Mood)
                                reactions.Add(string.Format("{0} stole from me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                                }
                            } else {
                                //- No Relationship (Positive Mood)
                                reactions.Add(string.Format("{0} stole from me? What a horrible person!", actor.name));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                                }
                                recipient.CreateUndermineJobOnly(actor, "informed", status);
                            }
                        } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE){
                            //- Has Negative Relationship
                            reactions.Add(string.Format("That stupid {0} stole from me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                            }
                            recipient.CreateUndermineJobOnly(actor, "informed", status);
                        } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                            //- Has Positive Relationship
                            if (!hasCrimeBeenReported) {
                                if(recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
                                    reactions.Add(string.Format("I should have never trusted {0}!", actor.name));
                                } else {
                                    reactions.Add("Everybody deserves a second chance.");
                                }
                            }
                        }
                    } else {
                        //- Not Same Faction
                        if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                            //- Has Positive Relationship
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("I should have never trusted {0}! {1} will not get away with this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true)));
                                recipient.CreateUndermineJobOnly(actor, "informed", status);
                            } else {
                                reactions.Add("Everybody deserves a second chance.");
                            }
                        } else {
                            //- Has Negative/No Relationship
                            reactions.Add(string.Format("{0} will not get away with this!", actor.name));
                            recipient.CreateUndermineJobOnly(actor, "informed", status);
                        }
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (!hasCrimeBeenReported) {
                            if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
                                reactions.Add(string.Format("{0} is a thief?! I regret that I ever liked {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                            } else {
                                reactions.Add(string.Format("{0} is a thief? I don't believe that.", actor.name));
                            }
                        }
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add(string.Format("{0} is a thief? Why am I not surprised?", actor.name));
                        if (!hasCrimeBeenReported) {
                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                    } else {
                        reactions.Add(string.Format("Poor {0}. {1} must pay.", targetCharacter.name, actor.name));
                        if (!hasCrimeBeenReported) {
                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        reactions.Add(string.Format("It's {0}'s fault for not taking care of {1} possessions.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.POSSESSIVE, false)));
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add("I hate both of them but a crime's a crime.");
                        if (!hasCrimeBeenReported) {
                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                    } else {
                        reactions.Add("I hate both of them but a crime's a crime.");
                        AddTraitTo(recipient, "Cheery");
                    }
                }
                //- Recipient Has No Relationship with Target
                else {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (!hasCrimeBeenReported) {
                            if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
                                reactions.Add(string.Format("{0} is a thief?! I regret that I ever liked {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                            } else {
                                reactions.Add(string.Format("{0} is a thief? I don't believe that.", actor.name));
                            }
                        }
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add(string.Format("{0} is a thief? Why am I not surprised?", actor.name));
                        if (!hasCrimeBeenReported) {
                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                    } else {
                        reactions.Add(string.Format("Poor {0}. {1} must pay.", targetCharacter.name, actor.name));
                        if (!hasCrimeBeenReported) {
                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                    }
                }
            }
        }
        return reactions;
    }

    private List<string> StealFailReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;
        if (awareCharactersOfThisAction.Contains(recipient)) {
            //- If Recipient is Aware
            reactions.Add(string.Format("I already know that {0} attempted to steal from me.", actor.name));
        } else {
            //- If Recipient is Not Aware
            //- Recipient is Actor
            CHARACTER_MOOD recipientMood = recipient.currentMoodType;
            if (recipient == actor) {
                if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
                    //- If Negative Mood: "Are you threatening me?!"
                    reactions.Add("I didn't even get a thing!");
                } else {
                    //- If Positive Mood: "Yes I did that."
                    reactions.Add("Too bad I got nothing.");
                }
            }
            //- Recipient is Target
            else if (recipient == targetCharacter) {
                if (recipient.faction == actor.faction) {
                    //- Same Faction
                    if (!recipient.HasRelationshipWith(actor)) {
                        if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                            reactions.Add(string.Format("{0} tried to steal from me?! What a horrible person!", actor.name));
                        } else {
                            reactions.Add("Good thing I wasn't carrying any item at that time.");
                        }
                    } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE) {
                        //- Has Negative Relationship
                        reactions.Add(string.Format("That stupid {0} attempted to steal from me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                        //- Has Positive Relationship
                        if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                            reactions.Add(string.Format("I should have never trusted {0}!", actor.name));
                        } else {
                            reactions.Add("Relax. Nothing was stolen.");
                        }
                    }
                } else {
                    //- Not Same Faction
                    if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                        //- Has Positive Relationship
                        if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                            reactions.Add(string.Format("I should have never trusted {0}!", actor.name));
                        } else {
                            reactions.Add("Relax. Nothing was stolen.");
                        }
                    } else {
                        //- Has Negative/No Relationship
                        reactions.Add(string.Format("{0} will not get away with this!", actor.name));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    }
                }
            }
            //- Recipient Has Positive Relationship with Target
            else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        reactions.Add(string.Format("{0} is a thief?! I regret that I ever liked {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    } else {
                        reactions.Add(string.Format("{0} is a thief? I don't believe that.", actor.name));
                    }
                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("{0} is a thief? Why am I not surprised?", actor.name));
                } else {
                    reactions.Add("I'm glad nothing of value was taken.");
                }
            }
            //- Recipient Has Negative Relationship with Target
            else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("Nothing was taken from {0}? Too bad.", targetCharacter.name));
                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("Nothing was taken from {0}? {1} is so useless.", targetCharacter.name, actor.name));
                } else {
                    reactions.Add(string.Format("Nothing was taken from {0}? Ugh! Too bad!", targetCharacter.name));
                }
            }
            //- Recipient Has No Relationship with Target
            else {
                RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                        reactions.Add(string.Format("{0} is a thief?! I regret that I ever liked {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    } else {
                        reactions.Add(string.Format("{0} is a thief? I don't believe that.", actor.name));
                    }
                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("{0} is a thief? Why am I not surprised?", actor.name));
                } else {
                    reactions.Add("I'm glad nothing of value was taken.");
                }
            }
        }
        return reactions;
    }
    #endregion
}
