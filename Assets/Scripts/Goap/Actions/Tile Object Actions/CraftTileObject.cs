using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CraftTileObject : GoapAction {

    public CraftTileObject() : base(INTERACTION_TYPE.CRAFT_TILE_OBJECT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_WOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Success", goapNode);
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TileObject obj = node.poiTarget as TileObject;
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ": +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.ownParty.RemoveCarriedPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.ownParty.RemoveCarriedPOI();
        (node.poiTarget as TileObject).SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);    
    }
    #endregion

    #region State Effects
    public void PreCraftSuccess(ActualGoapNode goapNode) {
        TileObject obj = goapNode.poiTarget as TileObject;
        if (goapNode.actor.ownParty.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
            int cost = TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost;
            carriedPile.AdjustResourceInPile(-cost);
            goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, cost);
        }
        obj.SetMapObjectState(MAP_OBJECT_STATE.BUILDING);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    public void AfterCraftSuccess(ActualGoapNode goapNode) {
        TileObject tileObj = goapNode.poiTarget as TileObject;
        tileObj.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        //goapNode.actor.AdjustResource(RESOURCE.WOOD, -TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost);
        int cost = TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost;
        goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, -cost);
        //ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
        //carriedPile.AdjustResourceInPile(-TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost);
    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        int cost = TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).constructionCost;
        if (poiTarget.HasResourceAmount(RESOURCE.WOOD, cost)) {
            return true;
        }
        if (actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile) {
            ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
            return carriedPile.resourceInPile >= cost;
        }
        return false;
        //return actor.supply >= TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).constructionCost;
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
