using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Study : WorldEvent {

    public Study() : base(WORLD_EVENT.STUDY) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.GAIN_POSITIVE_TRAIT, WORLD_EVENT_EFFECT.EXPLORE };
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region) {
        //(gain a positive trait) 
        //LandmarkManager.Instance.enemyOfPlayerArea.AdjustSuppliesInBank(50);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    #endregion

}
