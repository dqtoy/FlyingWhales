using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingConstruction : WorldEvent {

    public HousingConstruction() : base(WORLD_EVENT.HOUSING_CONSTRUCTION) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        base.ExecuteAfterEffect(region);
        //- after effect: adds a new house to owner settlement
    }
    public override bool CanSpawnEventAt(Region region) {
        return false;
        return region.HasAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN) && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.FACTORY;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    #endregion
}

