using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryTraining : WorldEvent {

	public MilitaryTraining() : base(WORLD_EVENT.MILITARY_TRAINING) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        //- after effect: character will level up
        region.eventSpawnedBy.LevelUp();
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.HasAnyCharacterOfType(CHARACTER_ROLE.SOLDIER) && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.BARRACKS;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(CHARACTER_ROLE.SOLDIER);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
