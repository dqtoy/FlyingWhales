using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineSupply : WorldEvent {

    public MineSupply() : base(WORLD_EVENT.MINE_SUPPLY) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.GET_SUPPLY };
        description = "This mission will increase the settlement's Supply.";
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        //- after effect: provides an initial +50 Supply to owner settlement after completion
        LandmarkManager.Instance.enemyOfPlayerArea.AdjustSuppliesInBank(50);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region, spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        return region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.MINES;
    }
    #endregion
}
