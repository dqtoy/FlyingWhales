using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CraftTileObject : GoapAction {

    public CraftTileObject() : base(INTERACTION_TYPE.CRAFT_TILE_OBJECT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    //#region Overrides
    //public override void Perform(ActualGoapNode goapNode) {
    //    base.Perform(goapNode);
    //    if (poiTarget.gridTileLocation != null) {
    //        SetState("Craft Success");
    //    } else {
    //        SetState("Target Missing");
    //    }
    //}
    //protected override void AddFillersToLog(Log log) {
    //    base.AddFillersToLog(log);
    //    TileObject obj = poiTarget as TileObject;
    //    log.AddToFillers(null, Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
    //    log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    //}
    //protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
    //    return 2;
    //}
    //#endregion

    //#region State Effects
    //private void PreCraftSuccess() {
    //    TileObject obj = poiTarget as TileObject;
    //    currentState.AddLogFiller(null, Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
    //    currentState.AddLogFiller(null, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    //}
    //private void AfterCraftSuccess() {
    //    //poiTarget.traitContainer.RemoveTrait(poiTarget, "Burnt");
    //    //poiTarget.traitContainer.RemoveTrait(poiTarget, "Damaged");

    //    //TileObject tileObj = poiTarget as TileObject;
    //    //TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
    //    //actor.AdjustSupply((int)(data.constructionCost * 0.5f));

    //    TileObject tileObj = poiTarget as TileObject;
    //    tileObj.SetPOIState(POI_STATE.ACTIVE);
    //}
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    //#endregion

    //#region Requirement
    //private bool Requirement() {
    //    return poiTarget.state == POI_STATE.INACTIVE;
    //}
    //#endregion

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
