using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SettlementManager : MonoBehaviour {

    public static SettlementManager Instance = null;

    public List<Settlement> allSettlements;

    private void Awake() {
        Instance = this;
    }

    /*
     Create New Settlement on a specified tile.
     settlementType should be a type of Settlement,
     otherwise this will just return a null value
     */
    public Settlement CreateNewSettlementOnTile(HexTile location, System.Type settlementType) {
        if(settlementType == typeof(City)) {
            City newCity = new City(location);
            location.CreateStructureOnTile(STRUCTURE_TYPE.CITY);
            location.city.PopulateBorderTiles();
            location.emptyCityGO.SetActive(false);
            return newCity;
        }
        return null;
    }

    public Settlement CreateNewSettlementOnTile(HexTile location, System.Type settlementType, LANDMARK_TYPE landmarkType) {
        if (settlementType == typeof(Landmark)) {
            Landmark newLandmark = location.CreateLandmarkOfType(landmarkType);
            return newLandmark;
        }
        return null;
    }
}
