using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;

    private void Awake() {
        Instance = this;
    }

    /*
     Create a new landmark on a specified tile.
     */
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        BASE_LANDMARK_TYPE baseLandmarkType = Utilities.GetBaseLandmarkType(landmarkType);
        BaseLandmark newLandmark = location.CreateLandmarkOfType(baseLandmarkType, landmarkType);
        if(baseLandmarkType == BASE_LANDMARK_TYPE.SETTLEMENT && landmarkType != LANDMARK_TYPE.CITY) {
            if(landmarkType == LANDMARK_TYPE.GOBLIN_CAMP) {
                //Create a new faction to occupy the new settlement
                Faction newFaction = FactionManager.Instance.CreateNewFaction(typeof(Camp), RACE.GOBLIN);
                newLandmark.OccupyLandmark(newFaction);
            }
        }
        return newLandmark;
    }
    /*
     Occupy a specified landmark.
         */
    public void OccupyLandmark(BaseLandmark landmark, Faction occupant) {
        landmark.OccupyLandmark(occupant);
    }
    /*
     Occupy the main settlement in a region
         */
    public void OccupyLandmark(Region region, Faction occupant) {
        region.centerOfMass.landmarkOnTile.OccupyLandmark(occupant);
    }
}
