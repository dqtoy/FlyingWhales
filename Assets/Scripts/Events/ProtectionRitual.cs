using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectionRitual : WorldEvent {

    public ProtectionRitual() : base(WORLD_EVENT.PROTECTION_RITUAL) {
        duration = 3 * GameManager.ticksPerHour;
        isUnique = true;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: puts a Protective Barrier to the settlement that can only be removed when the Temple's region has been corrupted or the Temple landmark has been destroyed
        LandmarkManager.Instance.mainSettlement.coreTile.AddTileTag(TILE_TAG.PROTECTIVE_BARRIER);
        landmark.AddAfterInvasionAction(() => LandmarkManager.Instance.mainSettlement.coreTile.RemoveTileTag(TILE_TAG.PROTECTIVE_BARRIER));

        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddToFillers(LandmarkManager.Instance.mainSettlement, LandmarkManager.Instance.mainSettlement.name, LOG_IDENTIFIER.LANDMARK_3);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(ATTACK_TYPE.MAGICAL) && landmark.specificLandmarkType == LANDMARK_TYPE.TEMPLE && base.CanSpawnEventAt(landmark);
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(ATTACK_TYPE.MAGICAL);
    }
    #endregion
}
