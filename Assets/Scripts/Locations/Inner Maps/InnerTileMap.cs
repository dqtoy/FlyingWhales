using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
namespace Inner_Maps {
    public abstract class InnerTileMap : MonoBehaviour {
        
        public static int WestEdge = 0;
        public static int NorthEdge = 0;
        public static int SouthEdge = 0;
        public static int EastEdge = 0;
        
        [Header("Tile Maps")]
        public Tilemap groundTilemap;
        public Tilemap detailsTilemap;
        public Tilemap structureTilemap;
        
        [Header("Seamless Edges")]
        public Tilemap northEdgeTilemap;
        public Tilemap southEdgeTilemap;
        public Tilemap westEdgeTilemap;
        public Tilemap eastEdgeTilemap;
        
        [Header("Parents")]
        public Transform objectsParent;
        public Transform structureParent;
        [FormerlySerializedAs("worldUICanvas")] public Canvas worldUiCanvas;
        public Grid grid;
        
        [Header("Other")]
        [FormerlySerializedAs("centerGOPrefab")] public GameObject centerGoPrefab;
        public Vector4 cameraBounds;
        
        [Header("Structures")]
        [SerializeField] protected GameObject buildSpotPrefab;
        
        public int width { get; set; }
        public int height { get; set; }
        public LocationGridTile[,] map { get; private set; }
        public List<LocationGridTile> allTiles { get; private set; }
        public List<LocationGridTile> allEdgeTiles { get; private set; }
        public ILocation location { get; private set; }
        public GridGraph pathfindingGraph { get; set; }
        public Vector3 worldPos { get; private set; }
        public GameObject centerGo { get; private set; }
        public abstract bool isSettlementMap { get; }
        public List<BurningSource> activeBurningSources { get; private set; }
        public BuildingSpot[,] buildingSpots { get; protected set; }
        
