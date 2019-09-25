using PathFind;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using BayatGames.SaveGameFree.Types;

public class LocationGridTile : IHasNeighbours<LocationGridTile>, ITraitable {

    public enum Tile_Type { Empty, Wall, Structure, Gate, Road, Structure_Entrance }
    public enum Tile_State { Empty, Occupied }
    public enum Tile_Access { Passable, Impassable, }
    public enum Ground_Type { Soil, Grass, Stone, Snow, Tundra, Cobble, Wood, Snow_Dirt, Water }

    protected List<Trait> _traits;
    public bool hasDetail { get; set; }
    public string name { get { return localPlace.ToString(); } }
    public AreaInnerTileMap parentAreaMap { get; private set; }
    public Tilemap parentTileMap { get; private set; }
    public Vector3Int localPlace { get; private set; }
    public Vector3 worldLocation { get; private set; }
    public Vector3 centeredWorldLocation { get; private set; }
    public Vector3 localLocation { get; private set; }
    public Vector3 centeredLocalLocation { get; private set; }
    public bool isInside { get; private set; }
    public Tile_Type tileType { get; private set; }
    public Tile_State tileState { get; private set; }
    public Tile_Access tileAccess { get; private set; }
    public Ground_Type groundType { get; private set; }
    public LocationStructure structure { get; private set; }
    public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }
    public List<LocationGridTile> neighbourList { get; private set; }
    public IPointOfInterest objHere { get; private set; }
    public List<Character> charactersHere { get; private set; }
    public bool isOccupied { get { return tileState == Tile_State.Occupied; } }
    public bool isLocked { get; private set; } //if a tile is locked, any asset on it should not be replaced.
    public TILE_OBJECT_TYPE reservedObjectType { get; private set; } //the only type of tile object that can be placed here
    public FurnitureSpot furnitureSpot { get; private set; }
    public bool hasFurnitureSpot { get; private set; }
    public List<Trait> normalTraits {
        get { return _traits; }
    }
    public LocationGridTile gridTileLocation { get { return this; } }

    private Color defaultTileColor;

    public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty || o.tileType == Tile_Type.Gate || o.tileType == Tile_Type.Road).ToList(); } }
    public List<LocationGridTile> RealisticTiles { get { return FourNeighbours().Where(o => o.tileAccess == Tile_Access.Passable && (o.structure != null || o.tileType == Tile_Type.Road || o.tileType == Tile_Type.Gate)).ToList(); } }
    public List<LocationGridTile> RoadTiles { get { return neighbours.Values.Where(o => o.tileType == Tile_Type.Road).ToList(); } }
    public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbours.Values.Where(o => !o.isOccupied && o.structure == this.structure).ToList(); } }

    public TileObject genericTileObject { get; private set; }

    public LocationGridTile(int x, int y, Tilemap tilemap, AreaInnerTileMap parentAreaMap) {
        this.parentAreaMap = parentAreaMap;
        parentTileMap = tilemap;
        localPlace = new Vector3Int(x, y, 0);
        worldLocation = tilemap.CellToWorld(localPlace);
        localLocation = tilemap.CellToLocal(localPlace);
        centeredLocalLocation = new Vector3(localLocation.x + 0.5f, localLocation.y + 0.5f, localLocation.z);
        centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
        //int xMult = worldLocation.x < 0 ? -1 : 1;
        //int yMult = worldLocation.y < 0 ? -1 : 1;
        //centeredWorldLocation = new Vector3(((int)worldLocation.x) + (0.5f * xMult), ((int)worldLocation.y) + (0.5f * yMult), worldLocation.z);
        tileType = Tile_Type.Empty;
        tileState = Tile_State.Empty;
        tileAccess = Tile_Access.Passable;
        charactersHere = new List<Character>();
        _traits = new List<Trait>();
        SetLockedState(false);
        SetReservedType(TILE_OBJECT_TYPE.NONE);
        defaultTileColor = Color.white;
    }
    public LocationGridTile(SaveDataLocationGridTile data, Tilemap tilemap, AreaInnerTileMap parentAreaMap) {
        this.parentAreaMap = parentAreaMap;
        parentTileMap = tilemap;
        localPlace = new Vector3Int((int)data.localPlace.x, (int)data.localPlace.y, 0);
        worldLocation = data.worldLocation;
        localLocation = data.localLocation;
        centeredLocalLocation = data.centeredLocalLocation;
        centeredWorldLocation = data.centeredWorldLocation;
        tileType = data.tileType;
        tileState = data.tileState;
        tileAccess = data.tileAccess;
        SetLockedState(data.isLocked);
        SetReservedType(data.reservedObjectType);
        charactersHere = new List<Character>();
        _traits = new List<Trait>();
        defaultTileColor = Color.white;
    }

    private void CreateGenericTileObject() {
        genericTileObject = new GenericTileObject(this.structure);
        genericTileObject.SetGridTileLocation(this);
        genericTileObject.DisableCollisionTrigger();
    }
    public void UpdateWorldLocation() {
        worldLocation = parentTileMap.CellToWorld(localPlace);
        centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
        //int xMult = worldLocation.x < 0 ? -1 : 1;
        //int yMult = worldLocation.y < 0 ? -1 : 1;
        //centeredWorldLocation = new Vector3(((int) worldLocation.x) + (0.5f * xMult), ((int) worldLocation.y) + (0.5f * yMult), worldLocation.z);
    }
    public List<LocationGridTile> FourNeighbours() {
        List<LocationGridTile> fn = new List<LocationGridTile>();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Key.IsCardinalDirection()) {
                fn.Add(keyValuePair.Value);
            }
        }
        return fn;
    }
    public Dictionary<TileNeighbourDirection, LocationGridTile> FourNeighboursDictionary() {
        Dictionary<TileNeighbourDirection, LocationGridTile> fn = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Key.IsCardinalDirection()) {
                fn.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
        return fn;
    }
    public void FindNeighbours(LocationGridTile[,] map) {
        neighbours = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        neighbourList = new List<LocationGridTile>();
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
                neighbourList.Add(map[result.X, result.Y]);
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
            case Tile_Type.Structure:
                if (parentTileMap.GetTile(localPlace).name.Contains("Structure Floor Tile")) { //wood
                    SetGroundType(Ground_Type.Wood);
                }
                break;
                //default:
                //    SetTileState(Tile_State.Empty);
                //    break;
        }
    }
    public void SetGroundType(Ground_Type groundType) {
        this.groundType = groundType;
        switch (groundType) {
            case Ground_Type.Grass:
            case Ground_Type.Wood:
                if (structure != null) {
                    AddTrait("Flammable");
                }
                break;
            default:
                RemoveTrait("Flammable");
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
        if (this.structure != null) {
            this.structure.RemoveTile(this);
        }
        this.structure = structure;
        this.structure.AddTile(this);
        CreateGenericTileObject();
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

    #region Characters
    public void AddCharacterHere(Character character) {
        if (!charactersHere.Contains(character)) {
            charactersHere.Add(character);
        }
    }
    public void RemoveCharacterHere(Character character) {
        charactersHere.Remove(character);
    }
    #endregion

    #region Points of Interest
    public void SetObjectHere(IPointOfInterest poi) {
        objHere = poi;
        if (hasFurnitureSpot && poi is TileObject) {
            FURNITURE_TYPE furnitureType = (poi as TileObject).tileObjectType.ConvertTileObjectToFurniture();
            if (furnitureType != FURNITURE_TYPE.NONE) {
                FurnitureSetting settings;
                if (furnitureSpot.TryGetFurnitureSettings(furnitureType, out settings)) {
                    TileBase usedAsset = InteriorMapManager.Instance.GetTileAsset(settings.tileAssetName);
                    parentAreaMap.objectsTilemap.SetTile(localPlace, usedAsset);
                    Matrix4x4 m = parentAreaMap.objectsTilemap.GetTransformMatrix(localPlace);
                    m.SetTRS(Vector3.zero, Quaternion.Euler(settings.rotation), Vector3.one);
                    parentAreaMap.objectsTilemap.SetTransformMatrix(localPlace, m);
                    if (poi is Table) {
                        (poi as Table).SetUsedAsset(usedAsset);
                    }
                }
            }
        }
        poi.SetGridTileLocation(this);
        SetTileState(Tile_State.Occupied);
        Messenger.Broadcast(Signals.OBJECT_PLACED_ON_TILE, this, poi);
    }
    public IPointOfInterest RemoveObjectHere(Character removedBy) {
        if (objHere != null) {
            IPointOfInterest removedObj = objHere;
            if (objHere is TileObject && removedBy != null) {
                //if the object in this tile is a tile object and it was removed by a character, use tile object specific function
                (objHere as TileObject).RemoveTileObject(removedBy);
            } else {
                objHere.SetGridTileLocation(null);
            }
            objHere = null;
            SetTileState(Tile_State.Empty);
            return removedObj;
        }
        return null;
    }
    #endregion

    #region Utilities
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
    public bool IsAdjacentTo(IPointOfInterest poi) {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (poi is Character) {
                if (keyValuePair.Value.charactersHere.Contains(poi)) {
                    return true;
                }
            } else {
                if (keyValuePair.Value.objHere == poi) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsAdjacentTo(System.Type type) {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if ((keyValuePair.Value.objHere != null && keyValuePair.Value.objHere.GetType() == type)) {
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
                    if (currTile != this && currTile.groundType != Ground_Type.Water) {
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
    public LocationGridTile GetRandomUnoccupiedNeighbor() {
        List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
        if (unoccupiedNeighbours.Count > 0) {
            return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
        }
        return null;
    }
    public Vector3[] GetVertices() {
        Vector3[] vertices = new Vector3[4];
        float allowance = 0.15f;
        float adjustment = 0.5f + allowance;

        vertices[0] = new Vector3(centeredWorldLocation.x - adjustment, centeredWorldLocation.y + adjustment, 0f); //top left corner
        vertices[1] = new Vector3(centeredWorldLocation.x + adjustment, centeredWorldLocation.y + adjustment, 0f); //top right corner
        vertices[2] = new Vector3(centeredWorldLocation.x + adjustment, centeredWorldLocation.y - adjustment, 0f); //bot right corner
        vertices[3] = new Vector3(centeredWorldLocation.x - adjustment, centeredWorldLocation.y - adjustment, 0f); //bot left corner
        return vertices;
    }
    public void SetLockedState(bool state) {
        isLocked = state;
    }
    public bool IsAtEdgeOfWalkableMap() {
        if ((localPlace.y == AreaInnerTileMap.southEdge && localPlace.x >= AreaInnerTileMap.westEdge && localPlace.x <= parentAreaMap.width - AreaInnerTileMap.eastEdge - 1)
            || (localPlace.y == parentAreaMap.height - AreaInnerTileMap.northEdge - 1 && localPlace.x >= AreaInnerTileMap.westEdge && localPlace.x <= parentAreaMap.width - AreaInnerTileMap.eastEdge - 1)
            || (localPlace.x == AreaInnerTileMap.westEdge && localPlace.y >= AreaInnerTileMap.southEdge && localPlace.y <= parentAreaMap.height - AreaInnerTileMap.northEdge - 1) 
            || (localPlace.x == parentAreaMap.width - AreaInnerTileMap.eastEdge - 1 && localPlace.y >= AreaInnerTileMap.southEdge && localPlace.y <= parentAreaMap.height - AreaInnerTileMap.northEdge - 1)) {
            return true;
        }
        return false;
    }
    public void HighlightTile() {
        parentAreaMap.groundTilemap.SetColor(localPlace, Color.blue);
    }
    public void UnhighlightTile() {
        parentAreaMap.groundTilemap.SetColor(localPlace, defaultTileColor);
    }
    public bool HasCardinalNeighbourOfDifferentGroundType(out Dictionary<TileNeighbourDirection, LocationGridTile> differentTiles) {
        bool hasDiff = false;
        differentTiles = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        Dictionary<TileNeighbourDirection, LocationGridTile> cardinalNeighbours = FourNeighboursDictionary();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> keyValuePair in cardinalNeighbours) {
            if (keyValuePair.Value.groundType != this.groundType) {
                hasDiff = true;
                differentTiles.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
        return hasDiff;
    }
    public void SetDefaultTileColor(Color color) {
        defaultTileColor = color;
    }
    #endregion

    #region Mouse Actions
    public void OnClickTileActions(PointerEventData.InputButton inputButton) {
        if (InteriorMapManager.Instance.IsMouseOnMarker()) {
            return;
        }
        if (objHere == null) {
#if UNITY_EDITOR
            if (inputButton == PointerEventData.InputButton.Right) {
                UIManager.Instance.poiTestingUI.ShowUI(this);
                //if (InteriorMapManager.Instance.IsHoldingPOI()) {
                //    InteriorMapManager.Instance.PlaceHeldPOI(this);
                //}
                //this.AddTrait("Burning");
            } else {
                Messenger.Broadcast(Signals.HIDE_MENUS);
            }
#else
            Messenger.Broadcast(Signals.HIDE_MENUS);
#endif
        } else if (objHere is TileObject || objHere is SpecialToken) {
#if UNITY_EDITOR
            if (inputButton == PointerEventData.InputButton.Middle) {
                if (objHere is TileObject) {
                    (objHere as TileObject).LogActionHistory();
                }
            } else if (inputButton == PointerEventData.InputButton.Right) {
                if (objHere is TileObject) {
                    UIManager.Instance.poiTestingUI.ShowUI(objHere);
                    //objHere.AddTrait("Burning");
                }
            } else {
                if (objHere is TileObject) {
                    UIManager.Instance.ShowTileObjectInfo(objHere as TileObject);
                }
            }
#else
             if (inputButton == PointerEventData.InputButton.Left) {
                if (objHere is TileObject) {
                    UIManager.Instance.ShowTileObjectInfo(objHere as TileObject);
                }
             }
#endif
        }
    }
    #endregion

    #region Tile Objects
    public void SetReservedType(TILE_OBJECT_TYPE reservedType) {
        reservedObjectType = reservedType;
    }
    #endregion

    #region Furniture Spots
    public void SetFurnitureSpot(FurnitureSpot spot) {
        furnitureSpot = spot;
        hasFurnitureSpot = true;
    }
    public FURNITURE_TYPE GetFurnitureThatCanProvide(FACILITY_TYPE facility) {
        List<FURNITURE_TYPE> choices = new List<FURNITURE_TYPE>();
        for (int i = 0; i < furnitureSpot.allowedFurnitureTypes.Length; i++) {
            FURNITURE_TYPE currType = furnitureSpot.allowedFurnitureTypes[i];
            if (currType.ConvertFurnitureToTileObject().CanProvideFacility(facility)) {
                choices.Add(currType);
            }
        }
        if (choices.Count > 0) {
            return choices[UnityEngine.Random.Range(0, choices.Count)];
        }
        throw new System.Exception("Furniture spot at " + this.ToString() + " cannot provide facility " + facility.ToString() + "! Should not reach this point if that is the case!");
    }
    #endregion

    #region Traits
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (AttributeManager.Instance.IsInstancedTrait(traitName)) {
            return AddTrait(AttributeManager.Instance.CreateNewInstancedTraitClass(traitName), characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        } else {
            return AddTrait(AttributeManager.Instance.allTraits[traitName], characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        }
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (trait.IsUnique()) {
            Trait oldTrait = GetNormalTrait(trait.name);
            if (oldTrait != null) {
                oldTrait.SetCharacterResponsibleForTrait(characterResponsible);
                oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                return false;
            }
            //return false;
        }
        _traits.Add(trait);
        trait.SetGainedFromDoing(gainedFromDoing);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        trait.AddCharacterResponsibleForTrait(characterResponsible);
        //ApplyTraitEffects(trait);
        //ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait), this);
            trait.SetExpiryTicket(this, ticket);
        }
        if (triggerOnAdd) {
            trait.OnAddTrait(this);
        }
        if (trait.IsTangible()) {
            genericTileObject?.SetGridTileLocation(this);
        }
        return true;
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true, Character removedBy = null, bool includeAlterEgo = true) {
        if (_traits.Remove(trait)) {
            trait.RemoveExpiryTicket(this);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this, removedBy);
            }
            if (!HasTangibleTrait()) {
                genericTileObject?.RemoveTileObject(removedBy);
            }
            return true;
        }
        return false;
    }
    private bool HasTangibleTrait() {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].IsTangible()) {
                return true;
            }
        }
        return false;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true, Character removedBy = null) {
        Trait trait = GetNormalTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove, removedBy);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    public List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        List<Trait> removedTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == traitType) {
                removedTraits.Add(_traits[i]);
                _traits.RemoveAt(i);
                i--;
            }
        }
        return removedTraits;
    }
    public Trait GetNormalTrait(params string[] traitNames) {
        for (int i = 0; i < _traits.Count; i++) {
            Trait trait = _traits[i];

            for (int j = 0; j < traitNames.Length; j++) {
                if (trait.name == traitNames[j] && !trait.isDisabled) {
                    return trait;
                }
            }
        }
        return null;
    }
    public void RefreshTraitExpiry(Trait trait) {
        if (trait.expiryTickets.ContainsKey(this)) {
            SchedulingManager.Instance.RemoveSpecificEntry(trait.expiryTickets[this]);
        }
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait), this);
            trait.SetExpiryTicket(this, ticket);
        }

    }
    private void RemoveAllTraits() {
        List<Trait> allTraits = new List<Trait>(normalTraits);
        for (int i = 0; i < allTraits.Count; i++) {
            RemoveTrait(allTraits[i]);
        }
    }
    public List<ITraitable> GetAllTraitablesOnTile() {
        List<ITraitable> traitables = new List<ITraitable>();
        traitables.Add(this);
        if (objHere != null) {
            traitables.Add(objHere);
        }
        for (int i = 0; i < charactersHere.Count; i++) {
            traitables.Add(charactersHere[i]);
        }
        return traitables;
    }
    public List<ITraitable> GetAllTraitablesOnTileWithTrait(string requiredTrait) {
        List<ITraitable> traitables = new List<ITraitable>();
        if (this.GetNormalTrait(requiredTrait) != null) {
            traitables.Add(this);
        }
        if (objHere != null && objHere.GetNormalTrait(requiredTrait) != null) {
            traitables.Add(objHere);
        }
        for (int i = 0; i < charactersHere.Count; i++) {
            Character character = charactersHere[i];
            if (character.GetNormalTrait(requiredTrait) != null) {
                traitables.Add(character);
            }
        }
        return traitables;
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


[System.Serializable]
public class SaveDataLocationGridTile {
    public Vector3Save localPlace; //this is the id
    public Vector3Save worldLocation;
    public Vector3Save centeredWorldLocation;
    public Vector3Save localLocation;
    public Vector3Save centeredLocalLocation;
    public LocationGridTile.Tile_Type tileType;
    public LocationGridTile.Tile_State tileState;
    public LocationGridTile.Tile_Access tileAccess;
    public LocationGridTile.Ground_Type groundType;
    //public LocationStructure structure { get; private set; }
    //public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }
    //public List<Vector3Save> neighbours;
    //public List<TileNeighbourDirection> neighbourDirections;
    public List<SaveDataTrait> traits;
    //public List<int> charactersHere;
    public int objHereID;
    public POINT_OF_INTEREST_TYPE objHereType;
    public TILE_OBJECT_TYPE objHereTileObjectType;


    public TILE_OBJECT_TYPE reservedObjectType;
    public FurnitureSpot furnitureSpot;
    public bool hasFurnitureSpot;
    public bool hasDetail;
    public bool isInside;
    public bool isLocked;

    public int structureID;
    public STRUCTURE_TYPE structureType;

    private LocationGridTile loadedGridTile;

    //tilemap assets
    public string groundTileMapAssetName;
    public string roadTileMapAssetName;
    public string wallTileMapAssetName;
    public string detailTileMapAssetName;
    public string structureTileMapAssetName;
    public string objectTileMapAssetName;

    public void Save(LocationGridTile gridTile) {
        localPlace = new Vector3Save(gridTile.localPlace);
        worldLocation = gridTile.worldLocation;
        centeredWorldLocation = gridTile.centeredWorldLocation;
        localLocation = gridTile.localLocation;
        centeredLocalLocation = gridTile.centeredLocalLocation;
        tileType = gridTile.tileType;
        tileState = gridTile.tileState;
        tileAccess = gridTile.tileAccess;
        groundType = gridTile.groundType;
        reservedObjectType = gridTile.reservedObjectType;
        furnitureSpot = gridTile.furnitureSpot;
        hasFurnitureSpot = gridTile.hasFurnitureSpot;
        hasDetail = gridTile.hasDetail;
        isInside = gridTile.isInside;
        isLocked = gridTile.isLocked;

        if(gridTile.structure != null) {
            structureID = gridTile.structure.id;
            structureType = gridTile.structure.structureType;
        } else {
            structureID = -1;
        }

        //neighbourDirections = new List<TileNeighbourDirection>();
        //neighbours = new List<Vector3Save>();
        //foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in gridTile.neighbours) {
        //    neighbourDirections.Add(kvp.Key);
        //    neighbours.Add(new Vector3Save(kvp.Value.localPlace));
        //}

        traits = new List<SaveDataTrait>();
        for (int i = 0; i < gridTile.normalTraits.Count; i++) {
            SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(gridTile.normalTraits[i]);
            if (saveDataTrait != null) {
                saveDataTrait.Save(gridTile.normalTraits[i]);
                traits.Add(saveDataTrait);
            }
        }

        if(gridTile.objHere != null) {
            objHereID = gridTile.objHere.id;
            objHereType = gridTile.objHere.poiType;
            if(gridTile.objHere is TileObject) {
                objHereTileObjectType = (gridTile.objHere as TileObject).tileObjectType;
            }
        } else {
            objHereID = -1;
        }

        //tilemap assets
        groundTileMapAssetName = gridTile.parentAreaMap.groundTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        roadTileMapAssetName = gridTile.parentAreaMap.roadTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        wallTileMapAssetName = gridTile.parentAreaMap.wallTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        detailTileMapAssetName = gridTile.parentAreaMap.detailsTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        structureTileMapAssetName = gridTile.parentAreaMap.structureTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        objectTileMapAssetName = gridTile.parentAreaMap.objectsTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
    }

    public LocationGridTile Load(Tilemap tilemap, AreaInnerTileMap parentAreaMap, Dictionary<string, TileBase> tileAssetDB) {
        LocationGridTile tile = new LocationGridTile(this, tilemap, parentAreaMap);

        if(structureID != -1) {
            LocationStructure structure = parentAreaMap.area.GetStructureByID(structureType, structureID);
            tile.SetStructure(structure);
        }

        tile.SetGroundType(groundType);
        tile.SetFurnitureSpot(furnitureSpot);
        loadedGridTile = tile;

        //load tile assets
        tile.parentAreaMap.groundTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(groundTileMapAssetName, tileAssetDB));
        tile.parentAreaMap.roadTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(roadTileMapAssetName, tileAssetDB));
        tile.parentAreaMap.wallTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(wallTileMapAssetName, tileAssetDB));
        tile.parentAreaMap.detailsTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(detailTileMapAssetName, tileAssetDB));
        tile.parentAreaMap.structureTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(structureTileMapAssetName, tileAssetDB));
        tile.parentAreaMap.objectsTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(objectTileMapAssetName, tileAssetDB));

        //TODO: hasDetail
        return tile;
    }

    public void LoadTraits() {
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            loadedGridTile.AddTrait(trait, responsibleCharacter);
        }
    }

    //This is loaded last so release loadedGridTile here
    public void LoadObjectHere() {
        if(objHereID != -1) {
            if(objHereType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                loadedGridTile.structure.AddPOI(CharacterManager.Instance.GetCharacterByID(objHereID), loadedGridTile, false);
            }

            //NOTE: Do not load item in grid tile because it is already loaded in LoadAreaItems
            //else if (objHereType == POINT_OF_INTEREST_TYPE.ITEM) {
            //    loadedGridTile.structure.AddPOI(TokenManager.Instance.GetSpecialTokenByID(objHereID), loadedGridTile);
            //}
            else if (objHereType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                TileObject obj = InteriorMapManager.Instance.GetTileObject(objHereTileObjectType, objHereID);
                if (obj == null) {
                    throw new System.Exception("Could not find object of type " + objHereTileObjectType.ToString() + " with id " + objHereID.ToString() + " at " + loadedGridTile.structure.ToString());
                }
                loadedGridTile.structure.AddPOI(obj, loadedGridTile, false);
            }
        }
        //loadedGridTile = null;
    }
}
