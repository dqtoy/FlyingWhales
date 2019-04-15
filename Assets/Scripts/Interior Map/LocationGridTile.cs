using PathFind;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LocationGridTile : IHasNeighbours<LocationGridTile> {

    public enum Tile_Type { Empty, Wall, Structure, Gate, Road, Structure_Entrance }
    public enum Tile_State { Empty, Occupied }
    public enum Tile_Access { Passable, Impassable, }
    public enum Ground_Type { Soil, Grass, Stone }

    public bool hasDetail = false;

    public AreaInnerTileMap parentAreaMap { get; private set; }
    public Tilemap parentTileMap { get; private set; }
    public Vector3Int localPlace { get; private set; }
    public Vector2Int localPlace2D { get; private set; }
    public Vector3 worldLocation { get; private set; }
    public Vector3 centeredWorldLocation { get; private set; }
    public Vector3 localLocation { get; private set; }
    public Vector3 centeredLocalLocation { get; private set; }
    public bool isInside { get; private set; }
    public Tile_Type tileType { get; private set; }
    public Tile_State tileState { get; private set; }
    public Tile_Access tileAccess { get; private set; }
    public Ground_Type groundType { get; set; }
    public LocationStructure structure { get; private set; }
    public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }
    //public List<LocationGridTile> neighbourList { get; private set; }
    public GameObject prefabHere { get; private set; } //if there is a prefab that was instantiated at this tiles location
    //public List<LocationGridTile> neighborList { get; private set; }
    public IPointOfInterest objHere { get; private set; }
    public List<Character> charactersHere { get; private set; }
    public Character occupant { get; private set; }
    public bool isOccupied { get { return tileState == Tile_State.Occupied || occupant != null; } }

    public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty || o.tileType == Tile_Type.Gate || o.tileType == Tile_Type.Road).ToList(); } }
    public List<LocationGridTile> RealisticTiles { get { return FourNeighbours().Where(o => o.tileAccess == Tile_Access.Passable && (o.structure != null || o.tileType == Tile_Type.Road || o.tileType == Tile_Type.Gate)).ToList(); } }
    public List<LocationGridTile> RoadTiles { get { return neighbours.Values.Where(o => o.tileType == Tile_Type.Road).ToList(); } }
    public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbours.Values.Where(o => !o.isOccupied && o.structure == this.structure).ToList(); } }

    public LocationGridTile(int x, int y, Tilemap tilemap, AreaInnerTileMap parentAreaMap) {
        this.parentAreaMap = parentAreaMap;
        parentTileMap = tilemap;
        localPlace = new Vector3Int(x, y, 0);
        localPlace2D = new Vector2Int(x, y);
        worldLocation = tilemap.CellToWorld(localPlace);
        localLocation = tilemap.CellToLocal(localPlace);
        centeredLocalLocation = new Vector3(localLocation.x + 0.5f, localLocation.y + 0.5f, localLocation.z);
        int xMult = worldLocation.x < 0 ? -1 : 1;
        int yMult = worldLocation.y < 0 ? -1 : 1;
        centeredWorldLocation = new Vector3(((int)worldLocation.x) + (0.5f * xMult), ((int)worldLocation.y) + (0.5f * yMult), worldLocation.z);
        tileType = Tile_Type.Empty;
        tileState = Tile_State.Empty;
        tileAccess = Tile_Access.Passable;
        charactersHere = new List<Character>();
    }
    public void UpdateWorldLocation() {
        worldLocation = parentTileMap.CellToWorld(localPlace);
        centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
    }
    public List<LocationGridTile> FourNeighbours() {
        List<LocationGridTile> fn = new List<LocationGridTile>();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            switch (keyValuePair.Key) {
                case TileNeighbourDirection.North:
                case TileNeighbourDirection.South:
                case TileNeighbourDirection.West:
                case TileNeighbourDirection.East:
                    fn.Add(keyValuePair.Value);
                    break;
            }
        }
        return fn;
    }
    public Dictionary<TileNeighbourDirection, LocationGridTile> FourNeighboursDictionary() {
        Dictionary<TileNeighbourDirection, LocationGridTile> fn = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            switch (keyValuePair.Key) {
                case TileNeighbourDirection.North:
                case TileNeighbourDirection.South:
                case TileNeighbourDirection.West:
                case TileNeighbourDirection.East:
                    fn.Add(keyValuePair.Key, keyValuePair.Value);
                    break;
            }
        }
        return fn;
    }
    public void FindNeighbours(LocationGridTile[,] map) {
        neighbours = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        //neighbourList = new List<LocationGridTile>();
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
        //        neighbourList.Add(map[result.X, result.Y]);
        //    }
        //}
    }
    public Dictionary<TileNeighbourDirection, Point> possibleExits {
        get {
            return new Dictionary<TileNeighbourDirection, Point>() {
                {TileNeighbourDirection.North, new Point(0,1) },
                {TileNeighbourDirection.South, new Point(0,-1) },
                {TileNeighbourDirection.West, new Point(-1,0) },
                {TileNeighbourDirection.East, new Point(1,0) },
                {TileNeighbourDirection.North_West, new Point(-1,1) },
                {TileNeighbourDirection.North_East, new Point(1,1) },
                {TileNeighbourDirection.South_West, new Point(-1,-1) },
                {TileNeighbourDirection.South_East, new Point(1,-1) },
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
                SetTileAccess(Tile_Access.Impassable);
                break;
            case Tile_Type.Gate:
                SetTileAccess(Tile_Access.Passable);
                break;
                //default:
                //    SetTileState(Tile_State.Empty);
                //    break;
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
    public List<TileNeighbourDirection> GetDifferentStructureNeighbourDirections() {
        List<TileNeighbourDirection> dirs = new List<TileNeighbourDirection>();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (kvp.Value.structure != this.structure) {
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
        //if (state == Tile_State.Occupied) {
        //    Messenger.Broadcast(Signals.TILE_OCCUPIED, this, objHere);
        //}
    }
    public void SetTileAccess(Tile_Access state) {
        this.tileAccess = state;
    }
    #endregion

    #region Points of Interest
    public void SetObjectHere(IPointOfInterest poi) {
        objHere = poi;
        poi.SetGridTileLocation(this);
        SetTileState(Tile_State.Occupied);
        Messenger.Broadcast(Signals.OBJECT_PLACED_ON_TILE, this, poi);
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
    /// <summary>
    /// Does this tile have a neighbour that is part of a different structure, or is part of the outside map?
    /// </summary>
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
    //public void SetPrefabHere(GameObject obj) {
    //    prefabHere = obj;
    //}
    public float GetDistanceTo(LocationGridTile tile) {
        return Vector2.Distance(this.localLocation, tile.localLocation);
    }
    public bool HasOccupiedNeighbour() {
        for (int i = 0; i < neighbours.Values.Count; i++) {
            if (neighbours.Values.ElementAt(i).isOccupied) {
                return true;
            }
        }
        return false;
    }
    public bool HasNeighbourOfType(Tile_Type type, bool useFourNeighbours = false) {
        Dictionary<TileNeighbourDirection, LocationGridTile> n = neighbours;
        if (useFourNeighbours) {
            n = FourNeighboursDictionary();
        }
        for (int i = 0; i < n.Values.Count; i++) {
            if (neighbours.Values.ElementAt(i).tileType == type) {
                return true;
            }
        }
        return false;
    }
    public bool IsNeighbour(LocationGridTile tile) {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Value == tile) {
                return true;
            }
        }
        return false;
    }
    public void SetOccupant(Character character) {
        occupant = character;
        character.SetGridTileLocation(this);
        //Debug.LogWarning("Tile occupied signal fired for tile " + ToString() + " by " + character.name + " because the character is now its occupant");
        Messenger.Broadcast<LocationGridTile, IPointOfInterest>(Signals.TILE_OCCUPIED, this, occupant);
    }
    public void RemoveOccupant() {
        if (occupant != null) {
            //occupant.SetGridTileLocation(null);
            occupant = null;
            //if (prefabHere != null) {
            //    CharacterPortrait portrait = prefabHere.GetComponent<CharacterPortrait>();
            //    if (portrait != null) {
            //        portrait.SetImageRaycastTargetState(true);
            //    }
            //    //ObjectPoolManager.Instance.DestroyObject(tile.prefabHere);
            //    SetPrefabHere(null);
            //}
        }
    }
    public bool IsAdjacentTo(IPointOfInterest poi) {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Value.objHere == poi || keyValuePair.Value.occupant == poi) {
                return true;
            }
        }
        return false;
    }
    public bool IsAdjacentTo(System.Type type) {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if ((keyValuePair.Value.objHere != null && keyValuePair.Value.objHere.GetType() == type)
                || (keyValuePair.Value.occupant != null && keyValuePair.Value.occupant.GetType() == type)) {
                return true;
            }
        }
        return false;
    }
    public bool IsAdjacentToPasssableTiles(int count = 1) {
        int passableCount = 0;
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (kvp.Value.tileAccess == Tile_Access.Passable && kvp.Value.structure == structure) {
                passableCount++;
            }
            if (passableCount >= count) {
                return true;
            }
        }
        return false;
    }
    public bool WillMakeNeighboursPassableTileInvalid(int neededPassable = 1) {
        SetTileAccess(Tile_Access.Impassable);
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (kvp.Value.structure == structure) {
                if (!kvp.Value.IsAdjacentToPasssableTiles(neededPassable)) {
                    SetTileAccess(Tile_Access.Passable);
                    return true;
                }
            }
        }
        SetTileAccess(Tile_Access.Passable);
        return false;
    }
    public bool HasNeighbouringStructureOfType(STRUCTURE_TYPE type, bool useFourNeighbours = false) {
        Dictionary<TileNeighbourDirection, LocationGridTile> n = neighbours;
        if (useFourNeighbours) {
            n = FourNeighboursDictionary();
        }
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in n) {
            if (keyValuePair.Value.structure != null && keyValuePair.Value.structure.structureType == type) {
                return true;
            }
        }
        return false;
    }
    public bool HasNeighbouringStructureOfType(List<STRUCTURE_TYPE> types) {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Value.structure != null && types.Contains(keyValuePair.Value.structure.structureType)) {
                return true;
            }
        }
        return false;
    }
    public bool CanBeAnEntrance() {
        if (!HasNeighbouringStructureOfType(STRUCTURE_TYPE.WORK_AREA, true)) {
            return false;
        }
        if (GetDifferentStructureNeighbourDirections().Count > 3) { //because corner tiles of structures always have more than 3 different structure neighbours, and corner tiles should not be made entrances (looks weird)
            return false;
        }
        if (parentAreaMap.objectsTilemap.GetTile(localPlace) != null) {
            return false; //if this tile has a preplaced object on it, it is not valid
        }
        return true;
    }
    public bool HasStructureOfTypeInRadius(List<STRUCTURE_TYPE> types, int radius) {
        List<LocationGridTile> tiles = parentAreaMap.GetTilesInRadius(this, radius, false, true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (currTile.structure != null && types.Contains(currTile.structure.structureType)) {
                return true;
            }
        }
        return false;
    }
    public bool HasStructureOfTypeHorizontally(List<STRUCTURE_TYPE> types, int range) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        if (range > 0) {
            for (int i = 0; i < range; i++) {
                int nextX = this.localPlace.x + 1;
                if (nextX < parentAreaMap.map.GetUpperBound(0)) {
                    tiles.Add(parentAreaMap.map[nextX, this.localPlace.y]);
                }
            }
        } else if (range < 0) {
            for (int i = 0; i < Mathf.Abs(range); i++) {
                int nextX = this.localPlace.x - 1;
                if (nextX > parentAreaMap.map.GetLowerBound(0)) {
                    tiles.Add(parentAreaMap.map[nextX, this.localPlace.y]);
                }
            }
        }
        
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (currTile.structure != null && types.Contains(currTile.structure.structureType)) {
                return true;
            }
        }
        return false;
    }
    public TileNeighbourDirection GetCardinalDirectionOfStructureType(STRUCTURE_TYPE type) {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in FourNeighboursDictionary()) {
            if (keyValuePair.Value.structure != null && keyValuePair.Value.structure.structureType == type) {
                return keyValuePair.Key;
            }
        }
        throw new System.Exception(this.ToString() + " this tile is not next to a structure of type " + type.ToString());
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
        if (unoccupiedNeighbours.Count == 0) {
            if (this.structure != null) {
                LocationGridTile nearestTile = null;
                float nearestDist = 99999f;
                for (int i = 0; i < this.structure.unoccupiedTiles.Count; i++) {
                    LocationGridTile currTile = this.structure.unoccupiedTiles[i];
                    if (currTile != this) {
                        float dist = Vector2.Distance(currTile.localLocation, this.localLocation);
                        if (dist < nearestDist) {
                            nearestTile = currTile;
                            nearestDist = dist;
                        }
                    }
                }
                return nearestTile;
            }
        } else {
            return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
        }
        return null;
    }
    #endregion

    #region Intel
    public void OnClickTileActions(PointerEventData.InputButton inputButton) {
        Messenger.Broadcast(Signals.HIDE_MENUS);
        //Comment Reason: Used this to quickly set a tiles state from occupied to empty and vice versa.
        //if (inputButton == PointerEventData.InputButton.Right) {
        //    if (tileState == Tile_State.Occupied) {
        //        SetTileState(Tile_State.Empty);
        //    } else {
        //        SetTileState(Tile_State.Occupied);
        //    }
        //    return;
        //}
        if (InteriorMapManager.Instance.IsMouseOnMarker()) {
            return;
        }
        if (objHere == null) {
            //if (inputButton == PointerEventData.InputButton.Right) {
            //    if (InteriorMapManager.Instance.IsHoldingPOI()) {
            //        InteriorMapManager.Instance.PlaceHeldPOI(this);
            //    }
            //}
        } else if (objHere is TileObject || objHere is SpecialToken) {
            if (inputButton == PointerEventData.InputButton.Middle && objHere is TileObject) {
                (objHere as TileObject).LogActionHistory();
            } 
            //else if (inputButton == PointerEventData.InputButton.Right) {
            //    if (!InteriorMapManager.Instance.IsHoldingPOI()) {
            //        InteriorMapManager.Instance.HoldPOI(objHere);
            //    }
            //} 
            else {
                //parentAreaMap.ShowIntelItemAt(this, InteractionManager.Instance.CreateNewIntel(objHere));
                if ((objHere as TileObject).tileObjectType == TILE_OBJECT_TYPE.CORPSE) {
                    UIManager.Instance.ShowCharacterInfo((objHere as Corpse).character);
                }
            }
        } else if (objHere is Character) {
            UIManager.Instance.ShowCharacterInfo((objHere as Character));
        } else if (occupant != null) {
            UIManager.Instance.ShowCharacterInfo(occupant);
        }
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
