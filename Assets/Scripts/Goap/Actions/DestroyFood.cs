using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFood : GoapAction {

    private int destroyed;

    public DestroyFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DESTROY_FOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        isNotificationAnIntel = false;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        SetIsStealth(true);
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            FoodPile pile = poiTarget as FoodPile;
            if (pile.foodInPile > 0) {
                SetState("Destroy Success");
            } else {
                SetState("Destroy Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        LocationGridTile knownLoc = poiTarget.gridTileLocation;
        if (poiTarget is FoodPile) {
            FoodPile pile = poiTarget as FoodPile;
            if (pile.foodInPile > 0) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreDestroySuccess() {
        currentState.SetIntelReaction(DestroySuccessReactions);
        SetCommittedCrime(CRIME.HERETIC, new Character[] { actor });
        FoodPile pile = poiTarget as FoodPile;
        destroyed = Mathf.FloorToInt((float)pile.foodInPile * (UnityEngine.Random.Range(0.25f, 0.5f)));
        currentState.AddLogFiller(null, destroyed.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterDestroySuccess() {
        FoodPile pile = poiTarget as FoodPile;
        pile.AdjustFoodInPile(-destroyed);
    }
    #endregion

    #region Intel Reactions
    private List<string> DestroySuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        if (status == SHARE_INTEL_STATUS.WITNESSED) {
            if (recipient != actor) {
                recipient.ReactToCrime(CRIME.HERETIC, this, actorAlterEgo, status);
            }
        }
        return reactions;
    }
    #endregion
}

public class DestroyFoodData : GoapActionData {
    public DestroyFoodData() : base(INTERACTION_TYPE.DESTROY_FOOD) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        LocationGridTile knownLoc = poiTarget.gridTileLocation;
        if (poiTarget is FoodPile) {
            FoodPile pile = poiTarget as FoodPile;
            if (pile.foodInPile > 0) {
                return true;
            }
        }
        return false;
    }
}