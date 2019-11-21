using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Repair : GoapAction {

    public Repair() : base(INTERACTION_TYPE.REPAIR) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        canBeAdvertisedEvenIfActorIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override List<Precondition> GetPreconditions(IPointOfInterest poiTarget, object[] otherData) {
        List <Precondition> p = new List<Precondition>(base.GetPreconditions(poiTarget, otherData));
        TileObject tileObj = poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        int craftCost = (int)(data.constructionCost * 0.5f);
        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, craftCost.ToString(), true, GOAP_EFFECT_TARGET.ACTOR), HasSupply));

        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Repair Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 2;
    }
    #endregion

    #region State Effects
    public void PreRepairSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        int gainedHPPerTick = 20;
        int missingHP = goapNode.poiTarget.maxHP - goapNode.poiTarget.currentHP;
        int ticksToRecpverMissingHP = missingHP / gainedHPPerTick;
        //TODO: currentState.OverrideDuration(ticksToRecpverMissingHP);
    }
    public void PerTickRepairSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.AdjustHP(20);
    }
    public void AfterRepairSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Burnt");
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Damaged");

        TileObject tileObj = goapNode.poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        goapNode.actor.AdjustSupply((int) (data.constructionCost * 0.5f));
    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        TileObject tileObj = poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        int craftCost = (int)(data.constructionCost * 0.5f);
        return actor.supply >= craftCost;
    }
    #endregion

}

public class RepairData : GoapActionData {
    public RepairData() : base(INTERACTION_TYPE.REPAIR) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        //requirementAction = Requirement;
    }

    //private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    return actor == poiTarget;
    //}
}
