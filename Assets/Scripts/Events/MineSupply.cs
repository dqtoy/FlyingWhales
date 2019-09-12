using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineSupply : WorldEvent {

    public MineSupply() : base(WORLD_EVENT.MINE_SUPPLY) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        //- after effect: provides an initial +50 Supply to owner settlement after completion
        LandmarkManager.Instance.enemyOfPlayerArea.AdjustSuppliesInBank(50);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.HasAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN) && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.MINES;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
