using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSpot {

    //data
	public int id { get; private set; }
    public bool isOpen { get; private set; }
    public bool isOccupied { get; private set; }
    public Vector3Int location { get; private set; }
    public Vector3 centeredLocation { get; private set; }
    public int[] adjacentSpots { get; private set; }
    public LocationGridTile[] tilesInTerritory { get; private set; }
    public Vector2Int locationInBuildSpotGrid { get; private set; }
    public Dictionary<GridNeighbourDirection, BuildingSpot> neighbours { get; private set; }

    //Building
    public LocationStructureObject blueprint { get; private set; }
    public STRUCTURE_TYPE blueprintType { get; private set; }


    #region getters
    public bool hasBlueprint {
        get { return blueprint != null; }
    }
    #endregion

    public BuildingSpot(BuildingSpotData data) {
        this.id = data.id;
        this.isOpen = data.isOpen;
        this.location = data.location;
        this.adjacentSpots = data.adjacentSpots;
        this.locationInBuildSpotGrid = data.buildingSpotGridPos;
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
    public void FindNeighbours(AreaInnerTileMap map) {
        if (neighbours != null) {
            throw new System.Exception($"Build spot {this.id.ToString()} is trying to find neighbours again!");
        }
        //Debug.Log("Finding neighbours for build spot " + id.ToString());
        neighbours = new Dictionary<GridNeighbourDirection, BuildingSpot>();
        int mapUpperBoundX = map.buildingSpots.GetUpperBound(0);
        int mapUpperBoundY = map.buildingSpots.GetUpperBound(1);
        Point thisPoint = new Point(locationInBuildSpotGrid.x, locationInBuildSpotGrid.y);
        foreach (KeyValuePair<GridNeighbourDirection, Point> kvp in possibleExits) {
            GridNeighbourDirection direction = kvp.Key;
            Point exit = kvp.Value;
            Point result = exit.Sum(thisPoint);
            if (Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                neighbours.Add(direction, map.buildingSpots[result.X, result.Y]);
            }
        }
    }

    #region Data Setting
    public void SetIsOpen(bool isOpen) {
        this.isOpen = isOpen;
        //Debug.Log($"Set building spot {id.ToString()} is open to {isOpen.ToString()}");
    }
    public void SetIsOccupied(bool isOccupied) {
        this.isOccupied = isOccupied;
        //Debug.Log($"Set building spot {id.ToString()} is occupied to {isOccupied.ToString()}");
    }
    public void SetAllAdjacentSpotsAsOpen(AreaInnerTileMap map) {
        List<BuildingSpot> adjacent = GetNeighbourList();
        for (int i = 0; i < adjacent.Count; i++) {
            BuildingSpot adjacentSpot = adjacent[i];
            //set adjacent spots that are not yet open, and are not yet occupied to be open. Need to check for occupancy because all spots start off as closed.
            if (adjacentSpot.isOccupied == false && adjacentSpot.isOpen == false) {
                adjacentSpot.SetIsOpen(true);
            }
        }
    }
    public List<BuildingSpot> GetNeighbourList() {
        List<BuildingSpot> adjacent = new List<BuildingSpot>();
        if (neighbours == null) {
            throw new System.Exception($"Building spot { this.id } has a null neighbours dictionary!");
        }
        foreach (KeyValuePair<GridNeighbourDirection, BuildingSpot> kvp in neighbours) {
            adjacent.Add(kvp.Value);
        }
        return adjacent;
    }
    public Dictionary<GridNeighbourDirection, Point> possibleExits {
        get {
            return new Dictionary<GridNeighbourDirection, Point>() {
                {GridNeighbourDirection.North, new Point(0,1) },
                {GridNeighbourDirection.South, new Point(0,-1) },
                {GridNeighbourDirection.West, new Point(-1,0) },
                {GridNeighbourDirection.East, new Point(1,0) },
                //{GridNeighbourDirection.North_West, new Point(-1,1) },
                //{GridNeighbourDirection.North_East, new Point(1,1) },
                //{GridNeighbourDirection.South_West, new Point(-1,-1) },
                //{GridNeighbourDirection.South_East, new Point(1,-1) },
            };
        }
    }
    #endregion

    #region Checkers
    public void CheckIfAdjacentSpotsCanStillBeOccupied(AreaInnerTileMap map) {
        List<BuildingSpot> adjacent = GetNeighbourList();
        for (int i = 0; i < adjacent.Count; i++) {
            BuildingSpot currSpot = adjacent[i];
            currSpot.CheckIfOccupied(map);
        }
    }
    public void CheckIfOccupied(AreaInnerTileMap map) {
        for (int i = 0; i < tilesInTerritory.Length; i++) {
            LocationGridTile currTile = tilesInTerritory[i];
            if (currTile.hasBlueprint || (currTile.structure != null && currTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && currTile.structure.structureType != STRUCTURE_TYPE.WORK_AREA)) {
                //the spot is now occupied. set that
                SetIsOccupied(true);
                SetIsOpen(false);
                SetAllAdjacentSpotsAsOpen(map); //set all adjacent unoccupied spots as open
                break;
            }
        }
    }
    public bool CanPlaceStructureOnSpot(LocationStructureObject obj, AreaInnerTileMap map) {
        return map.IsBuildSpotValidFor(obj, this);
    }
    #endregion

    #region Building
    public void SetBlueprint(LocationStructureObject blueprint, STRUCTURE_TYPE blueprintType) {
        this.blueprint = blueprint;
        this.blueprintType = blueprintType;
    }
    public void ClearBlueprints() {
        this.blueprint = null;
    }
    public Vector3 GetRandomTilePositionInBuildSpot(LocationStructureObject structureObj) {
        int radius = Mathf.FloorToInt(InteriorMapManager.Building_Spot_Size.x / 2f);
        if (structureObj.size.x - radius >= radius || structureObj.size.y - radius >= radius) {
            //do not randomize spot, just place at center.
            return centeredLocation;
        } else {
            int limitedRadius = radius - 2; //-2 is to limit the structure from being placed on the border.
            int randomX = Random.Range(-limitedRadius, limitedRadius);
            int randomY = Random.Range(-limitedRadius, limitedRadius);

            int newX = location.x;
            //if (structureObj.IsHorizontallyBig() == false) {
                newX += randomX;
            //}
            int newY = location.y;
            //if (structureObj.IsVerticallyBig() == false) {
                newY += randomY;
            //}

            return new Vector3(newX + 0.5f, newY + 0.5f);
        }
        
    }
    #endregion

}
