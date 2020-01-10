using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RitualKilling : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public RitualKilling() : base(INTERACTION_TYPE.RITUAL_KILLING) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
                TIME_IN_WORDS.AFTER_MIDNIGHT,
                TIME_IN_WORDS.EARLY_NIGHT,
                TIME_IN_WORDS.LATE_NIGHT,
        };
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Killing Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (actor.marker.CanDoStealthActionToTarget(poiTarget) == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Killing Fail";
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && actor.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreKillingSuccess(ActualGoapNode goapNode) {
        
    }
    public void AfterKillingSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustHappiness(10000);
        if (goapNode.poiTarget is Character) {
            Character targetCharacter = goapNode.poiTarget as Character;
            targetCharacter.Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
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
        return actor != poiTarget && actor.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null;
    }
}

