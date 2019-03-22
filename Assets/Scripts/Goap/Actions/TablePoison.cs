using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TablePoison : GoapAction {
    public TablePoison(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TABLE_POISON, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Poison Table";
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        //**Effect 1**: Table - Add Trait (Poisoned)
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.ADD_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
        if (poiTarget.gridTileLocation.structure is Dwelling) {
            Dwelling dwelling = poiTarget.gridTileLocation.structure as Dwelling;
            for (int i = 0; i < dwelling.residents.Count; i++) {
                //**Effect 2**: Owner/s - Add Trait (Sick)
                AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.ADD_TRAIT, conditionKey = "Sick", targetPOI = dwelling.residents[i] });
                //**Effect 3**: Kill Owner/s
                AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = dwelling.residents[i] });
            }
        }
        
        
        
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            if (poiTarget.gridTileLocation.structure.charactersHere.Count == 1 && poiTarget.gridTileLocation.structure.charactersHere.Contains(actor)) {
                SetState("Poison Success");
            } else {
                SetState("Poison Fail");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 4;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Poison Fail");
    }
    #endregion

    #region State Effects
    public void PrePoisonSuccess() {
        //**Effect 1**: Add Poisoned Trait to target table
        poiTarget.AddTrait("Poisoned");
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PrePoisonFail() {
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        //**Advertiser**: All Tables inside Dwellings
        return poiTarget.gridTileLocation.structure is Dwelling;
    }
    #endregion
}
