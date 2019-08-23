using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidMine : WorldEvent {

    public RaidMine() : base(WORLD_EVENT.RAID_MINE) {
        duration = 30;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: reduces initial Supply to onwer settlement by 200
        landmark.eventSpawnedBy.homeArea.AdjustSuppliesInBank(-200);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.BANDIT) && landmark.specificLandmarkType == LANDMARK_TYPE.MINES;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.BANDIT);
    }
    #endregion
}
