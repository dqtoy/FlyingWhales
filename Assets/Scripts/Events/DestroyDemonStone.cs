﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDemonStone : WorldEvent {

    public DestroyDemonStone() : base(WORLD_EVENT.DESTROY_DEMON_STONE) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        //- after effect: Demon Stone object will be removed
        landmark.SetWorldObject(null);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(landmark);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER) && landmark.worldObj is DemonStone;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}

