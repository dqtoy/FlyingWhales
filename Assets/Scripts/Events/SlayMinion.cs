using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlayMinion : WorldEvent {

    public SlayMinion() : base(WORLD_EVENT.SLAY_MINION) {
        duration = 3 * GameManager.ticksPerHour;
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.COMBAT };
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        Minion minion = region.assignedMinion;
        region.assignedMinion.SetAssignedRegion(null);
        region.SetAssignedMinion(null);

        minion.Death();

        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddToFillers(null, minion.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region, spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        return region.assignedMinion != null && region.mainLandmark.specificLandmarkType.IsPlayerLandmark();
    }
    #endregion
}
