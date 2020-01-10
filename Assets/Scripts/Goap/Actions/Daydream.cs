using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Daydream : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Daydream() : base(INTERACTION_TYPE.DAYDREAM) {
        shouldIntelNotificationOnlyIfActorIsActive = true;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
        };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Daydream Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: randomize between 10-30
        return Utilities.rng.Next(10, 31);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetLonely(-1);
        actor.needsComponent.AdjustDoNotGetTired(-1);
    }
    #endregion

    #region Effects
    public void PreDaydreamSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetLonely(1);
        goapNode.actor.needsComponent.AdjustDoNotGetTired(1);
    }
    public void PerTickDaydreamSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(3.35f);
        goapNode.actor.needsComponent.AdjustComfort(1f);
    }
    public void AfterDaydreamSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetLonely(-1);
        goapNode.actor.needsComponent.AdjustDoNotGetTired(-1);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (actor.traitContainer.GetNormalTrait<Trait>("Disillusioned") != null) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}

public class DaydreamData : GoapActionData {
    public DaydreamData() : base(INTERACTION_TYPE.DAYDREAM) {
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
