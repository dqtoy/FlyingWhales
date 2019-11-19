using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Strangle : GoapAction {

    public Strangle() : base(INTERACTION_TYPE.STRANGLE) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Override
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Strangle Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 2;
    }
    public override LocationStructure GetTargetStructure(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor.homeStructure != null) {
            return actor.homeStructure;
        } else {
            return actor.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PerTickStrangleSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHP(-(int)((float)goapNode.actor.maxHP * 0.18f));
    }
    public void AfterStrangleSuccess(ActualGoapNode goapNode) {
        goapNode.actor.Death("suicide", goapNode, _deathLog: goapNode.action.states[goapNode.currentStateName].descriptionLog);
    }
    #endregion
}

public class StrangleData : GoapActionData {
    public StrangleData() : base(INTERACTION_TYPE.STRANGLE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}
