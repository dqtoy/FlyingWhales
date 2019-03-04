using PathFind;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LocationGridTile : IHasNeighbours<LocationGridTile> {

    public enum Tile_Type { Empty, Wall, Structure, Gate }
    public enum Tile_State { Impassable, Empty, Reserved, Occupied }
    public enum Ground_Type { Soil, Grass, Stone }

    public bool hasDetail = false;

    public Tilemap parentTileMap { get; private set; }
    public Vector3Int localPlace { get; private set; }
    public Vector3 worldLocation { get; private set; }
    public Vector3 localLocation { get; private set; }
    public bool isInside { get; private set; }
    public Tile_Type tileType { get; private set; }
    public Tile_State tileState { get; private set; }
    public Ground_Type groundType { get; set; }
    public LocationStructure structure { get; private set; }
    public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }
    public GameObject prefabHere { get; private set; } //if there is a prefab that was instantiated at this tiles location
    //public List<LocationGridTile> neighborList { get; private set; }
    public IPointOfInterest objHere { get; private set; }

    public List<LocationGridTile> ValidTiles { get { return neighbours.Values.Where(o => o.tileType == Tile_Type.Empty || o.tileType == Tile_Type.Gate).ToList(); } }

    public LocationGridTile(int x, int y, Tilemap tilemap) {
        parentTileMap = tilemap;
        localPlace = new Vector3Int(x, y, 0);
        worldLocation = tilemap.CellToWorld(localPlace);
        localLocation = tilemap.CellToLocal(localPlace);
        tileType = Tile_Type.Empty;
        tileState = Tile_State.Empty;
    }
    public void FindNeighbours(LocationGridTile[,] map) {
        neighbours = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        //neighborList = new List<LocationGridTile>();
        int mapUpperBoundX = map.GetUpperBound(0);
        int mapUpperBoundY = map.GetUpperBound(1);
        Point thisPoint = new Point(localPlace.x, localPlace.y);
        foreach (KeyValuePair<TileNeighbourDirection, Point> kvp in possibleExits) {
            TileNeighbourDirection currDir = kvp.Key;
            Point exit = kvp.Value;
            Point result = exit.Sum(thisPoint);
            if (Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                neighbours.Add(currDir, map[result.X, result.Y]);
            }
        }

        //for (int i = 0; i < LandmarkManager.mapNeighborPoints.Count; i++) {
        //    Point pointCalculation = LandmarkManager.mapNeighborPoints[i];
        //    Point result = thisPoint.Sum(pointCalculation);
        //    if (Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
        //        Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
        //        neighborList.Add(map[result.X, result.Y]);
        //    }
        //}
    }
    public Dictionary<TileNeighbourDirection, Point> possibleExits {
        get {
            return new Dictionary<TileNeighbourDirection, Point>() {
                {TileNeighbourDirection.Top, new Point(0,1) },
                {TileNeighbourDirection.Bottom, new Point(0,-1) },
                {TileNeighbourDirection.Left, new Point(-1,0) },
                {TileNeighbourDirection.Right, new Point(1,0) },
            };
        }
    }
    public void SetIsInside(bool isInside) {
        this.isInside = isInside;
    }
    public void SetTileType(Tile_Type tileType) {
        this.tileType = tileType;
        switch (tileType) {
            case Tile_Type.Wall:
                SetTileState(Tile_State.Impassable);
                break;
            default:
                SetTileState(Tile_State.Empty);
                break;
        }
    }

    public List<TileNeighbourDirection> GetSameStructureNeighbourDirections() {
        List<TileNeighbourDirection> dirs = new List<TileNeighbourDirection>();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (kvp.Value.structure == this.structure) {
                dirs.Add(kvp.Key);
            }
        }
        return dirs;
    }

    #region Structures
    public void SetStructure(LocationStructure structure) {
        this.structure = structure;
        this.structure.AddTile(this);
    }
    public void SetTileState(Tile_State state) {
        this.tileState = state;
    }
    #endregion

    #region Points of Interest
    public void SetObjectHere(IPointOfInterest poi) {
        objHere = poi;
        SetTileState(Tile_State.Occupied);
        poi.SetGridTileLocation(this);
    }
    public IPointOfInterest RemoveObjectHere() {
        if (objHere != null) {
            IPointOfInterest removedObj = objHere;
            objHere.SetGridTileLocation(null);
            objHere = null;
            SetTileState(Tile_State.Empty);
            return removedObj;
        }
        return null;
    }
    #endregion

    #region Utilities
    public bool HasOutsideNeighbour() {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (!kvp.Value.isInside) {
                return true;
            }
        }
        return false;
    }
    public bool IsAtEdgeOfMap() {
        TileNeighbourDirection[] dirs = Utilities.GetEnumValues<TileNeighbourDirection>();
        for (int i = 0; i < dirs.Length; i++) {
            if (!neighbours.ContainsKey(dirs[i])) {
                return true;
            }
        }
        return false;
    }
    public bool HasNeighborAtEdgeOfMap() {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (kvp.Value.IsAtEdgeOfMap()) {
                return true;
            }
        }
        return false;
    }
    public bool HasDifferentDwellingOrOutsideNeighbour() {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (!kvp.Value.isInside || (kvp.Value.structure != this.structure)) {
                return true;
            }
        }
        return false;
    }
    public bool IsAdjacentToWall() {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (kvp.Value.tileType == Tile_Type.Wall || (kvp.Value.structure != null && kvp.Value.structure.structureType != STRUCTURE_TYPE.WORK_AREA)) {
                return true;
            }
        }
        return false;
    }
    public override string ToString() {
        return localPlace.ToString();
    }
    public void SetPrefabHere(GameObject obj) {
        prefabHere = obj;
    }
    #endregion
}

[System.Serializable]
public struct TwoTileDirections {
    public TileNeighbourDirection from;
    public TileNeighbourDirection to;

    public TwoTileDirections(TileNeighbourDirection from, TileNeighbourDirection to) {
        this.from = from;
        this.to = to;
    }
}
