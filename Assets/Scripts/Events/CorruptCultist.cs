using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class CorruptCultist : WorldEvent {

    public CorruptCultist() : base(WORLD_EVENT.CORRUPT_CULTIST) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.CORRUPT_CHARACTER };
        description = "This mission will convert a cultist into a minion";
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        Cultist cultist = spawner.traitContainer.GetNormalTrait("Cultist") as Cultist;
        spawner.RecruitAsMinion(cultist.minionData);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region, spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        return region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE;
    }
    #endregion
}
