using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectionRitual : WorldEvent {

    public ProtectionRitual() : base(WORLD_EVENT.PROTECTION_RITUAL) {
        isUnique = true;
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region) {
        //- after effect: puts a Protective Barrier to the settlement that can only be removed when the Temple's region has been corrupted or the Temple landmark has been destroyed
        LandmarkManager.Instance.enemyOfPlayerArea.coreTile.AddTileTag(TILE_TAG.PROTECTIVE_BARRIER);
        region.AddAfterInvasionAction(() => LandmarkManager.Instance.enemyOfPlayerArea.coreTile.RemoveTileTag(TILE_TAG.PROTECTIVE_BARRIER));

        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddToFillers(LandmarkManager.Instance.enemyOfPlayerArea, LandmarkManager.Instance.enemyOfPlayerArea.name, LOG_IDENTIFIER.LANDMARK_3);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.HasAnyCharacterOfType(ATTACK_TYPE.MAGICAL) && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.TEMPLE && base.CanSpawnEventAt(region);
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(ATTACK_TYPE.MAGICAL);
    }
    #endregion
}
