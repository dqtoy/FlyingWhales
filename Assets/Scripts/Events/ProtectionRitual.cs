using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectionRitual : WorldEvent {

    public ProtectionRitual() : base(WORLD_EVENT.PROTECTION_RITUAL) {
        duration = 3 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: puts a Protective Barrier to the settlement that can only be removed when the Temple's region has been corrupted or the Temple landmark has been destroyed
    }
    public override void ExecuteAfterInvasionEffect(BaseLandmark landmark) {
        base.ExecuteAfterInvasionEffect(landmark);
        //- after invasion: remove Protective Barrier on settlement
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return false;
        return landmark.HasAnyCharacterOfType(ATTACK_TYPE.MAGICAL) && landmark.specificLandmarkType == LANDMARK_TYPE.TEMPLE;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(ATTACK_TYPE.MAGICAL);
    }
    #endregion
}
