using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockoutCharacter : GoapAction {

    public KnockoutCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.KNOCKOUT_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
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
        if(currentState.name == "Knockout Fail") {
            if (poiTarget is Character) {
                Character targetCharacter = poiTarget as Character;
                targetCharacter.marker.AddHostileInRange(actor);
            }
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget && actor.GetNormalTrait("Serial Killer") != null;
    }
    #endregion

    #region State Effects
    public void AfterKnockoutSuccess() {
        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        poiTarget.AddTrait("Unconscious", actor, gainedFromDoing: this);
    }
    public void AfterKnockoutFail() {
        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
        if(poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            if (!targetCharacter.ReactToCrime(committedCrime, this, actorAlterEgo, SHARE_INTEL_STATUS.WITNESSED)) {
                CharacterManager.Instance.RelationshipDegradation(actor, targetCharacter, this);

                //NOTE: Adding hostile in range is done after the action is done processing fully, See OnResultReturnedToActor
            }
        }
    }
    #endregion
}
