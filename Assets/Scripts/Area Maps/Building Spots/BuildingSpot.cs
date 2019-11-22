using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSpot {

	public int id { get; private set; }
    public bool isOpen { get; private set; }
    public bool isOccupied { get; private set; }
    public Vector3Int location { get; private set; }
    public Vector3 centeredLocation { get; private set; }
    public int[] adjacentSpots { get; private set; }
    public LocationGridTile[] tilesInTerritory { get; private set; }

    public BuildingSpot(BuildingSpotData data) {
        this.id = data.id;
        this.isOpen = data.isOpen;
        this.location = data.location;
        this.adjacentSpots = data.adjacentSpots;
        centeredLocation = new Vector3(location.x + 0.5f, location.y + 0.5f);
    }

    public void Initialize(AreaInnerTileMap tileMap) {
        //get the tiles in this spots territory.
        tilesInTerritory = new LocationGridTile[(int)InteriorMapManager.Building_Spot_Size.x * (int)InteriorMapManager.Building_Spot_Size.y];
        int radius = Mathf.FloorToInt(InteriorMapManager.Building_Spot_Size.x / 2f);
        Vector2Int startingPos = new Vector2Int(location.x - radius, location.y - radius);
        Vector2Int endPos = new Vector2Int(location.x + radius, location.y + radius);
        int tileCount = 0;
        for (int x = startingPos.x; x <= endPos.x; x++) {
            for (int y = startingPos.y; y <= endPos.y; y++) {
                LocationGridTile tile = tileMap.map[x, y];
                tilesInTerritory[tileCount] = tile;
                tileCount++;
            }
        }
    }

    #region Data Setting
    public void SetIsOpen(bool isOpen) {
        this.isOpen = isOpen;
    }
    public void SetIsOccupied(bool isOccupied) {
        this.isOccupied = isOccupied;
    }
    public void SetAllAdjacentSpotsAsOpen(AreaInnerTileMap map) {
        BuildingSpot[] adjacent = map.GetBuildingSpotsWithID(adjacentSpots);
        for (int i = 0; i < adjacent.Length; i++) {
            BuildingSpot adjacentSpot = adjacent[i];
            //set adjacent spots that are not yet open, and are not yet occupied to be open. Need to check for occupancy because all spots start off as closed.
            if (adjacentSpot.isOccupied == false && adjacentSpot.isOpen == false) {
                adjacentSpot.SetIsOpen(true);
            }
        }
    }
    #endregion

    #region Checkers
    public void CheckIfAdjacentSpotsCanStillBeOccupied(AreaInnerTileMap map) {
        BuildingSpot[] adjacent = map.GetBuildingSpotsWithID(adjacentSpots);
        for (int i = 0; i < adjacent.Length; i++) {
            BuildingSpot currSpot = adjacent[i];
            currSpot.CheckIfOccupied(map);
        }
    }
    public void CheckIfOccupied(AreaInnerTileMap map) {
        for (int i = 0; i < tilesInTerritory.Length; i++) {
            LocationGridTile currTile = tilesInTerritory[i];
            if (currTile.structure != null && currTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && currTile.structure.structureType != STRUCTURE_TYPE.WORK_AREA) {
                //the spot is now occupied. set that
                SetIsOccupied(true);
                SetIsOpen(false);
                SetAllAdjacentSpotsAsOpen(map); //set all adjacent unoccupied spots as open
                break;
            }
        }
    }
    #endregion

}
