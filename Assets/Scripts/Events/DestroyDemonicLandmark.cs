using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDemonicLandmark : WorldEvent {

    public DestroyDemonicLandmark() : base(WORLD_EVENT.DESTROY_DEMONIC_LANDMARK) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.COMBAT, WORLD_EVENT_EFFECT.DESTROY_LANDMARK };
        description = "This mission aims to destroy the structure on the current location.";
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        if (region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
            //the landmark was demolished by the player
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect_invalid");
            AddDefaultFillersToLog(log, region);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
        } else {
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
            AddDefaultFillersToLog(log, region);
            log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(region.mainLandmark.specificLandmarkType.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            LandmarkManager.Instance.CreateNewLandmarkOnTile(region.coreTile, LANDMARK_TYPE.NONE, false);
        }        
        base.ExecuteAfterEffect(region, spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        return region.coreTile.isCorrupted && region.mainLandmark.specificLandmarkType.IsPlayerLandmark() && region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL;
    }
    #endregion

}

