using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceTileObject : GoapAction {

    private TileObject tileObjectToReplace;
    private LocationGridTile whereToPlace;

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    public ReplaceTileObject(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REPLACE_TILE_OBJECT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        tileObjectToReplace = null;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void AddDefaultObjectsToLog(Log log) {
        base.AddDefaultObjectsToLog(log);
        if (tileObjectToReplace != null) {
            log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(tileObjectToReplace.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        }
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (tileObjectToReplace != null) {
            TileObjectData data = TileObjectDB.GetTileObjectData(tileObjectToReplace.tileObjectType);
            AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = data.constructionCost, targetPOI = actor }, () => HasSupply(data.constructionCost));
        }
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION., conditionKey = poiTarget, targetPOI = actor });
    }
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //public override void DoAction(GoapPlan plan) {
    //    SetTargetStructure();
    //    base.DoAction(plan);
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Replace Success");
        } else {
            SetState("Target Missing");
        }
    }
    public override LocationGridTile GetTargetLocationTile() {
        return whereToPlace;
    }
    protected override int GetCost() {
        return 1;
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 2 && otherData[0] is TileObject && otherData[1] is LocationGridTile) {
            tileObjectToReplace = (TileObject) otherData[0];
            whereToPlace = (LocationGridTile) otherData[1];
            _targetStructure = whereToPlace.structure;
            SetTargetStructure();
            preconditions.Clear();
            expectedEffects.Clear();
            ConstructPreconditionsAndEffects();
            CreateThoughtBubbleLog();
            states["Replace Success"].OverrideDuration(TileObjectDB.GetTileObjectData(tileObjectToReplace.tileObjectType).constructionTime);
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region State Effects
    private void PreReplaceSuccess() {
        currentState.AddLogFiller(null, Utilities.NormalizeStringUpperCaseFirstLetters(tileObjectToReplace.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterReplaceSuccess() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        //place the tile object at the specified location.
        targetStructure.AddPOI(tileObjectToReplace, whereToPlace);
        tileObjectToReplace.AdjustHP(tileObjectToReplace.maxHP);
        actor.AdjustSupply(-TileObjectDB.GetTileObjectData(tileObjectToReplace.tileObjectType).constructionCost);
        //make all residents aware of supply pile, just in case it was ever removed because of ghost collision
        for (int i = 0; i < whereToPlace.parentAreaMap.area.region.residents.Count; i++) {
            whereToPlace.parentAreaMap.area.region.residents[i].AddAwareness(tileObjectToReplace);
        }
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

}

public class ReplaceTileObjectData : GoapActionData {
    public ReplaceTileObjectData() : base(INTERACTION_TYPE.REPLACE_TILE_OBJECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}