using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultFoundation : WorldEvent {

    public CultFoundation() : base(WORLD_EVENT.CULT_FOUNDATION) {
        duration = 3 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return false;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    #endregion
}
