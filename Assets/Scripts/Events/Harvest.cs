using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvest : WorldEvent {

    public Harvest() : base(WORLD_EVENT.HARVEST) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.GET_FOOD };
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region) {
        //- after effect: provides an initial +100 Food to owner settlement after completion
        LandmarkManager.Instance.enemyOfPlayerArea.AdjustFoodInBank(100);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.FARM;
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
