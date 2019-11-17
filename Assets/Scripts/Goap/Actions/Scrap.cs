using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Scrap : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Scrap() : base(INTERACTION_TYPE.SCRAP) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //TODO: AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = TokenManager.Instance.itemData[item.specialTokenType].supplyValue, targetPOI = actor });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Scrap Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 5;
    }
    #endregion

    #region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget is SpecialToken) {
                SpecialToken token = poiTarget as SpecialToken;
                if (token.gridTileLocation != null && token.gridTileLocation.structure.location.IsRequiredByWarehouse(token)) {
                    return false;
                }
            }
            if (poiTarget.gridTileLocation != null) {
                if (poiTarget.factionOwner != null) {
                    if (actor.faction == poiTarget.factionOwner) {
                        return true;
                    }
                } else {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreScrapSuccess(ActualGoapNode goapNode) {
        SpecialToken item = goapNode.poiTarget as SpecialToken;
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        currentState.AddLogFiller(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(item, item.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(null, TokenManager.Instance.itemData[item.specialTokenType].supplyValue.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterScrapSuccess(ActualGoapNode goapNode) {
        SpecialToken item = goapNode.poiTarget as SpecialToken;
        goapNode.actor.AdjustSupply(TokenManager.Instance.itemData[item.specialTokenType].supplyValue);
        goapNode.actor.DestroyToken(item);
    }
    #endregion
}

public class ScrapData : GoapActionData {
    public ScrapData() : base(INTERACTION_TYPE.SCRAP) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is SpecialToken) {
            SpecialToken token = poiTarget as SpecialToken;
            if (token.gridTileLocation != null && token.gridTileLocation.structure.location.IsRequiredByWarehouse(token)) {
                return false;
            }
        }
        if (poiTarget.gridTileLocation != null) {
            if (poiTarget.factionOwner != null) {
                if (actor.faction == poiTarget.factionOwner) {
                    return true;
                }
            } else {
                return true;
            }
        }
        return false;
    }
}
