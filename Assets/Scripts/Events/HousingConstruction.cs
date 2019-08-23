using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingConstruction : WorldEvent {

    public HousingConstruction() : base(WORLD_EVENT.HOUSING_CONSTRUCTION) {
        duration = 3 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: adds a new house to owner settlement
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return false;
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN) && landmark.specificLandmarkType == LANDMARK_TYPE.PYRAMID;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    #endregion
}

