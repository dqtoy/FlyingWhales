using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseCharacter : GoapAction {

    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;
    private Log actorLog;
    //private Log targetLog;

    public CurseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CURSE_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        shouldAddLogs = false; //set to false because this action has a special case for logs
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return actor.gridTileLocation; //in place
    }
    public override void SetTargetStructure() {
        _targetStructure = actor.currentStructure;
        base.SetTargetStructure();
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Ritualized", targetPOI = actor }, () => HasTrait(actor, "Ritualized"));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Cursed", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });

        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
        //} else {
        //    SetState("Target Missing");
        //}
        SetState("Curse Success");
    }
    protected override int GetCost() {
        return 3;
    }
    protected override void MoveToDoAction(Character targetCharacter) {
        if (actor.specificLocation != targetStructure.location) {
            actor.currentParty.GoToLocation(targetStructure.location.region, PATHFINDING_MODE.NORMAL, targetStructure, () => actor.PerformGoapAction(), null, poiTarget, targetTile);
        } else {
            //if the actor is already at the area where the target structure is, immediately do the action, since this action is performed in place
            actor.PerformGoapAction();
        }
    }
    #endregion

    #region State Effects
    public void PreCurseSuccess() {
        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        actorLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), currentState.name.ToLower() + "_description_actor", this);
        actorLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        actorLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        targetLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), currentState.name.ToLower() + "_description_target", this);
        targetLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        currentState.OverrideDescriptionLog(actorLog);
        currentState.SetIntelReaction(CurseSuccessReactions);
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);
    }
    public void AfterCurseSuccess() {
        actorLog.SetDate(GameManager.Instance.Today());
        targetLog.SetDate(GameManager.Instance.Today());

        actor.AddHistory(actorLog);
        (poiTarget as Character).AddHistory(targetLog);

        //**After Effect 1**: Target gains Cursed trait.
        Cursed cursed = new Cursed();
        AddTraitTo(poiTarget, cursed, actor);
        //**After Effect 2**: Actor loses Ritualized trait.
        RemoveTraitFrom(actor, "Ritualized");
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region Intel Reactions
    public List<string> CurseSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
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
                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                    bool hasRelationshipDegraded = false;
                    if (!hasCrimeBeenReported) {
                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                    }
                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (hasRelationshipDegraded) {
                            reactions.Add(string.Format("So it was {0} who did that to me. I should start avoiding {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                        } else {
                            reactions.Add(string.Format("I forgave {0} already.", actor.name));
                        }
                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add(string.Format("So it was {0} who did that to me. I should get back at {1}.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                        if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed", status);
                        }
                    } else {
                        reactions.Add(string.Format("Why did {0} do that to me?", actor.name));
                        AddTraitTo(recipient, "Annoyed");
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                    bool hasRelationshipDegraded = false;
                    if (!hasCrimeBeenReported) {
                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                    }
                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (hasRelationshipDegraded) {
                            reactions.Add(string.Format("{0} shouldn't have done that to {1}. I should start avoiding {2}.", actor.name, targetCharacter.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                        } else {
                            reactions.Add("Everyone makes mistakes.");
                        }
                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add(string.Format("Why did {0} do that to {1}? I should get back at {2}.", actor.name, targetCharacter.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                        if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed", status);
                        }
                    } else {
                        reactions.Add(string.Format("Why did {0} do that to {1}?", actor.name, targetCharacter.name));
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("Serves {0} right.", targetCharacter.name));
                }
                //- Recipient Has No Relationship with Target
                else {
                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.GetRelationshipEffectWith(actor);
                    bool hasRelationshipDegraded = false;
                    if (!hasCrimeBeenReported) {
                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
                    }
                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (hasRelationshipDegraded) {
                            reactions.Add(string.Format("{0} shouldn't have done that to {1}. I should start avoiding {2}.", actor.name, targetCharacter.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                        } else {
                            reactions.Add(string.Format("{0} probably has {1} reason for doing that.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.POSSESSIVE, false)));
                        }
                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
                        reactions.Add(string.Format("{0} is up to no good again.", actor.name));
                    } else {
                        reactions.Add(string.Format("Why did {0} do that to {1}?", actor.name, targetCharacter.name));
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class CurseCharacterData : GoapActionData {
    public CurseCharacterData() : base(INTERACTION_TYPE.CURSE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}