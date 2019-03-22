using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steal : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;
    public Steal(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STEAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
            TIME_IN_WORDS.AFTER_MIDNIGHT,
        };
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if(poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Steal Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 2;
    }
    public override void SetTargetStructure() {
        ItemAwareness awareness = actor.GetAwareness(poiTarget) as ItemAwareness;
        _targetStructure = awareness.knownLocation.structure;
        targetTile = awareness.knownLocation;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(poiTarget.gridTileLocation != null && !actor.isHoldingItem && poiTarget.factionOwner != null) {
            if(actor.faction.id != poiTarget.factionOwner.id) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreStealSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void AfterStealSuccess() {
        actor.PickUpToken(poiTarget as SpecialToken);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
