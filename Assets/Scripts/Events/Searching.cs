using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Searching : WorldEvent {

    public Searching() : base(WORLD_EVENT.SEARCHING) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.SEARCHING };
        description = "This mission lets the character search in the region.";
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region, spawner);
    }
    //public override bool CanSpawnEventAt(Region region, Character spawner) {
    //    return region.HasFeature(RegionFeatureDB.Hallowed_Ground_Feature);
    //}
    #endregion

}
