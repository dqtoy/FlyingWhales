﻿using PathFind;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using BayatGames.SaveGameFree.Types;
using Traits;

public class LocationGridTile : IHasNeighbours<LocationGridTile> {

    public enum Tile_Type { Empty, Wall, Structure_Entrance }
    public enum Tile_State { Empty, Occupied }
    public enum Tile_Access { Passable, Impassable, }
    public enum Ground_Type { Soil, Grass, Stone, Snow, Tundra, Cobble, Wood, Snow_Dirt, Water }
    public bool hasDetail { get; set; }
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
    public Dictionary<GridNeighbourDirection, LocationGridTile> neighbours { get; private set; }
    public List<LocationGridTile> neighbourList { get; private set; }
    public IPointOfInterest objHere { get; private set; }
    public List<Character> charactersHere { get; private set; }
    public bool isOccupied { get { return tileState == Tile_State.Occupied; } }
    public bool isLocked { get; private set; } //if a tile is locked, any asset on it should not be replaced.
    public TILE_OBJECT_TYPE reservedObjectType { get; private set; } //the only type of tile object that can be placed here
    public FurnitureSpot furnitureSpot { get; private set; }
    public bool hasFurnitureSpot { get; private set; }
    public List<Trait> normalTraits {
        get { return genericTileObject.traitContainer.allTraits; }
    }
    public bool hasBlueprint { get; private set; }

    private Color defaultTileColor;

