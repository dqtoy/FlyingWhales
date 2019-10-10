using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySupply : GoapAction {

    private int destroyedSupply;

    public DestroySupply(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DESTROY_SUPPLY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
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
            SupplyPile supplyPile = poiTarget as SupplyPile;
            if (supplyPile.suppliesInPile > 0) {
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
        if (poiTarget is SupplyPile) {
            SupplyPile supplyPile = poiTarget as SupplyPile;
            if (supplyPile.suppliesInPile > 0) {
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
        SupplyPile supplyPile = poiTarget as SupplyPile;
        destroyedSupply = Mathf.FloorToInt((float)supplyPile.suppliesInPile * (UnityEngine.Random.Range(0.25f, 0.5f)));
        currentState.AddLogFiller(null, destroyedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterDestroySuccess() {
        SupplyPile supplyPile = poiTarget as SupplyPile;
        supplyPile.AdjustSuppliesInPile(-destroyedSupply);
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

public class DestroySupplyData : GoapActionData {
    public DestroySupplyData() : base(INTERACTION_TYPE.DESTROY_SUPPLY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        LocationGridTile knownLoc = poiTarget.gridTileLocation;
        if (poiTarget is SupplyPile) {
            SupplyPile supplyPile = poiTarget as SupplyPile;
            if (supplyPile.suppliesInPile > 0) {
                return true;
            }
        }
        return false;
    }
}
