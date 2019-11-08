using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairTileObject : GoapAction {

    public RepairTileObject(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REPAIR_TILE_OBJECT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        TileObject tileObj = poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        int craftCost = (int)(data.constructionCost * 0.5f);
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = craftCost, targetPOI = actor }, () => HasSupply(craftCost));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burnt", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Damaged", targetPOI = poiTarget });
    }
    public override void Perform() {
        base.Perform();
        if (poiTarget.gridTileLocation != null) {
            SetState("Repair Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetBaseCost() {
        return 2;
    }
    #endregion

    #region State Effects
    private void PreRepairSuccess() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        int gainedHPPerTick = 20;
        int missingHP = poiTarget.maxHP - poiTarget.currentHP;
        int ticksToRecpverMissingHP = missingHP / gainedHPPerTick;
        currentState.OverrideDuration(ticksToRecpverMissingHP);
    }
    private void PerTickRepairSuccess() {
        poiTarget.AdjustHP(20);
    }
    private void AfterRepairSuccess() {
        poiTarget.RemoveTrait("Burnt");
        poiTarget.RemoveTrait("Damaged");

        TileObject tileObj = poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        actor.AdjustSupply((int) (data.constructionCost * 0.5f));
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

}

public class RepairTileObjectData : GoapActionData {
    public RepairTileObjectData() : base(INTERACTION_TYPE.REPAIR_TILE_OBJECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        //requirementAction = Requirement;
    }

    //private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    return actor == poiTarget;
    //}
}
