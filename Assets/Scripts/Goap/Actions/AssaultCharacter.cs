using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultCharacter : GoapAction {

    private Character winner;
    private Character loser;

    public AssaultCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASSAULT_ACTION_NPC, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    protected override void MoveToDoAction(Character targetCharacter) {
        base.MoveToDoAction(targetCharacter);
        //change end reached distance to the characters attack range. NOTE: Make sure to return the range to default after this action is done.
        actor.marker.pathfindingAI.SetEndReachedDistance(actor.characterClass.attackRange);
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        cannotCancelAction = true;
        actor.marker.pathfindingAI.ResetEndReachedDistance();
        
        Character targetCharacter = poiTarget as Character;
        if (targetCharacter.specificLocation == actor.specificLocation && !targetCharacter.currentParty.icon.isAreaTravelling) {
            CharacterState combatState;
            if (!actor.marker.hostilesInRange.Contains(targetCharacter)) {
                actor.marker.AddHostileInRange(targetCharacter, out combatState, CHARACTER_STATE.COMBAT, false);
            } else {
                combatState = actor.stateComponent.currentState as CombatState; //target character is already in the actor's hostile range so I assume that the actor is in combat state
            }
            if (combatState is CombatState) {
                (combatState as CombatState).SetOnEndStateAction(OnFinishCombatState);
                SetState("In Progress");
            } else {
                Debug.LogWarning(GameManager.Instance.TodayLogString() + actor.name + " did not return a combat state when reacting to " + poiTarget.name + " in assault action!");
                SetState("Target Missing");
            }
        } else {
            SetState("Target Missing");
        }
                
        //Character targetCharacter = poiTarget as Character;
        //if (!isTargetMissing && targetCharacter.IsInOwnParty() && !targetCharacter.isDead) {

        //    float attackersChance = 0f;
        //    float defendersChance = 0f;

        //    CombatManager.Instance.GetCombatChanceOfTwoLists(new List<Character>() { actor }, new List<Character>() { targetCharacter }, out attackersChance, out defendersChance);

        //    string nextState = CombatEncounterEvents(actor, targetCharacter, UnityEngine.Random.Range(0, 100) < attackersChance);
        //    if (nextState == "Target Killed") {
        //        parentPlan.SetDoNotRecalculate(true);
        //    }
        //    SetState(nextState);
        //} else {
        //    SetState("Target Missing");
        //}

    }
    protected override void OnCancelActionTowardsTarget() {
        actor.marker.pathfindingAI.ResetEndReachedDistance();
        base.OnCancelActionTowardsTarget();
    }
    private void OnFinishCombatState() {
        //TODO: Add checking if poi target has become unconscious. If yes action was a success.
        Debug.Log(actor.name + " finished combat state!");
        Character target = poiTarget as Character;
        loser = target;
        winner = actor; // TODO: How to determine if actor won?
        if (target.GetNormalTrait("Dead") != null) {
            SetState("Target Killed");
        } else if (target.GetNormalTrait("Unconscious") != null) {
            SetState("Target Knocked Out");
        } else if (target.GetNormalTrait("Injured") != null) {
            SetState("Target Injured");
        } else if (actor.specificLocation != target.specificLocation) {
            SetState("Target Missing");
        } else {
            loser = actor;
            winner = target;
            SetState("Assault Failed");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override void OnStopActionDuringCurrentState() {
        actor.marker.pathfindingAI.ResetEndReachedDistance();
    }
    public override void OnResultReturnedToActor() {
        actor.marker.pathfindingAI.ResetEndReachedDistance();
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(poiTarget is Character && actor != poiTarget) {
            if (!actor.IsCombatReady()) { //a character that will flee when seeing a hostile character, should not do this action.
                return false;
            }
            Character target = poiTarget as Character;
            if(!target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreTargetInjured() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
        if (!actor.IsHostileWith(poiTarget as Character)
           //Assaulting a criminal as part of apprehending him should not be considered a crime
           && !IsFromApprehendJob()) {
            SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        }
        currentState.AddLogFiller(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
        //AddTraitTo(winner, "Combat Recovery", loser);
        //Injured injured = new Injured();
        //AddTraitTo(loser, injured, winner);
        CharacterManager.Instance.RelationshipDegradation(actor, poiTarget as Character, this);
        currentState.SetIntelReaction(TargetInjuredKnockOutReactions);
    }
    public void AfterTargetInjured() {
        //moved this to pre effect, because if put here, will cause infinite loop:
        // - loser will gain injured trait
        // - if the loser is the actor of this action
        // - will switch to flee state
        // - which will then stop the current action which is this, but still perform the after action
        // - so this loop will never end.

        //Injured injured = new Injured();
        //AddTraitTo(loser, injured, winner);
    }
    public void PreTargetKnockedOut() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
        if (!actor.IsHostileWith(poiTarget as Character)
            //Assaulting a criminal as part of apprehending him should not be considered a crime
            && !IsFromApprehendJob()) {
            SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        }
        currentState.AddLogFiller(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
        //AddTraitTo(winner, "Combat Recovery", loser);
        CharacterManager.Instance.RelationshipDegradation(actor, poiTarget as Character, this);
        currentState.SetIntelReaction(TargetInjuredKnockOutReactions);
    }
    public void AfterTargetKnockedOut() {
        if(parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        resumeTargetCharacterState = false; //do not resume the target character's current state after being knocked out
        //Character target = poiTarget as Character;
        //Unconscious unconscious = new Unconscious();
        //AddTraitTo(loser, unconscious, winner);
    }
    public void PreTargetKilled() {
        //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is a Murder crime
        if (!actor.IsHostileWith(poiTarget as Character)
            //Assaulting a criminal as part of apprehending him should not be considered a crime
            && !IsFromApprehendJob()) {
            SetCommittedCrime(CRIME.MURDER, new Character[] { actor });
        }
        currentState.AddLogFiller(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
        //AddTraitTo(winner, "Combat Recovery", loser);
        currentState.SetIntelReaction(TargetKilledReactions);
    }
    public void AfterTargetKilled() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        //loser.Death(deathFromAction: this);
    }
    //public void PreAssaultFailed() {
    //    currentState.AddLogFiller(poiTarget as Character, (poiTarget as Character).name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    #region Combat
    private string CombatEncounterEvents(Character actor, Character target, bool actorWon) {
        //Reference: https://trello.com/c/uY7JokJn/1573-combat-encounter-event

        if (actorWon) {
            winner = actor;
            loser = target;
        } else {
            winner = target;
            loser = actor;
        }

        //**Character That Lost**
        //40 Weight: Gain Unconscious trait (reduce to 0 if already Unconscious)
        //10 Weight: Gain Injured trait and enter Flee mode (reduce to 0 if already Injured)
        //5 Weight: death
        WeightedDictionary<string> loserResults = new WeightedDictionary<string>();
        if (loser.GetNormalTrait("Unconscious") == null) {
            loserResults.AddElement("Unconscious", 40);
        }
        if (loser.GetNormalTrait("Injured") == null) {
            loserResults.AddElement("Injured", 10);
        }
        loserResults.AddElement("Death", 5);

        string result = loserResults.PickRandomElementGivenWeights();
        switch (result) {
            case "Unconscious":
                return "Target Knocked Out";
            case "Injured":
                return "Target Injured";
            case "Death":
                return "Target Killed";
        }
        throw new System.Exception("No state for result " + result);
    }
    #endregion

    #region Intel Reactions
    private List<string> TargetInjuredKnockOutReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            CHARACTER_MOOD recipientMood = recipient.currentMoodType;
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
                    if (!recipient.HasRelationshipWith(actor)) {
                        reactions.Add("Please don't remind me of that altercation.");
                        AddTraitTo(recipient, "Annoyed");
                    } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE) {
                        //- Has Negative Relationship
                        reactions.Add(string.Format("Now that you've reminded me about that, I think I should get back at {0}.", actor.name));
                        recipient.CreateUndermineJobOnly(actor, "informed", status);
                    } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                        //- Has Positive Relationship
                        if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
                            //- No Relationship (Negative Mood)
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("Now that you've reminded me about that, I think I really should start avoiding {0}!", actor.name));
                            } else {
                                reactions.Add("Past is past.");
                            }
                        } else {
                            //- No Relationship (Positive Mood)
                            reactions.Add("Past is past.");
                        }
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (actor.IsHostileWith(targetCharacter)) {
                        reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
                    } else {
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add(string.Format("I may have been fond of {0} but I can't allow such violence!", actor.name));
                            } else {
                                reactions.Add(string.Format("{0} assaulted {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("{0} assaulted {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                        } else {
                            reactions.Add(string.Format("Poor {1}. {0} must pay.", actor.name, targetCharacter.name));
                        }
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        reactions.Add(string.Format("{0} deserves to be beaten.", targetCharacter.name));
                        AddTraitTo(recipient, "Cheery");
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    if (recipient.marker.inVisionPOIs.Contains(targetCharacter)) {
                        //        recipient.marker.AddHostileInRange(targetCharacter, checkHostility: false);
                        //    }
                        //}
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add("Those misfits are always up to no good.");
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
                        //        recipient.marker.AddAvoidInRange(actor);
                        //    }
                        //}
                    } else {
                        reactions.Add(string.Format("{0} deserves to be beaten.", targetCharacter.name));
                        AddTraitTo(recipient, "Cheery");
                    }
                }
                //- Recipient Has No Relationship with Target
                else {
                    if (actor.IsHostileWith(targetCharacter)) {
                        reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
                    } else {
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add(string.Format("I may have been fond of {0} but I can't allow such violence!", actor.name));
                            } else {
                                reactions.Add(string.Format("{0} assaulted {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("{0} assaulted {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                        } else {
                            reactions.Add(string.Format("Poor {1}. {0} must pay.", actor.name, targetCharacter.name));
                        }
                    }
                }
            }
        }
        return reactions;
    }
    private List<string> TargetKilledReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
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
                    reactions.Add("I know what I did.");
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    reactions.Add("That altercation ended my past life.");
                    AddTraitTo(recipient, "Heartbroken");
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (actor.IsHostileWith(targetCharacter)) {
                        reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
                    } else {
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add(string.Format("I may have been fond of {0} but I can't condone murder!", actor.name));
                            } else {
                                reactions.Add(string.Format("{0} killed {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("{0} murdered {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                        } else {
                            reactions.Add(string.Format("{1} was killed? {0} must pay.", actor.name, targetCharacter.name));
                        }
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        reactions.Add(string.Format("Suits {0} right.", Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                        AddTraitTo(recipient, "Cheery");
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
                        //        recipient.marker.AddAvoidInRange(actor);
                        //    }
                        //}
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add("My enemies are killing each other. Isn't that funny?");
                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
                        //        recipient.marker.AddAvoidInRange(actor);
                        //    }
                        //}
                    } else {
                        if (CharacterManager.Instance.RelationshipImprovement(actor, recipient, this)) {
                            reactions.Add(string.Format("I am happy {0} was able to do what I cannot.", actor.name));
                            //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
                            //        recipient.marker.AddAvoidInRange(actor);
                            //    }
                            //}
                        } else {
                            reactions.Add(string.Format("Though I dislike {1}, I am appalled at {0}'s actions.", actor.name, targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                            }
                            //if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
                            //        recipient.marker.AddAvoidInRange(actor);
                            //    }
                            //}
                        }
                        AddTraitTo(recipient, "Cheery");
                    }
                }
                //- Recipient Has No Relationship with Target
                else {
                    if (actor.IsHostileWith(targetCharacter)) {
                        reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
                    } else {
                        RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                        bool hasRelationshipDegraded = false;
                        if (!hasCrimeBeenReported) {
                            hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                        }
                        if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                            if (hasRelationshipDegraded) {
                                reactions.Add(string.Format("I may be fond of {0} but I can't condone murder!", actor.name));
                            } else {
                                reactions.Add(string.Format("{0} killed {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                            reactions.Add(string.Format("{0} is truly wicked.", actor.name));
                        } else {
                            reactions.Add(string.Format("Poor {1}. {0} must pay.", actor.name, targetCharacter.name));
                        }
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}
