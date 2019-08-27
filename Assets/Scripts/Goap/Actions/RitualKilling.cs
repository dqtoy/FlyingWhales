using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitualKilling : GoapAction {

    public RitualKilling(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RITUAL_KILLING, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //rather than checking location check if the character is not in anyone elses party and is still active
        if (!isTargetMissing) {
            if (actor.marker.CanDoStealthActionToTarget(poiTarget)) {
                SetState("Killing Success");
            } else {
                SetState("Killing Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget && actor.GetNormalTrait("Serial Killer") != null;
    }
    #endregion

    #region State Effects
    private void PreKillingSuccess() {
        SetCommittedCrime(CRIME.MURDER, new Character[] { actor });
    }
    private void AfterKillingSuccess() {
        actor.AdjustHappiness(10000);
        if (poiTarget is Character) {
            SetCannotCancelAction(true);
            Character targetCharacter = poiTarget as Character;
            targetCharacter.Death(deathFromAction: this, responsibleCharacter: actor);
        }
    }
    #endregion
}

public class RitualKillingData : GoapActionData {
    public RitualKillingData() : base(INTERACTION_TYPE.RITUAL_KILLING) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && actor.GetNormalTrait("Serial Killer") != null;
    }
}

