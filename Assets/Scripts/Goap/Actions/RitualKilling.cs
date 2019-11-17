using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RitualKilling : GoapAction {

    public RitualKilling(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RITUAL_KILLING, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
                TIME_IN_WORDS.AFTER_MIDNIGHT,
                TIME_IN_WORDS.EARLY_NIGHT,
                TIME_IN_WORDS.LATE_NIGHT,
            };
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = poiTarget });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
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
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    #endregion

    #region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        return actor != poiTarget && actor.traitContainer.GetNormalTrait("Serial Killer") != null;
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
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && actor.traitContainer.GetNormalTrait("Serial Killer") != null;
    }
}

