using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;

    public List<Settlement> allSettlements;

    private void Awake() {
        Instance = this;
    }

    /*
     Create a new landmark on a specified tile.
     */
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        BASE_LANDMARK_TYPE baseLandmarkType = Utilities.GetBaseLandmarkType(landmarkType);
        BaseLandmark newLandmark = location.CreateLandmarkOfType(baseLandmarkType, landmarkType);
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
