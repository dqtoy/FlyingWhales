using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildStructure : GoapAction {

    public BuildStructure() : base(INTERACTION_TYPE.BUILD_STRUCTURE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Build Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 3;
    }
    public override void AddFillersToLog(Log log, Character actor, IPointOfInterest poiTarget, object[] otherData, LocationStructure targetStructure) {
        base.AddFillersToLog(log, actor, poiTarget, otherData, targetStructure);
        BuildSpotTileObject target = poiTarget as BuildSpotTileObject;
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(target.spot.blueprintType.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            BuildSpotTileObject buildSpot = poiTarget as BuildSpotTileObject;
            return buildSpot.spot.hasBlueprint;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreBuildSuccess(ActualGoapNode goapNode) {
        BuildSpotTileObject target = goapNode.poiTarget as BuildSpotTileObject;
        goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(target.spot.blueprintType.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        BuildSpotTileObject spot = goapNode.poiTarget as BuildSpotTileObject;
        spot.BuildBlueprint();
    }
    #endregion
}

