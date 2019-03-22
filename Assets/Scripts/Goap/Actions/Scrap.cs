﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;
    public Scrap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SCRAP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        SpecialToken item = poiTarget as SpecialToken;
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = ItemManager.Instance.itemData[item.specialTokenType].supplyValue, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Scrap Success");
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
        if (poiTarget.gridTileLocation != null) {
            if (poiTarget.factionOwner != null) {
                if (actor.faction.id == poiTarget.factionOwner.id) {
                    return true;
                }
            } else {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreScrapSuccess() {
        SpecialToken item = poiTarget as SpecialToken;
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(item, item.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(null, ItemManager.Instance.itemData[item.specialTokenType].supplyValue.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterScrapSuccess() {
        SpecialToken item = poiTarget as SpecialToken;
        actor.AdjustSupply(ItemManager.Instance.itemData[item.specialTokenType].supplyValue);
        actor.DestroyToken(item);
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