using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Pray : GoapAction {

    public Pray() : base(INTERACTION_TYPE.PRAY) {
        this.goapName = "Pray";
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        actionIconString = GoapActionStateDB.Pray_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Pray Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: randomize between 15 - 55
        return Utilities.rng.Next(15, 56);
    }
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhilePerforming(actor, target, otherData);
        actor.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region State Effects
    public void PrePraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetLonely(1);
    }
    public void PerTickPraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHappiness(400);
    }
    public void AfterPraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (actor.traitContainer.GetNormalTrait("Evil") != null) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}

public class PrayData : GoapActionData {
    public PrayData() : base(INTERACTION_TYPE.PRAY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
}
