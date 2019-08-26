using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryTraining : WorldEvent {

	public MilitaryTraining() : base(WORLD_EVENT.MILITARY_TRAINING) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        //- after effect: character will level up
        landmark.eventSpawnedBy.LevelUp();
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(landmark);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.SOLDIER) && landmark.specificLandmarkType == LANDMARK_TYPE.BARRACKS;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.SOLDIER);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
