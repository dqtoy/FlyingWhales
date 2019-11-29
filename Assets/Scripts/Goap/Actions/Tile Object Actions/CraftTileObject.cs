using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CraftTileObject : GoapAction {

    public CraftTileObject() : base(INTERACTION_TYPE.CRAFT_TILE_OBJECT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Success", goapNode);
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TileObject obj = node.poiTarget as TileObject;
        log.AddToFillers(null, Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 2;
    }
    #endregion

    #region State Effects
    public void PreCraftSuccess(ActualGoapNode goapNode) {
        TileObject obj = goapNode.poiTarget as TileObject;
        goapNode.descriptionLog.AddToFillers(null, Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    public void AfterCraftSuccess(ActualGoapNode goapNode) {
        TileObject tileObj = goapNode.poiTarget as TileObject;
        tileObj.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        goapNode.actor.AdjustResource(RESOURCE.WOOD, -TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost);
    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.supply >= TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).constructionCost;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            TileObject target = poiTarget as TileObject;
            return target.mapObjectState == MAP_OBJECT_STATE.UNBUILT;
        }
        return false;
    }
    #endregion

}

public class CraftTileObjectData : GoapActionData {
    public CraftTileObjectData() : base(INTERACTION_TYPE.CRAFT_TILE_OBJECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.state == POI_STATE.INACTIVE;
    }
}