    public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty).ToList(); } }
    public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbours.Values.Where(o => !o.isOccupied && o.structure == this.structure).ToList(); } }

    public GenericTileObject genericTileObject { get; private set; }
    public List<WallObject> walls { get; private set; }

    public LocationGridTile(int x, int y, Tilemap tilemap, AreaInnerTileMap parentAreaMap) {
        this.parentAreaMap = parentAreaMap;
        parentTileMap = tilemap;
        localPlace = new Vector3Int(x, y, 0);
        worldLocation = tilemap.CellToWorld(localPlace);
        localLocation = tilemap.CellToLocal(localPlace);
        centeredLocalLocation = new Vector3(localLocation.x + 0.5f, localLocation.y + 0.5f, localLocation.z);
        centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
        tileType = Tile_Type.Empty;
        tileState = Tile_State.Empty;
        tileAccess = Tile_Access.Passable;
        charactersHere = new List<Character>();
        walls = new List<WallObject>();
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
        walls = new List<WallObject>();
        defaultTileColor = Color.white;
    }

    public void CreateGenericTileObject() {
        genericTileObject = new GenericTileObject();
    }
    public void UpdateWorldLocation() {
        worldLocation = parentTileMap.CellToWorld(localPlace);
        centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
    }
    public List<LocationGridTile> FourNeighbours() {
        List<LocationGridTile> fn = new List<LocationGridTile>();
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Key.IsCardinalDirection()) {
                fn.Add(keyValuePair.Value);
            }
        }
        return fn;
    }
    public Dictionary<GridNeighbourDirection, LocationGridTile> FourNeighboursDictionary() {
        Dictionary<GridNeighbourDirection, LocationGridTile> fn = new Dictionary<GridNeighbourDirection, LocationGridTile>();
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Key.IsCardinalDirection()) {
                fn.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
        return fn;
    }
    public void FindNeighbours(LocationGridTile[,] map) {
        neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
        neighbourList = new List<LocationGridTile>();
        int mapUpperBoundX = map.GetUpperBound(0);
        int mapUpperBoundY = map.GetUpperBound(1);
        Point thisPoint = new Point(localPlace.x, localPlace.y);
        foreach (KeyValuePair<GridNeighbourDirection, Point> kvp in possibleExits) {
            GridNeighbourDirection currDir = kvp.Key;
            Point exit = kvp.Value;
            Point result = exit.Sum(thisPoint);
            if (Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                neighbours.Add(currDir, map[result.X, result.Y]);
                neighbourList.Add(map[result.X, result.Y]);
            }

        }
    }
    public Dictionary<GridNeighbourDirection, Point> possibleExits {
        get {
            return new Dictionary<GridNeighbourDirection, Point>() {
                {GridNeighbourDirection.North, new Point(0,1) },
                {GridNeighbourDirection.South, new Point(0,-1) },
                {GridNeighbourDirection.West, new Point(-1,0) },
                {GridNeighbourDirection.East, new Point(1,0) },
                {GridNeighbourDirection.North_West, new Point(-1,1) },
                {GridNeighbourDirection.North_East, new Point(1,1) },
                {GridNeighbourDirection.South_West, new Point(-1,-1) },
                {GridNeighbourDirection.South_East, new Point(1,-1) },
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
        }
    }
    private void SetGroundType(Ground_Type groundType) {
        this.groundType = groundType;
        if (genericTileObject != null) {
            switch (groundType) {
                case Ground_Type.Grass:
                case Ground_Type.Wood:
                    genericTileObject.traitContainer.AddTrait(genericTileObject, "Flammable");
                    break;
                default:
                    genericTileObject.traitContainer.RemoveTrait(genericTileObject, "Flammable");
                    break;
            }
        }
    }
    private void UpdateGroundTypeBasedOnAsset() {
        TileBase groundAsset = parentAreaMap.groundTilemap.GetTile(this.localPlace);
        if (groundAsset != null) {
            string assetName = groundAsset.name.ToLower();
            if (assetName.Contains("structure floor")) {
                SetGroundType(Ground_Type.Wood);
            } else if (assetName.Contains("cobble")) {
                SetGroundType(Ground_Type.Cobble);
            } else if (assetName.Contains("water") || assetName.Contains("pond")) {
                SetGroundType(Ground_Type.Water);
            } else if (assetName.Contains("snow")) {
                if (assetName.Contains("dirt")) {
                    SetGroundType(Ground_Type.Snow_Dirt);
                } else {
                    SetGroundType(Ground_Type.Snow);
                }
            } else if (assetName.Contains("stone")) {
                SetGroundType(Ground_Type.Stone);
            } else if (assetName.Contains("grass")) {
                SetGroundType(Ground_Type.Grass);
            } else if (assetName.Contains("soil")) {
                SetGroundType(Ground_Type.Soil);
            } else if (assetName.Contains("tundra")) {
                SetGroundType(Ground_Type.Tundra);
                //override tile to use tundra soil
                parentAreaMap.groundTilemap.SetTile(this.localPlace, parentAreaMap.tundraSoilAsset);
            } else if ((assetName.Contains("Dirt") || assetName.Contains("dirt")) && (parentAreaMap.area.coreTile.biomeType == BIOMES.SNOW || parentAreaMap.area.coreTile.biomeType == BIOMES.TUNDRA)) {
                SetGroundType(Ground_Type.Tundra);
                //override tile to use tundra soil
                parentAreaMap.groundTilemap.SetTile(this.localPlace, parentAreaMap.tundraSoilAsset);
            }
        }
    }
    public bool TryGetNeighbourDirection(LocationGridTile tile, out GridNeighbourDirection dir) {
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Value == tile) {
                dir = keyValuePair.Key;
                return true;
            }
        }
        dir = GridNeighbourDirection.East;
        return false;
    }

    #region Visuals
    public void SetGroundTilemapVisual(TileBase tileBase) {
        parentAreaMap.groundTilemap.SetTile(this.localPlace, tileBase);
        UpdateGroundTypeBasedOnAsset();
    }
    #endregion

    #region Structures
    public void SetStructure(LocationStructure structure) {
        if (this.structure != null) {
            this.structure.RemoveTile(this);
        }
        this.structure = structure;
        this.structure.AddTile(this);
        if (!genericTileObject.hasBeenInitialized) { //TODO: Make this better
            genericTileObject.ManualInitialize(structure, this);
        }
    }
    public void SetTileState(Tile_State state) {
        if (structure != null) {
            if (this.tileState == Tile_State.Empty && state == Tile_State.Occupied) {
                structure.RemoveUnoccupiedTile(this);
            } else if (this.tileState == Tile_State.Occupied && state == Tile_State.Empty && reservedObjectType == TILE_OBJECT_TYPE.NONE) {
                structure.AddUnoccupiedTile(this);
            }
        }
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
        GridNeighbourDirection[] dirs = Utilities.GetEnumValues<GridNeighbourDirection>();
        for (int i = 0; i < dirs.Length; i++) {
            if (!neighbours.ContainsKey(dirs[i])) {
                return true;
            }
        }
        return false;
    }
    public bool HasNeighborAtEdgeOfMap() {
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
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
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (!kvp.Value.isInside || (kvp.Value.structure != this.structure)) {
                return true;
            }
        }
        return false;
    }
    public bool IsAdjacentToWall() {
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
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
        Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
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
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Value == tile) {
                return true;
            }
        }
        return false;
    }
    public bool IsAdjacentTo(System.Type type) {
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if ((keyValuePair.Value.objHere != null && keyValuePair.Value.objHere.GetType() == type)) {
                return true;
            }
        }
        return false;
    }
    public bool IsAdjacentToPasssableTiles(int count = 1) {
        int passableCount = 0;
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
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
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
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
    public bool HasNeighbouringWalledStructure() {
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
            if (keyValuePair.Value.structure != null && keyValuePair.Value.structure.structureType.IsOpenSpace() == false) {
                return true;
            }
        }
        return false;
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
    public bool HasCardinalNeighbourOfDifferentGroundType(out Dictionary<GridNeighbourDirection, LocationGridTile> differentTiles) {
        bool hasDiff = false;
        differentTiles = new Dictionary<GridNeighbourDirection, LocationGridTile>();
        Dictionary<GridNeighbourDirection, LocationGridTile> cardinalNeighbours = FourNeighboursDictionary();
        foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in cardinalNeighbours) {
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
    public List<ITraitable> GetTraitablesOnTileWithTrait(string requiredTrait) {
        List<ITraitable> traitables = new List<ITraitable>();
        if (genericTileObject.traitContainer.GetNormalTrait(requiredTrait) != null) {
            traitables.Add(genericTileObject);
        }
        for (int i = 0; i < walls.Count; i++) {
            WallObject wallObject = walls[i];
            if (wallObject.traitContainer.GetNormalTrait(requiredTrait) != null) {
                traitables.Add(wallObject);
            }
        }
        if (objHere != null && objHere.traitContainer.GetNormalTrait(requiredTrait) != null) {
            if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)
                || (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                traitables.Add(objHere);
            }
        }
        for (int i = 0; i < charactersHere.Count; i++) {
            Character character = charactersHere[i];
            if (character.traitContainer.GetNormalTrait(requiredTrait) != null) {
                traitables.Add(character);
            }
        }
        return traitables;
    }
    public List<IDamageable> GetDamageablesOnTile() {
        List<IDamageable> damagables = new List<IDamageable>();
        if (structure.structureType.IsOpenSpace() == false) {
            //only add floor and walls if structure is not open space
            damagables.Add(genericTileObject);
            for (int i = 0; i < walls.Count; i++) {
                WallObject wallObject = walls[i];
                damagables.Add(wallObject);
            }
        }
        
        if (objHere != null) {
            if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)
                || (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                damagables.Add(objHere);
            }
        }
        for (int i = 0; i < charactersHere.Count; i++) {
            Character character = charactersHere[i];
            damagables.Add(character);
        }
        return damagables;
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
            if (inputButton == PointerEventData.InputButton.Right) {
                if (objHere is TileObject) {
                    UIManager.Instance.poiTestingUI.ShowUI(objHere);
                    structure.RemovePOI(objHere);
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
        if (structure != null) {
            if (this.reservedObjectType != TILE_OBJECT_TYPE.NONE && reservedType == TILE_OBJECT_TYPE.NONE && tileState == Tile_State.Empty) {
                structure.AddUnoccupiedTile(this);
            } else if (this.reservedObjectType == TILE_OBJECT_TYPE.NONE && reservedType != TILE_OBJECT_TYPE.NONE) {
                structure.RemoveUnoccupiedTile(this);
            }
        }
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
        if (furnitureSpot.allowedFurnitureTypes != null) {
            for (int i = 0; i < furnitureSpot.allowedFurnitureTypes.Length; i++) {
                FURNITURE_TYPE currType = furnitureSpot.allowedFurnitureTypes[i];
                if (currType.ConvertFurnitureToTileObject().CanProvideFacility(facility)) {
                    choices.Add(currType);
                }
            }
            if (choices.Count > 0) {
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            }
        }
        throw new System.Exception("Furniture spot at " + this.ToString() + " cannot provide facility " + facility.ToString() + "! Should not reach this point if that is the case!");
    }
    #endregion

    #region Building
    public void SetHasBlueprint(bool hasBlueprint) {
        this.hasBlueprint = hasBlueprint;
    }
    #endregion

    #region Walls
    public void AddWallObject(WallObject wallObject) {
        walls.Add(wallObject);
    }
    public void RemoveWallObject(WallObject wallObject) {
        walls.Remove(wallObject);
    }
    public void ClearWallObjects() {
        walls.Clear();
    }
    #endregion
}

[System.Serializable]
public struct TwoTileDirections {
    public GridNeighbourDirection from;
    public GridNeighbourDirection to;

    public TwoTileDirections(GridNeighbourDirection from, GridNeighbourDirection to) {
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

    public Matrix4x4 groundTileMapMatrix;
    public Matrix4x4 roadTileMapMatrix;
    public Matrix4x4 wallTileMapMatrix;
    public Matrix4x4 detailTileMapMatrix;
    public Matrix4x4 structureTileMapMatrix;
    public Matrix4x4 objectTileMapMatrix;

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
        detailTileMapAssetName = gridTile.parentAreaMap.detailsTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        structureTileMapAssetName = gridTile.parentAreaMap.structureTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;

        groundTileMapMatrix = gridTile.parentAreaMap.groundTilemap.GetTransformMatrix(gridTile.localPlace);
        detailTileMapMatrix = gridTile.parentAreaMap.detailsTilemap.GetTransformMatrix(gridTile.localPlace);
        structureTileMapMatrix = gridTile.parentAreaMap.structureTilemap.GetTransformMatrix(gridTile.localPlace);
    }

    public LocationGridTile Load(Tilemap tilemap, AreaInnerTileMap parentAreaMap, Dictionary<string, TileBase> tileAssetDB) {
        LocationGridTile tile = new LocationGridTile(this, tilemap, parentAreaMap);

        if(structureID != -1) {
            LocationStructure structure = parentAreaMap.area.GetStructureByID(structureType, structureID);
            tile.SetStructure(structure);
        }

        //tile.SetGroundType(groundType);
        if (hasFurnitureSpot) {
            tile.SetFurnitureSpot(furnitureSpot);
        }
        loadedGridTile = tile;

        //load tile assets
        tile.SetGroundTilemapVisual(InteriorMapManager.Instance.TryGetTileAsset(groundTileMapAssetName, tileAssetDB));
        tile.parentAreaMap.detailsTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(detailTileMapAssetName, tileAssetDB));
        tile.parentAreaMap.structureTilemap.SetTile(tile.localPlace, InteriorMapManager.Instance.TryGetTileAsset(structureTileMapAssetName, tileAssetDB));

        tile.parentAreaMap.groundTilemap.SetTransformMatrix(tile.localPlace, groundTileMapMatrix);
        tile.parentAreaMap.detailsTilemap.SetTransformMatrix(tile.localPlace, detailTileMapMatrix);
        tile.parentAreaMap.structureTilemap.SetTransformMatrix(tile.localPlace, structureTileMapMatrix);

        return tile;
    }

    public void LoadTraits() {
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            loadedGridTile.genericTileObject.traitContainer.AddTrait(loadedGridTile.genericTileObject, trait, responsibleCharacter);
        }
    }

    //This is loaded last so release loadedGridTile here
    public void LoadObjectHere() {
        if(objHereID != -1) {
            if(objHereType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                loadedGridTile.structure.AddPOI(CharacterManager.Instance.GetCharacterByID(objHereID), loadedGridTile);
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
                loadedGridTile.structure.AddPOI(obj, loadedGridTile);
            }
        }
        //loadedGridTile = null;
    }
}