        public virtual void Initialize(ILocation location) {
            this.location = location;
            activeBurningSources = new List<BurningSource>();
        }
        protected IEnumerator GenerateGrid(int width, int height) {
            this.width = width;
            this.height = height;

            map = new LocationGridTile[width, height];
            allTiles = new List<LocationGridTile>();
            allEdgeTiles = new List<LocationGridTile>();
            int batchCount = 0;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), GetOutsideFloorTile(location));
                    LocationGridTile tile = new LocationGridTile(x, y, groundTilemap, this);
                    allTiles.Add(tile);
                    if (tile.IsAtEdgeOfWalkableMap()) {
                        allEdgeTiles.Add(tile);
                    }
                    map[x, y] = tile;
                }
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                    batchCount = 0;
                    yield return null;    
                }
            }
            allTiles.ForEach(x => x.FindNeighbours(map));
        }

        #region Loading
        protected void LoadGrid(SaveDataAreaInnerTileMap data) {
            map = new LocationGridTile[width, height];
            allTiles = new List<LocationGridTile>();
            allEdgeTiles = new List<LocationGridTile>();

            Dictionary<string, TileBase> tileDb = InnerMapManager.Instance.GetTileAssetDatabase();

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    //groundTilemap.SetTile(new Vector3Int(x, y, 0), GetOutsideFloorTileForArea(area));
                    LocationGridTile tile = data.map[x][y].Load(groundTilemap, this, tileDb);
                    allTiles.Add(tile);
                    if (tile.IsAtEdgeOfWalkableMap()) {
                        allEdgeTiles.Add(tile);
                    }
                    map[x, y] = tile;
                }
            }
            allTiles.ForEach(x => x.FindNeighbours(map));

            groundTilemap.RefreshAllTiles();
        }
        #endregion
        
        #region Visuals
        public void ClearAllTilemaps() {
            Tilemap[] maps = GetComponentsInChildren<Tilemap>();
            for (var i = 0; i < maps.Length; i++) {
                maps[i].ClearAllTiles();
            }
        }
        protected TileBase GetOutsideFloorTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowOutsideTile;
                default:
                    return InnerMapManager.Instance.assetManager.outsideTile;
            }
        }
        protected TileBase GetBigTreeTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowBigTreeTile;
                default:
                    return InnerMapManager.Instance.assetManager.bigTreeTile;
            }
        }
        protected TileBase GetTreeTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowTreeTile;
                default:
                    return InnerMapManager.Instance.assetManager.treeTile;
            }
        }
        protected TileBase GetFlowerTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowFlowerTile;
                default:
                    return InnerMapManager.Instance.assetManager.flowerTile;
            }
        }
        protected TileBase GetGarbTile(ILocation location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return InnerMapManager.Instance.assetManager.snowGarbTile;
                default:
                    return InnerMapManager.Instance.assetManager.randomGarbTile;
            }
        }
        protected IEnumerator CreateSeamlessEdges() {
            int batchCount = 0;
            for (int i = 0; i < allTiles.Count; i++) {
                LocationGridTile tile = allTiles[i];
                if (tile.structure != null && !tile.structure.structureType.IsOpenSpace()) { continue; } //skip non open space structure tiles.
                tile.CreateSeamlessEdgesForTile(this);
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapSeamlessEdgeBatches) {
                    batchCount = 0;
                    yield return null;
                }
            }
        }
        #endregion

        #region Data Getting
        public List<LocationGridTile> GetUnoccupiedTilesInRadius(LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
            int mapSizeX = map.GetUpperBound(0);
            int mapSizeY = map.GetUpperBound(1);
            int x = centerTile.localPlace.x;
            int y = centerTile.localPlace.y;
            if (includeCenterTile) {
                tiles.Add(centerTile);
            }

            int xLimitLower = x - radiusLimit;
            int xLimitUpper = x + radiusLimit;
            int yLimitLower = y - radiusLimit;
            int yLimitUpper = y + radiusLimit;

            for (int dx = x - radius; dx <= x + radius; dx++) {
                for (int dy = y - radius; dy <= y + radius; dy++) {
                    if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                        if (dx == x && dy == y) {
                            continue;
                        }
                        if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                            continue;
                        }
                        LocationGridTile result = map[dx, dy];
                        if ((!includeTilesInDifferentStructure && result.structure != centerTile.structure) || result.isOccupied || result.charactersHere.Count > 0) { continue; }
                        tiles.Add(result);
                    }
                }
            }
            return tiles;
        }
        public List<LocationGridTile> GetTilesInRadius(LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
            int mapSizeX = map.GetUpperBound(0);
            int mapSizeY = map.GetUpperBound(1);
            int x = centerTile.localPlace.x;
            int y = centerTile.localPlace.y;
            if (includeCenterTile) {
                tiles.Add(centerTile);
            }
            int xLimitLower = x - radiusLimit;
            int xLimitUpper = x + radiusLimit;
            int yLimitLower = y - radiusLimit;
            int yLimitUpper = y + radiusLimit;


            for (int dx = x - radius; dx <= x + radius; dx++) {
                for (int dy = y - radius; dy <= y + radius; dy++) {
                    if(dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                        if(dx == x && dy == y) {
                            continue;
                        }
                        if(radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                            continue;
                        }
                        LocationGridTile result = map[dx, dy];
                        if(result.structure == null) { continue; } //do not include tiles with no structures
                        if(!includeTilesInDifferentStructure && result.structure != centerTile.structure) { continue; }
                        tiles.Add(result);
                    }
                }
            }
            return tiles;
        }
        public LocationGridTile GetRandomUnoccupiedEdgeTile() {
            List<LocationGridTile> unoccupiedEdgeTiles = new List<LocationGridTile>();
            for (int i = 0; i < allEdgeTiles.Count; i++) {
                if (!allEdgeTiles[i].isOccupied && allEdgeTiles[i].structure != null) { // - There should not be a checker for structure, fix the generation of allEdgeTiles in AreaInnerTileMap's GenerateGrid
                    unoccupiedEdgeTiles.Add(allEdgeTiles[i]);
                }
            }
            if (unoccupiedEdgeTiles.Count > 0) {
                return unoccupiedEdgeTiles[UnityEngine.Random.Range(0, unoccupiedEdgeTiles.Count)];
            }
            return null;
        }
        #endregion
        
        #region Points of Interest
        public void PlaceObject(IPointOfInterest obj, LocationGridTile tile, bool placeAsset = true) {
            switch (obj.poiType) {
                case POINT_OF_INTEREST_TYPE.CHARACTER:
                    OnPlaceCharacterOnTile(obj as Character, tile);
                    break;
                default:
                    tile.SetObjectHere(obj);
                    break;
            }
        }
        public void RemoveObject(LocationGridTile tile, Character removedBy = null) {
            tile.RemoveObjectHere(removedBy);
        }
        public void RemoveObjectWithoutDestroying(LocationGridTile tile) {
            tile.RemoveObjectHereWithoutDestroying();
        }
        private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
            GameObject markerGO = character.marker.gameObject; 
            if (markerGO.transform.parent != objectsParent) {
                //This means that the character travelled to a different area
                markerGO.transform.SetParent(objectsParent);
                markerGO.transform.localPosition = tile.centeredLocalLocation;
                // character.marker.UpdatePosition();
            }

            if (!character.marker.gameObject.activeSelf) {
                character.marker.gameObject.SetActive(true);
            }
        }
        public void OnCharacterMovedTo(Character character, LocationGridTile to, LocationGridTile from) {
            if (from == null) { 
                //from is null (Usually happens on start up, should not happen otherwise)
                to.AddCharacterHere(character);
                to.structure.AddCharacterAtLocation(character);
            } else {
                if (to.structure == null) {
                    return; //quick fix for when the character is pushed to a tile with no structure
                }
                from.RemoveCharacterHere(character);
                to.AddCharacterHere(character);
                if (from.structure != to.structure) {
                    @from.structure?.RemoveCharacterAtLocation(character);
                    if (to.structure != null) {
                        to.structure.AddCharacterAtLocation(character);
                    } else {
                        throw new System.Exception(character.name + " is going to tile " + to.ToString() + " which does not have a structure!");
                    }
                
                }
            }
        
        }
        #endregion

        #region Data Setting
        public void UpdateTilesWorldPosition() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    map[x, y].UpdateWorldLocation();
                }
            }
            SetWorldPosition();
        }
        public void SetWorldPosition() {
            worldPos = transform.position;
        }
        #endregion

        #region Burning Source
        public void AddActiveBurningSource(BurningSource bs) {
            if (!activeBurningSources.Contains(bs)) {
                activeBurningSources.Add(bs);
            }
        }
        public void RemoveActiveBurningSources(BurningSource bs) {
            activeBurningSources.Remove(bs);
        }
        public void LoadBurningSources(List<SaveDataBurningSource> sources) {
            //for (int i = 0; i < sources.Count; i++) {
            //    SaveDataBurningSource data = sources[i];
            //    BurningSource bs = new BurningSource(area, data);
            //}
        }
        #endregion

        #region Utilities
        public void CleanUp() {
            Utilities.DestroyChildren(objectsParent);
        }
        public HexTile GetHexTileInRegionThatTileBelongsTo(LocationGridTile tile) {
            int localX = tile.localPlace.x / 14;
            int localY = tile.localPlace.y / 14;

            return location.coreTile.region.hexTileMap[localX, localY];
        }
        public void Open() { }
        public void Close() { }
        public void OnMapGenerationFinished() {
            this.name = location.name + "'s Inner Map";
            worldUiCanvas.worldCamera = InnerMapCameraMove.Instance.innerMapsCamera;
            var orthographicSize = InnerMapCameraMove.Instance.innerMapsCamera.orthographicSize;
            cameraBounds = new Vector4 {x = -185.8f}; //x - minX, y - minY, z - maxX, w - maxY 
            cameraBounds.y = orthographicSize;
            cameraBounds.z = (cameraBounds.x + width) - 28.5f;
            cameraBounds.w = height - orthographicSize;
            SpawnCenterGo();
        }
        private void SpawnCenterGo() {
            centerGo = GameObject.Instantiate<GameObject>(centerGoPrefab, transform);
            centerGo.transform.position = new Vector3((cameraBounds.x + cameraBounds.z) * 0.5f, (cameraBounds.y + cameraBounds.w) * 0.5f);
        }
        #endregion

        #region Building Spots
        private BuildingSpot GetRandomOpenBuildingSpot(HexTile tile) {
            List<BuildingSpot> choices = GetOpenBuildingSpots(tile);
            if (choices.Count > 0) {
                return Utilities.GetRandomElement(choices);
            }
            return null;
        }
        private bool TryGetValidBuildSpotForStructure(LocationStructureObject structureObject, HexTile tile, out BuildingSpot buildingSpot) {
            if (structureObject.IsBiggerThanBuildSpot()) {
                List<BuildingSpot> openSpots = GetOpenBuildingSpots(tile);
                if (openSpots.Count > 0) {
                    List<BuildingSpot> choices = new List<BuildingSpot>();
                    for (int i = 0; i < openSpots.Count; i++) {
                        BuildingSpot buildSpot = openSpots[i];
                        if (buildSpot.CanPlaceStructureOnSpot(structureObject, this, tile)) {
                            choices.Add(buildSpot);
                        }
                    }
                    if (choices.Count > 0) {
                        buildingSpot = Utilities.GetRandomElement(choices);
                        return true;
                    }
                }
                //could not find any spots
                buildingSpot = null;
                return false;
            } else {
                //if the object does not exceed the size of a build spot, then just give it a random open build spot
                buildingSpot = GetRandomOpenBuildingSpot(tile);
                return buildingSpot != null;
            }
        }
        protected BuildingSpot GetRandomBuildingSpotAtCenter(int allowance) {
            int upperBoundX = buildingSpots.GetUpperBound(0);
            int upperBoundY = buildingSpots.GetUpperBound(1);

            int centerX = upperBoundX / 2;
            int centerY = upperBoundY / 2;

            List<BuildingSpot> choices = new List<BuildingSpot>();
            for (int x = 0; x < upperBoundX; x++) {
                for (int y = 0; y < upperBoundY; y++) {
                    bool isXSatisfied = x == centerX || Utilities.IsInRange(x, centerX - allowance, centerX + allowance);
                    bool isYSatisfied = y == centerY || Utilities.IsInRange(y, centerY - allowance, centerY + allowance);
                    if (isXSatisfied && isYSatisfied) {
                        BuildingSpot currSpot = buildingSpots[x, y];
                        choices.Add(currSpot);
                    }
                }
            }
            return Utilities.GetRandomElement(choices);
        }
        private List<BuildingSpot> GetOpenBuildingSpots(HexTile tile) {
            List<BuildingSpot> open = new List<BuildingSpot>();
            for (int x = 0; x < buildingSpots.GetUpperBound(0); x++) {
                for (int y = 0; y < buildingSpots.GetUpperBound(1); y++) {
                    BuildingSpot currSpot = buildingSpots[x, y];
                    //only get open spots that belong to the provided hexTile
                    if (currSpot.isOpen && GetHexTileInRegionThatTileBelongsTo(currSpot.tilesInTerritory.First()) == tile) {
                        open.Add(currSpot);
                    }
                }
            }
            return open;
        }
        private BuildSpotTileObject GetRandomOpenBuildSpotTileObject() {
            List<BuildSpotTileObject> choices = GetOpenBuildSpotTileObjects();
            if (choices.Count > 0) {
                return Utilities.GetRandomElement(choices);
            }
            return null;
        }
        public bool TryGetValidBuildSpotTileObjectForStructure(LocationStructureObject structureObject, HexTile hexTile, out BuildSpotTileObject buildingSpot) {
            if (structureObject.IsBiggerThanBuildSpot()) {
                List<BuildSpotTileObject> openSpots = GetOpenBuildSpotTileObjects();
                if (openSpots.Count > 0) {
                    List<BuildSpotTileObject> choices = new List<BuildSpotTileObject>();
                    for (int i = 0; i < openSpots.Count; i++) {
                        BuildSpotTileObject buildSpot = openSpots[i];
                        if (buildSpot.spot.CanPlaceStructureOnSpot(structureObject, this, hexTile)) {
                            choices.Add(buildSpot);
                        }
                    }
                    if (choices.Count > 0) {
                        buildingSpot = Utilities.GetRandomElement(choices);
                        return true;
                    }
                }
                //could not find any spots
                buildingSpot = null;
                return false;
            } else {
                //if the object does not exceed the size of a build spot, then just give it a random open build spot
                buildingSpot = GetRandomOpenBuildSpotTileObject();
                return buildingSpot != null;
            }
        }
        private List<BuildSpotTileObject> GetOpenBuildSpotTileObjects() {
            List<BuildSpotTileObject> spots = location.coreTile.region.GetTileObjectsOfType(TILE_OBJECT_TYPE.BUILD_SPOT_TILE_OBJECT).Select(x => x as BuildSpotTileObject).ToList();
            List<BuildSpotTileObject> open = new List<BuildSpotTileObject>();
            for (int i = 0; i < spots.Count; i++) {
                BuildSpotTileObject buildSpotTileObject = spots[i];
                if (buildSpotTileObject.spot.isOpen) {
                    open.Add(buildSpotTileObject);
                }
            }
            return open;
        }
        public bool IsBuildSpotValidFor(LocationStructureObject structureObject, BuildingSpot spot, HexTile hexTile) {
            bool isHorizontallyBig = structureObject.IsHorizontallyBig();
            bool isVerticallyBig = structureObject.IsVerticallyBig();
            BuildingSpot currSpot = spot;
            if (isHorizontallyBig && isVerticallyBig) {
                //if it is bigger both horizontally and vertically
                //only get build spots that do not have any occupied adjacent spots at their top and right
                bool hasUnoccupiedNorth = currSpot.neighbours.ContainsKey(GridNeighbourDirection.North) 
                                          && currSpot.neighbours[GridNeighbourDirection.North].isOccupied == false
                                          && GetHexTileInRegionThatTileBelongsTo(currSpot.neighbours[GridNeighbourDirection.North].tilesInTerritory.First()) == hexTile;
                bool hasUnoccupiedEast = currSpot.neighbours.ContainsKey(GridNeighbourDirection.East) 
                                         && currSpot.neighbours[GridNeighbourDirection.East].isOccupied == false
                                         && GetHexTileInRegionThatTileBelongsTo(currSpot.neighbours[GridNeighbourDirection.East].tilesInTerritory.First()) == hexTile;
                if (hasUnoccupiedNorth && hasUnoccupiedEast) {
                    return true;
                }
            } else if (isHorizontallyBig) {
                //if it is bigger horizontally
                //only get build spots that do not have any occupied adjacent spots at their right
                bool hasUnoccupiedEast = currSpot.neighbours.ContainsKey(GridNeighbourDirection.East) 
                                         && currSpot.neighbours[GridNeighbourDirection.East].isOccupied == false
                                         && GetHexTileInRegionThatTileBelongsTo(currSpot.neighbours[GridNeighbourDirection.East].tilesInTerritory.First()) == hexTile;
                if (hasUnoccupiedEast) {
                    return true;
                }
            } else if (isVerticallyBig) {
                //if it is bigger vertically
                //only get build spots that do not have any occupied adjacent spots at their top
                bool hasUnoccupiedNorth = currSpot.neighbours.ContainsKey(GridNeighbourDirection.North) 
                                          && currSpot.neighbours[GridNeighbourDirection.North].isOccupied == false
                                          && GetHexTileInRegionThatTileBelongsTo(currSpot.neighbours[GridNeighbourDirection.North].tilesInTerritory.First()) == hexTile;
                if (hasUnoccupiedNorth) {
                    return true;
                }
            } else {
                //object is not big
                return true;
            }
            return false;
        }
        #endregion

        #region Structures
        public void PlaceStructure(STRUCTURE_TYPE type, HexTile hexTile) {
            LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(location, type);
            List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structure.structureType);
            GameObject chosenStructurePrefab = Utilities.GetRandomElement(choices);
            LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
            BuildingSpot chosenBuildingSpot;
            if (TryGetValidBuildSpotForStructure(lso, hexTile, out chosenBuildingSpot) == false) {
                chosenBuildingSpot = GetRandomOpenBuildingSpot(hexTile);
                if (structure.structureType != STRUCTURE_TYPE.CITY_CENTER) {
                    throw new System.Exception($"There was no valid spot to place {structure.ToString()} using prefab {chosenStructurePrefab.name} so it was placed at a random spot in the center");
                }
                Debug.LogWarning($"There was no valid spot to place {structure.ToString()} uso it was placed at a random spot in the center");
            }
            if (chosenBuildingSpot == null) {
                throw new System.Exception($"Could not find valid building spot for { structure.ToString() } using prefab { chosenStructurePrefab.name }");
            } else {
                PlaceStructureObjectAt(chosenBuildingSpot, chosenStructurePrefab, structure);
            }
        }
        private void PlaceStructureObjectAt(BuildingSpot chosenBuildingSpot, GameObject structurePrefab, LocationStructure structure) {
            GameObject structureGo = ObjectPoolManager.Instance.InstantiateObjectFromPool(structurePrefab.name, Vector3.zero, Quaternion.identity, structureParent);
            LocationStructureObject structureObjectPrefab = structureGo.GetComponent<LocationStructureObject>();
            structureGo.transform.localPosition = chosenBuildingSpot.GetPositionToPlaceStructure(structureObjectPrefab);
        
            LocationStructureObject structureObject = structureGo.GetComponent<LocationStructureObject>();
            structureObject.RefreshAllTilemaps();
            List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(this);
            structureObject.SetTilesInStructure(occupiedTiles.ToArray());

            structureObject.ClearOutUnimportantObjectsBeforePlacement();

            for (int j = 0; j < occupiedTiles.Count; j++) {
                LocationGridTile tile = occupiedTiles[j];
                tile.SetStructure(structure);
            }
            chosenBuildingSpot.SetIsOpen(false);
            chosenBuildingSpot.SetIsOccupied(true);
            chosenBuildingSpot.SetAllAdjacentSpotsAsOpen(this);
            chosenBuildingSpot.UpdateAdjacentSpotsOccupancy(this);

            structure.SetStructureObject(structureObject);
            structureObject.OnStructureObjectPlaced(this, structure);
        }
        #endregion
    }
}
