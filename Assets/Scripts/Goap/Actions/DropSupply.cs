using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSupply : GoapAction {
    public DropSupply(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_SUPPLY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = actor }, IsActorSupplyEnough);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = poiTarget });
    }
    public override void Perform() {
        base.Perform();
        if (!isTargetMissing) {
            SetState("Drop Success");
        } else {
            SetState("Target Missing");
        }
        
    }
    protected override int GetBaseCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Preconditions
    private bool IsActorSupplyEnough() {
        if (actor.supply > actor.role.reservedSupply) {
            SupplyPile supplyPile = poiTarget as SupplyPile;
            int supplyToBeDeposited = actor.supply - actor.role.reservedSupply;
            if((supplyToBeDeposited + supplyPile.suppliesInPile) >= 100) {
                return true;
            }
        }
        return false;
    }
    #endregion
    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        return actor.homeArea == poiTarget.gridTileLocation.structure.location;
    }
    #endregion

    #region State Effects
    private void PreDropSuccess() {
        int givenSupply = actor.supply - actor.role.reservedSupply;
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(null, givenSupply.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterDropSuccess() {
        SupplyPile supplyPile = poiTarget as SupplyPile;
        int givenSupply = actor.supply - actor.role.reservedSupply;
        actor.AdjustSupply(-givenSupply);
        supplyPile.AdjustSuppliesInPile(givenSupply);
    }
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}

public class DropSupplyData : GoapActionData {
    public DropSupplyData() : base(INTERACTION_TYPE.DROP_SUPPLY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        return actor.homeArea == poiTarget.gridTileLocation.structure.location;
    }
}
