using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidMine : WorldEvent {

    public RaidMine() : base(WORLD_EVENT.RAID_MINE) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        //- after effect: reduces initial Supply to onwer settlement by 200
        region.eventSpawnedBy.homeArea.AdjustSuppliesInBank(-200);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.HasAnyCharacterOfType(CHARACTER_ROLE.BANDIT) && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.MINES;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(CHARACTER_ROLE.BANDIT);
    }
    #endregion
}
