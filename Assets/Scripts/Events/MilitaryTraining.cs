using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryTraining : WorldEvent {

	public MilitaryTraining() : base(WORLD_EVENT.MILITARY_TRAINING) {
        duration = 2 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: character will level up
        landmark.eventSpawnedBy.LevelUp();
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.SOLDIER) && landmark.specificLandmarkType == LANDMARK_TYPE.BARRACKS;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.SOLDIER);
    }
    #endregion
}
