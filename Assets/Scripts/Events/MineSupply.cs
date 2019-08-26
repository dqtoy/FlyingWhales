using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineSupply : WorldEvent {

    public MineSupply() : base(WORLD_EVENT.MINE_SUPPLY) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        //- after effect: provides an initial +50 Supply to owner settlement after completion
        LandmarkManager.Instance.mainSettlement.AdjustSuppliesInBank(50);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(landmark);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN) && landmark.specificLandmarkType == LANDMARK_TYPE.MINES;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
