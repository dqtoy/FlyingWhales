using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : GoapAction {

    public override LocationStructure targetStructure { get { return structureToBeDropped; } }

    private LocationStructure structureToBeDropped;
    private LocationGridTile gridTileToBeDropped;

    public Drop(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeRegion, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Drop Success");
    }
    protected override int GetCost() {
        return 1;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        if(gridTileToBeDropped != null) {
            return gridTileToBeDropped;
        }
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionWhileTravelling() {
        base.OnStopActionWhileTravelling();
        Character targetCharacter = poiTarget as Character;
        actor.currentParty.RemoveCharacter(targetCharacter);
    }
    public override void OnStopActionDuringCurrentState() {
        base.OnStopActionDuringCurrentState();
        Character targetCharacter = poiTarget as Character;
        actor.currentParty.RemoveCharacter(targetCharacter);
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is LocationStructure) {
            structureToBeDropped = otherData[0] as LocationStructure;
            return true;
        }else if (otherData.Length == 2 && otherData[0] is LocationStructure && otherData[1] is LocationGridTile) {
            structureToBeDropped = otherData[0] as LocationStructure;
            gridTileToBeDropped = otherData[1] as LocationGridTile;
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region Preconditions
    private bool IsInActorParty() {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty;
    }
    #endregion

    #region State Effects
    public void PreDropSuccess() {
        currentState.AddLogFiller(actor.currentStructure, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterDropSuccess() {
        Character target = poiTarget as Character;
        actor.currentParty.RemoveCharacter(target, dropLocation: gridTileToBeDropped);
        //if (gridTileToBeDropped.objHere != null && gridTileToBeDropped.objHere is TileObject) {
        //    TileObject to = gridTileToBeDropped.objHere as TileObject;
        //    to.AddUser(target);
        //}
        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeRegion, targetPOI = poiTarget });
    }
    #endregion
}

public class DropData : GoapActionData {
    public DropData() : base(INTERACTION_TYPE.DROP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}
