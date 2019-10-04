using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductCharacter : GoapAction {
    public AbductCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ABDUCT_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //_isStealthAction = true;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget }, HasRestrainedTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY_WITHOUT_CONSENT, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            if ((poiTarget as Character).IsInOwnParty()) {
                SetState("In Progress");
            } else {
                SetState("Abduct Success");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion


    #region Preconditions
    private bool HasRestrainedTrait() {
        Character target = poiTarget as Character;
        return target.GetNormalTrait("Restrained") != null;
    }
    #endregion

    #region State Effects
    public void PreInProgress() {
        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
    }
    public void PreAbductSuccess() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.SetIntelReaction(AbductSuccessIntelReaction);
    }
    public void AfterAbductSuccess() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        Character target = poiTarget as Character;
        Restrained restrainedTrait = new Restrained();
        target.AddTrait(restrainedTrait, actor);

        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Intel Reactions
    private List<string> AbductSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
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
                    reactions.Add("Please help me!");
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("I want to save {0} from {1} but I need to know where {2} was taken.", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        if (recipient.marker.inVisionCharacters.Contains(actor)) {
                            recipient.marker.AddHostileInRange(actor, checkHostility: false);
                        }
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    reactions.Add(string.Format("{0} deserves what {1} got.", targetCharacter.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                    AddTraitTo(recipient, "Satisfied");
                }
                //- Recipient Has No Relationship with Target
                else {
                    reactions.Add(string.Format("Poor {0}. If you find out where {1} took {2}, I may be able to help.", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
                        if (recipient.marker.inVisionCharacters.Contains(actor)) {
                            recipient.marker.AddHostileInRange(actor, checkHostility: false);
                        }
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class AbductCharacterData: GoapActionData {
    public AbductCharacterData() : base(INTERACTION_TYPE.ABDUCT_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            //Character target = poiTarget as Character;
            //return target.GetNormalTrait("Restrained") == null;
            return true;
        }
        return false;
    }
}