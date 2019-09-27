using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvest : WorldEvent {

    public Harvest() : base(WORLD_EVENT.HARVEST) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.GET_FOOD };
        description = "This mission will provide more food for the settlement.";
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        //- after effect: provides an initial +100 Food to owner settlement after completion
        LandmarkManager.Instance.enemyOfPlayerArea.AdjustFoodInBank(100);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region, spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        return region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.FARM;
    }
    #endregion
}
