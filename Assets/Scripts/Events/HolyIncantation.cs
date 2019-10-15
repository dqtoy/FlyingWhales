using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyIncantation : WorldEvent {

    public HolyIncantation() : base(WORLD_EVENT.HOLY_INCANTATION) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.DIVINE_INTERVENTION_SPEED_UP };
        description = "This mission performs a holy incantation that will significantly speed up the Divine Intervention.";
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(region.mainLandmark.specificLandmarkType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        PlayerManager.Instance.player.AdjustDivineInterventionDuration(-GameManager.ticksPerDay);
        base.ExecuteAfterEffect(region, spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        return region.HasFeature(RegionFeatureDB.Hallowed_Ground_Feature);
    }
    #endregion

}
