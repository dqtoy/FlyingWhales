﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrayTileObject : GoapAction {

    public PrayTileObject(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PRAY_TILE_OBJECT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Pray";
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //};
        actionIconString = GoapActionStateDB.Pray_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Pray Success");
    }
    protected override int GetCost() {
        //**Cost**: randomize between 15 - 55
        return Utilities.rng.Next(15, 56);
    }
    protected override void AddDefaultObjectsToLog(Log log) {
        base.AddDefaultObjectsToLog(log);
        TileObject obj = poiTarget as TileObject;
        log.AddToFillers(poiTarget, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region State Effects
    public void PrePraySuccess() {
        TileObject obj = poiTarget as TileObject;
        currentState.AddLogFiller(poiTarget, Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterPraySuccess() {
        if (poiTarget is GoddessStatue) {
            //Speed up divine intervention by 4 hours
            PlayerManager.Instance.player.AdjustDivineInterventionDuration(GameManager.Instance.GetTicksBasedOnHour(4));
        }
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
    #endregion
}

public class PrayTileObjectData : GoapActionData {
    public PrayTileObjectData() : base(INTERACTION_TYPE.PRAY_TILE_OBJECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
}