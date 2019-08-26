﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvest : WorldEvent {

    public Harvest() : base(WORLD_EVENT.HARVEST) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        //- after effect: provides an initial +100 Food to owner settlement after completion
        LandmarkManager.Instance.mainSettlement.AdjustFoodInBank(100);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(landmark);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN) && landmark.specificLandmarkType == LANDMARK_TYPE.FARM;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
