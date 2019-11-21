using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingSpot {

    public int id;
    public Vector3Int location; //where in the template grid is this connector placed
    public bool isOpen; //is this connection still open?
    public int[] adjacentSpots;

    public BuildingSpot() {
        isOpen = false;
    }

    public Vector3Int Difference(BuildingSpot otherConnector) {
        return new Vector3Int(location.x - otherConnector.location.x, location.y - otherConnector.location.y, 0);
    }
    public void SetIsOpen(bool state) {
        isOpen = state;
    }
    public void SetAdjacentSpotsAsOpen(StructureTemplate structureTemplate, List<BuildingSpot> except) {
        if (adjacentSpots != null) {
            for (int i = 0; i < adjacentSpots.Length; i++) {
                int currID = adjacentSpots[i];
                BuildingSpot spot = structureTemplate.GetBuildingSpotWithID(currID);
                if (except.Contains(spot) == false) {
                    spot.SetIsOpen(true);
                }
            }
        }
    }
}
