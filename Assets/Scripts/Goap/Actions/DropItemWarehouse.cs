using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemWarehouse : GoapAction {

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private SPECIAL_TOKEN itemTypeToDeposit;
    private SpecialToken tokenToDeposit;

    public DropItemWarehouse(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void AddFillersToLog(Log log) {
        base.AddFillersToLog(log);
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(itemTypeToDeposit.ToString()), LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = itemTypeToDeposit.ToString(), targetPOI = actor }, HasItemTypeInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_ITEM, conditionKey = poiTarget, targetPOI = actor });
    }
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override void Perform() {
        base.Perform();
        SetState("Drop Success");
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(ACTION_LOCATION_TYPE.RANDOM_LOCATION, actor, null, targetStructure);
    }
    protected override int GetBaseCost() {
        return 1;
    }
    public override void SetTargetStructure() {
        _targetStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        base.SetTargetStructure();
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is SPECIAL_TOKEN) {
            itemTypeToDeposit = (SPECIAL_TOKEN) otherData[0];
            preconditions.Clear();
            expectedEffects.Clear();
            ConstructPreconditionsAndEffects();
            CreateThoughtBubbleLog();
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        //if (_targetStructure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
        //    _targetStructure.location.CheckAreaInventoryJobs(_targetStructure);
        //}
    }
    #endregion

    #region State Effects
    private void PreDropSuccess() {
        tokenToDeposit = actor.GetToken(itemTypeToDeposit);
        currentState.AddLogFiller(tokenToDeposit, tokenToDeposit.name, LOG_IDENTIFIER.ITEM_1);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterDropSuccess() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        //**Effect 1**: Actor loses item, add target item to tile. Clear personal owner of the item.
        LocationGridTile targetLocation = actor.gridTileLocation;
        if (targetLocation.isOccupied) {
            targetLocation = actor.currentStructure.GetRandomUnoccupiedTile();
        }
        actor.DropToken(tokenToDeposit, actor.specificLocation, actor.currentStructure, targetLocation);
        if (_targetStructure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            _targetStructure.location.CheckAreaInventoryJobs(_targetStructure);
        }
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        if (actor != poiTarget) {
            return false;
        }
        //if (!actor.HasToken(itemTypeToDeposit)) {
        //    return false;
        //}
        //there must still be an unoccupied tile in the target warehouse
        LocationStructure warehouse = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        if (warehouse == null) {
            return false;
        }
        return warehouse.unoccupiedTiles.Count > 0;
    }
    #endregion

    #region Preconditions
    private bool HasItemTypeInInventory() {
        if (parentPlan != null && parentPlan.job != null && parentPlan.job.jobQueueParent.isAreaOrQuestJobQueue) {
            //if this action is part of an area job, only check the actors actual inventory instead of taking into account his/her role required items.
            return actor.HasTokenInInventory(itemTypeToDeposit);
        }
        return actor.HasExtraTokenInInventory(itemTypeToDeposit);
        //return true;
    }
    #endregion
}

public class DropItemWarehouseData : GoapActionData {
    public DropItemWarehouseData() : base(INTERACTION_TYPE.DROP_ITEM_WAREHOUSE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            return false;
        }
        //there must still be an unoccupied tile in the target warehouse
        LocationStructure warehouse = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        if (warehouse == null) {
            return false;
        }
        return warehouse.unoccupiedTiles.Count > 0;
    }
}
