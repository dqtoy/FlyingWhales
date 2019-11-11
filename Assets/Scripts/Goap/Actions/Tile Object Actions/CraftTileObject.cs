using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CraftTileObject : GoapAction {

    public CraftTileObject(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CRAFT_TILE_OBJECT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    TileObject tileObj = poiTarget as TileObject;
    //    TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
    //    int craftCost = (int)(data.constructionCost * 0.5f);
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = craftCost, targetPOI = actor }, () => HasSupply(craftCost));
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burnt", targetPOI = poiTarget });
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Damaged", targetPOI = poiTarget });
    //}
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (poiTarget.gridTileLocation != null) {
            SetState("Craft Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override void AddDefaultObjectsToLog(Log log) {
        base.AddDefaultObjectsToLog(log);
        TileObject obj = poiTarget as TileObject;
        log.AddToFillers(null, Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    protected override int GetCost() {
        return 2;
    }
    #endregion

    #region State Effects
    private void PreCraftSuccess() {
        TileObject obj = poiTarget as TileObject;
        currentState.AddLogFiller(null, Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        currentState.AddLogFiller(null, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    private void AfterCraftSuccess() {
        //poiTarget.traitContainer.RemoveTrait(poiTarget, "Burnt");
        //poiTarget.traitContainer.RemoveTrait(poiTarget, "Damaged");

        //TileObject tileObj = poiTarget as TileObject;
        //TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        //actor.AdjustSupply((int)(data.constructionCost * 0.5f));

        TileObject tileObj = poiTarget as TileObject;
        tileObj.SetPOIState(POI_STATE.ACTIVE);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        return poiTarget.state == POI_STATE.INACTIVE;
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
