using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class AreaInnerTileMap : InnerTileMap {
        private static int _eastOutsideTiles = 15;
        private static int _westOutsideTiles = 8;
        private static int _northOutsideTiles = 8;
        private static int _southOutsideTiles = 8;

        public Grid grid;
        [FormerlySerializedAs("worldUICanvas")] public Canvas worldUiCanvas;
        
        [Header("Other")]
        public Vector4 cameraBounds;
        [FormerlySerializedAs("centerGOPrefab")] public GameObject centerGoPrefab;

        [Header("Structures")]
        [SerializeField] private GameObject buildSpotPrefab;
        [Header("Perlin Noise")]
        [SerializeField] private float offsetX;
        [SerializeField] private float offsetY;
        
        [Header("For Testing")]
        [SerializeField] private LineRenderer pathLineRenderer;

        //burning
        public List<BurningSource> activeBurningSources { get; private set; }
        //Building spots
        public BuildingSpot[,] buildingSpots { get; private set; }
        public Area area { get; private set; }
        private List<LocationGridTile> outsideTiles { get; set; }
        private List<LocationGridTile> insideTiles { get; set; }
        public TownMapSettings generatedTownMapSettings { get; private set; }
        public string usedTownCenterTemplateName { get; private set; }
        public GameObject centerGo { get; private set; }
        public bool isShowing => InnerMapManager.Instance.currentlyShowingMap == this;
        public override bool isSettlementMap => true;

        #region Map Generation
        public void Initialize(Area _area) {
            this.area = _area;
            _area.SetAreaMap(this);
            activeBurningSources = new List<BurningSource>();

            //set tile map sorting orders
            TilemapRenderer ground = groundTilemap.gameObject.GetComponent<TilemapRenderer>();
            ground.sortingOrder = InnerMapManager.GroundTilemapSortingOrder;
            TilemapRenderer details = detailsTilemap.gameObject.GetComponent<TilemapRenderer>();
            details.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder;

            TilemapRenderer northEdge = northEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            northEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            TilemapRenderer southEdge = southEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            southEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            TilemapRenderer westEdge = westEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            westEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
            TilemapRenderer eastEdge = eastEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            eastEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
        }
        public void DrawMap(TownMapSettings generatedSettings) {
            generatedTownMapSettings = generatedSettings;
            GenerateGrid(generatedSettings);
            SplitMap();
            Vector3Int startPoint = new Vector3Int(_eastOutsideTiles, _southOutsideTiles, 0);
            DrawTownMap(generatedSettings, startPoint);
            CreateBuildingSpots(generatedSettings, startPoint);
            AssignOuterAreas(insideTiles, outsideTiles);
        }
        public void LoadMap(SaveDataAreaInnerTileMap data) {
            outsideTiles = new List<LocationGridTile>();
            insideTiles = new List<LocationGridTile>();

            LoadBurningSources(data.burningSources);

            LoadGrid(data);
            SplitMap(false);
            //Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
            //DrawTownMap(data.generatedTownMapSettings, startPoint);
            //No need to Place Structures since structure of tile is loaded upon loading grid tile
            //AssignOuterAreas(insideTiles, outsideTiles); //no need for this because structure reference is already saved per location grid tile, and this only assigns the tile to either the wilderness or work area structure
        }
        public void OnMapGenerationFinished() {
            this.name = area.name + "'s Inner Map";
            worldUiCanvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
            cameraBounds = new Vector4 {x = -185.8f}; //x - minX, y - minY, z - maxX, w - maxY 
            var orthographicSize = AreaMapCameraMove.Instance.areaMapsCamera.orthographicSize;
            cameraBounds.y = orthographicSize;
            cameraBounds.z = (cameraBounds.x + width) - 28.5f;
            cameraBounds.w = height - orthographicSize;
            SpawnCenterGo();
        }
        private void SpawnCenterGo() {
            centerGo = GameObject.Instantiate(centerGoPrefab, transform);
            centerGo.transform.position = new Vector3((cameraBounds.x + cameraBounds.z) * 0.5f, (cameraBounds.y + cameraBounds.w) * 0.5f);
        }
        private void GenerateGrid(TownMapSettings settings) {
            Point determinedSize = GetWidthAndHeightForSettings(settings);
            GenerateGrid(determinedSize.X, determinedSize.Y, area);
        }
        private void SplitMap(bool changeFloorAssets = true) {
            //assign outer and inner areas
            //outer areas should always be 7 tiles from the edge (except for the east side that is 14 tiles from the edge)
            //values are all +1 to accomodate walls that take 1 tile
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    LocationGridTile currTile = map[x, y];
                    //determine if tile is outer or inner
                    if (x < _eastOutsideTiles || x >= width - _westOutsideTiles || y < _southOutsideTiles || y >= height - _northOutsideTiles) {
                        //outside
                        currTile.SetIsInside(false);
                        //GetOutsideFloorTileForArea(area)
                        if (changeFloorAssets) {
                            groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTile(area));
                        }
                        outsideTiles.Add(currTile);
                    } else {
                        //inside
                        currTile.SetIsInside(true);
                        //GetOutsideFloorTileForArea(area)
                        if (changeFloorAssets) {
                            groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTile(area));
                        }
                        insideTiles.Add(currTile);
                    }
                }
            }
        }
        #endregion

        #region Structures
        public TownMapSettings GenerateTownMap(out string log) {
            log = "Generating Inner Structures for " + area.name;
            insideTiles = new List<LocationGridTile>();
            outsideTiles = new List<LocationGridTile>();
            if (area.locationType != LOCATION_TYPE.DUNGEON && area.locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
                //if this area is not a dungeon type
                //first get a town center template that has the needed connections for the structures in the area
                List<StructureTemplate> validTownCenters = GetValidTownCenterTemplates(area);
                if (validTownCenters.Count == 0) {
                    string error = "There are no valid town center structures for area " + area.name + ". Needed connectors are: ";
                    foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in area.structures) {
                        error += "\n" + keyValuePair.Key.ToString() + " - " + keyValuePair.Value.Count.ToString();
                    }
                    throw new System.Exception(error);
                }
                //Once a town center is chosen
                StructureTemplate chosenTownCenter = validTownCenters[Utilities.rng.Next(0, validTownCenters.Count)];
                usedTownCenterTemplateName = chosenTownCenter.name;
                log += "\nChosen town center template is " + usedTownCenterTemplateName;
                //Place that template in the area generation tilemap
                Dictionary<int, Dictionary<int, LocationGridTileSettings>> mainGeneratedSettings = InnerMapManager.Instance.GenerateTownCenterTemplateForGeneration(chosenTownCenter, Vector3Int.zero);
                chosenTownCenter.UpdatePositionsGivenOrigin(Vector3Int.zero);
                Debug.Log(log);
                //once all structures are placed, get the occupied bounds in the area generation tilemap, and use that size to generate the actual grid for this map
                return InnerMapManager.Instance.GetTownMapSettings(mainGeneratedSettings);
            }
            return default(TownMapSettings);
        }
        private void AssignOuterAreas(List<LocationGridTile> inTiles, List<LocationGridTile> outTiles) {
            if (area.locationType != LOCATION_TYPE.DUNGEON) {
                if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
                    for (int i = 0; i < inTiles.Count; i++) {
                        LocationGridTile currTile = inTiles[i];
                        currTile.CreateGenericTileObject();
                        currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                    }
                    //gate.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                } else {
                    Debug.LogWarning(area.name + " doesn't have a structure for work area");
                }
            }

            if (area.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
                for (int i = 0; i < outTiles.Count; i++) {
                    LocationGridTile currTile = outTiles[i];
                    if (currTile.IsAtEdgeOfMap() || currTile.tileType == LocationGridTile.Tile_Type.Wall) {
                        continue; //skip
                    }
                    if (!Utilities.IsInRange(currTile.localPlace.x, 0, WestEdge) && 
                        !Utilities.IsInRange(currTile.localPlace.x, width - EastEdge, width)) {
                        currTile.CreateGenericTileObject();
                        currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                    }

                }
            } else {
                Debug.LogWarning(area.name + " doesn't have a structure for wilderness");
            }
        }
        /// <summary>
        /// Generate a dictionary of settings per structure in an area. This is used to determine the size of each map
        /// </summary>
        /// <param name="area">The area to generate settings for</param>
        /// <returns>Dictionary of Point settings per structure</returns>
        private Point GetWidthAndHeightForSettings(TownMapSettings settings) {
            //height is always 32, 18 is reserved for the inside structures (NOT including walls), and the remaining 14 is for the outside part (Top and bottom)
            Point size = new Point();

            int xSize = settings.size.X;
            int ySize = settings.size.Y;

            xSize += _eastOutsideTiles + _westOutsideTiles;
            ySize += _northOutsideTiles + _southOutsideTiles;

            size.X = xSize;
            size.Y = ySize;

            return size;
        }
        private void DrawTiles(Tilemap tilemap, TileTemplateData[] data, Vector3Int startPos) {
            for (int i = 0; i < data.Length; i++) {
                TileTemplateData currData = data[i];
                Vector3Int pos = new Vector3Int((int)currData.tilePosition.x, (int)currData.tilePosition.y, 0);
                pos.x += startPos.x;
                pos.y += startPos.y;
                if (!string.IsNullOrEmpty(currData.tileAssetName)) {
                    TileBase assetUsed = InnerMapManager.Instance.GetTileAsset(currData.tileAssetName, true);
                    LocationGridTile tile = map[pos.x, pos.y];
                
                    if (tilemap == detailsTilemap) {
                        tile.hasDetail = true;
                        tile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        tilemap.SetTile(pos, assetUsed);
                    } else if (tilemap == groundTilemap) {
                        tile.SetGroundTilemapVisual(assetUsed);
                        tile.SetPreviousGroundVisual(null);
                    } else {
                        tilemap.SetTile(pos, assetUsed);
                    }
                
                    tile.SetLockedState(true);
                }
                tilemap.SetTransformMatrix(pos, currData.matrix);

            }
        }
        public void PlaceInitialStructures(Area area) {
            //order the structures based on their priorities
            Dictionary<STRUCTURE_TYPE, List<LocationStructure>> ordered = area.structures.OrderBy(x => x.Key.StructureGenerationPriority()).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in ordered) {
                if (keyValuePair.Key.ShouldBeGeneratedFromTemplate()) {
                    for (int i = 0; i < keyValuePair.Value.Count; i++) {
                        LocationStructure structure = keyValuePair.Value[i];
                        List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(keyValuePair.Key);
                        GameObject chosenStructurePrefab = Utilities.GetRandomElement(choices);
                        LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
                        BuildingSpot chosenBuildingSpot;
                        if (TryGetValidBuildSpotForStructure(lso, out chosenBuildingSpot) == false) {
                            chosenBuildingSpot = GetRandomBuildingSpotAtCenter(0);
                            if (keyValuePair.Key != STRUCTURE_TYPE.CITY_CENTER) {
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
                }
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

        #region Town Generation
        private List<StructureTemplate> GetValidTownCenterTemplates(Area area) {
            List<StructureTemplate> valid = new List<StructureTemplate>();
            string extension = "Default";
            List<StructureTemplate> choices = InnerMapManager.Instance.GetStructureTemplates("TOWN CENTER/" + extension);
            for (int i = 0; i < choices.Count; i++) {
                StructureTemplate currTemplate = choices[i];
                if (currTemplate.HasEnoughBuildSpotsForArea(area)) {
                    valid.Add(currTemplate);
                }
            }

            return valid;
        }
        /// <summary>
        /// Draw the provided town map, grid must already be generated at this point.
        /// </summary>
        /// <param name="settings">The given settings</param>
        private void DrawTownMap(TownMapSettings settings, Vector3Int startPoint) {
            DrawTiles(groundTilemap, settings.groundTiles, startPoint);
            DrawTiles(structureTilemap, settings.structureTiles, startPoint);
            DrawTiles(detailsTilemap, settings.detailTiles, startPoint);
        }
        private void CreateBuildingSpots(TownMapSettings settings, Vector3Int startPoint) {
            buildingSpots = new BuildingSpot[settings.buildSpots.Max(x => x.buildingSpotGridPos.x + 1), settings.buildSpots.Max(x => x.buildingSpotGridPos.y + 1)];
            for (int i = 0; i < settings.buildSpots.Count; i++) {
                BuildingSpotData currSpotData = settings.buildSpots[i];
                Vector3Int pos = new Vector3Int(currSpotData.location.x, currSpotData.location.y, 0);
                pos.x += startPoint.x;
                pos.y += startPoint.y;
                currSpotData.location = pos;
                BuildingSpot actualSpot = new BuildingSpot(currSpotData);
                GameObject buildSpotGo = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
                BuildingSpotItem spotItem = buildSpotGo.GetComponent<BuildingSpotItem>();
                buildSpotGo.transform.localPosition = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
                spotItem.SetBuildingSpot(actualSpot);
                if (buildingSpots[currSpotData.buildingSpotGridPos.x, currSpotData.buildingSpotGridPos.y] != null) {
                    throw new System.Exception($"Problem with building spot array {currSpotData.buildingSpotGridPos.x},{currSpotData.buildingSpotGridPos.y} already has value");
                }
                buildingSpots[currSpotData.buildingSpotGridPos.x, currSpotData.buildingSpotGridPos.y] = actualSpot;
                actualSpot.Initialize(this);
            }

            for (int x = 0; x <= buildingSpots.GetUpperBound(0); x++) {
                for (int y = 0; y <= buildingSpots.GetUpperBound(1); y++) {
                    buildingSpots[x, y].FindNeighbours(this);
                }
            }
        }

        #endregion

        #region Details
        public void GenerateDetails() {
            //Generate details for the outside map
            MapPerlinDetails(
                outsideTiles.Where(x =>
                    x.objHere == null
                    && (x.structure == null || x.structure.structureType == STRUCTURE_TYPE.WILDERNESS || x.structure.structureType == STRUCTURE_TYPE.WORK_AREA)
                    && x.tileType != LocationGridTile.Tile_Type.Wall
                    && !x.isLocked
                    && !x.IsAdjacentTo(typeof(MagicCircle))
                ).ToList()
            ); //Make this better!

            if (area.locationType != LOCATION_TYPE.DUNGEON) {
                if (area.structures.ContainsKey(STRUCTURE_TYPE.WORK_AREA)) {
                    //only put details on tiles that
                    //  - do not already have details
                    //  - is not a road
                    //  - does not have an object place there (Point of Interest)
                    //  - is not near the gate (so as not to block path going outside)

                    //Generate details for inside map (Trees, shrubs, etc.)
                    MapPerlinDetails(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA).tiles
                        .Where(x => 
                            !x.hasDetail
                            && x.objHere == null 
                            && !x.isLocked).ToList());

                    //Generate details for work area (crates, barrels)
                    WorkAreaDetails(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA).tiles
                        .Where(x => 
                            !x.hasDetail 
                            && x.objHere == null 
                            && !x.isLocked
                            && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Structure_Entrance)).ToList());
                }
            }
            CreateSeamlessEdges();
        }
        private void MapPerlinDetails(List<LocationGridTile> tiles) {
            offsetX = Random.Range(0f, 99999f);
            offsetY = Random.Range(0f, 99999f);
            int minX = tiles.Min(t => t.localPlace.x);
            int maxX = tiles.Max(t => t.localPlace.x);
            int minY = tiles.Min(t => t.localPlace.y);
            int maxY = tiles.Max(t => t.localPlace.y);

            int width = maxX - minX;
            int height = maxY - minY;

            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                float xCoord = (float)currTile.localPlace.x / width * 11f + offsetX;
                float yCoord = (float)currTile.localPlace.y / height * 11f + offsetY;

                float xCoordDetail = (float)currTile.localPlace.x / width * 8f + offsetX;
                float yCoordDetail = (float)currTile.localPlace.y / height * 8f + offsetY;

                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                float sampleDetail = Mathf.PerlinNoise(xCoordDetail, yCoordDetail);
                //ground
                if (area.coreTile.biomeType == BIOMES.SNOW || area.coreTile.biomeType == BIOMES.TUNDRA) {
                    if (sample < 0.5f) {
                        currTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.snowTile);
                    } else if (sample >= 0.5f && sample < 0.8f) {
                        currTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.stoneTile);
                    } else {
                        currTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.snowDirt);
                    }
                } else {
                    if (sample < 0.5f) {
                        currTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.grassTile);
                    } else if (sample >= 0.5f && sample < 0.8f) {
                        currTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.soilTile);
                    } else {
                        currTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.stoneTile);
                    }
               
                }
                currTile.SetPreviousGroundVisual(null);

                //trees and shrubs
                if (!currTile.hasDetail && currTile.HasNeighbouringWalledStructure() == false) {
                    if (sampleDetail < 0.5f) {
                        if (currTile.groundType == LocationGridTile.Ground_Type.Grass || currTile.groundType == LocationGridTile.Ground_Type.Snow) {
                            List<LocationGridTile> overlappedTiles = GetTiles(new Point(2, 2), currTile, tiles);
                            int invalidOverlap = overlappedTiles.Count(t => t.hasDetail || !tiles.Contains(t) || t.objHere != null);
                            if (!currTile.IsAtEdgeOfMap() 
                                && !currTile.HasNeighborAtEdgeOfMap() && invalidOverlap == 0 
                                && overlappedTiles.Count == 4 && Random.Range(0, 100) < 5) {
                                //big tree
                                for (int j = 0; j < overlappedTiles.Count; j++) {
                                    LocationGridTile ovTile = overlappedTiles[j];
                                    ovTile.hasDetail = true;
                                    detailsTilemap.SetTile(ovTile.localPlace, null);
                                    ovTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                                    //ovTile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
                                }
                                detailsTilemap.SetTile(currTile.localPlace, GetBigTreeTile(area));
                                currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                                //currTile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
                            } else {
                                if (Random.Range(0, 100) < 50) {
                                    //shrubs
                                    if (area.coreTile.biomeType != BIOMES.SNOW && area.coreTile.biomeType != BIOMES.TUNDRA) {
                                        currTile.hasDetail = true;
                                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.shrubTile);
                                        if (currTile.structure != null) {
                                            //place tile object
                                            ConvertDetailToTileObject(currTile);
                                        } else {
                                            //place detail instead
                                            currTile.SetTileState(LocationGridTile.Tile_State.Empty);
                                            Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), Vector3.one);
                                            detailsTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                                            detailsTilemap.SetTransformMatrix(currTile.localPlace, m);
                                        }
                                    }
                                } else {
                                    currTile.hasDetail = true;
                                    detailsTilemap.SetTile(currTile.localPlace, GetTreeTile(area));
                                    if (currTile.structure != null) {
                                        ConvertDetailToTileObject(currTile);
                                    } else {
                                        //this is for details on tiles on the border.
                                        //normal tree
                                        currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                                        Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), Vector3.one);
                                        detailsTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                                        detailsTilemap.SetTransformMatrix(currTile.localPlace, m);
                                    }
                                }
                            }
                        }
                    } else {
                        currTile.hasDetail = false;
                        detailsTilemap.SetTile(currTile.localPlace, null);
                    }
                }
            }

            //flower, rock and garbage
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                if (!currTile.hasDetail && currTile.HasNeighbouringWalledStructure() == false) {
                    if (Random.Range(0, 100) < 3) {
                        currTile.hasDetail = true;
                        detailsTilemap.SetTile(currTile.localPlace, GetFlowerTile(area));
                        if (currTile.structure != null) {
                            ConvertDetailToTileObject(currTile);
                        } else {
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        }
                        
                    } else if (Random.Range(0, 100) < 4) {
                        currTile.hasDetail = true;
                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.rockTile);
                        if (currTile.structure != null) {
                            ConvertDetailToTileObject(currTile);
                        } else {
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        }
                    } else if (Random.Range(0, 100) < 3) {
                        currTile.hasDetail = true;
                        detailsTilemap.SetTile(currTile.localPlace, GetGarbTile(area));
                        if (currTile.structure != null) {
                            ConvertDetailToTileObject(currTile);
                        } else {
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        }
                    }
                    // Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), Vector3.one);
                    // detailsTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                    // detailsTilemap.SetTransformMatrix(currTile.localPlace, m);
                }
            }
        }
        /// <summary>
        /// Generate details for the work area (Crates, Barrels, etc.)
        /// </summary>
        /// <param name="insideTiles">Tiles included in the work area</param>
        private void WorkAreaDetails(List<LocationGridTile> insideTiles) {
            //5% of tiles that are adjacent to thin and thick walls should have crates or barrels
            List<LocationGridTile> tilesForBarrels = new List<LocationGridTile>();
            for (int i = 0; i < insideTiles.Count; i++) {
                LocationGridTile currTile = insideTiles[i];
                if (currTile.IsAdjacentToWall()) {
                    tilesForBarrels.Add(currTile);
                }
            }

            for (int i = 0; i < tilesForBarrels.Count; i++) {
                LocationGridTile currTile = tilesForBarrels[i];
                if (Random.Range(0, 100) < 5) {
                    currTile.hasDetail = true;
                    detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.crateBarrelTile);
                    currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                    //place tile object
                    ConvertDetailToTileObject(currTile);
                }
            }

            for (int i = 0; i < insideTiles.Count; i++) {
                LocationGridTile currTile = insideTiles[i];
                if (!currTile.hasDetail && currTile.HasNeighbouringWalledStructure() == false && currTile.structure.structureType.IsOpenSpace() && Random.Range(0, 100) < 3) {
                    //3% of tiles should have random garbage
                    currTile.hasDetail = true;
                    detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.randomGarbTile);
                    //place tile object
                    ConvertDetailToTileObject(currTile);
                }
            }
        }
        private void ConvertDetailToTileObject(LocationGridTile tile) {
            Sprite sprite = detailsTilemap.GetSprite(tile.localPlace);
            TileObject obj = InnerMapManager.Instance.CreateNewTileObject<TileObject>(InnerMapManager.Instance.GetTileObjectTypeFromTileAsset(sprite));
            tile.structure.AddPOI(obj, tile);
            obj.mapVisual.SetVisual(sprite);
            detailsTilemap.SetTile(tile.localPlace, null);
        }
        #endregion

        #region Movement & Mouse Interaction
        public void Update() {
            if (UIManager.Instance.characterInfoUI.isShowing 
                && UIManager.Instance.characterInfoUI.activeCharacter.currentRegion == area.region
                && !UIManager.Instance.characterInfoUI.activeCharacter.isDead
                //&& UIManager.Instance.characterInfoUI.activeCharacter.isWaitingForInteraction <= 0
                && UIManager.Instance.characterInfoUI.activeCharacter.marker != null
                && UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.hasPath
                && (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState == null 
                    || (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.PATROL 
                        && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL
                        && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL_OUTSIDE
                        && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED))) {

                if (UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.currentPath != null
                    && UIManager.Instance.characterInfoUI.activeCharacter.currentParty.icon.isTravelling) {
                    //ShowPath(UIManager.Instance.characterInfoUI.activeCharacter.marker.currentPath);
                    ShowPath(UIManager.Instance.characterInfoUI.activeCharacter);
                    //UIManager.Instance.characterInfoUI.activeCharacter.marker.HighlightHostilesInRange();
                } else {
                    HidePath();
                }
            } else {
                HidePath();
            }
        }
        #endregion

        #region Seamless Edges
        private void CreateSeamlessEdges() {
            for (int i = 0; i < allTiles.Count; i++) {
                LocationGridTile tile = allTiles[i];
                if (tile.structure != null && !tile.structure.structureType.IsOpenSpace()) { continue; } //skip non open space structure tiles.
                tile.CreateSeamlessEdgesForTile(this);
            }
        }
        #endregion

        #region Utilities
        private List<LocationGridTile> GetTiles(Point size, LocationGridTile startingTile, List<LocationGridTile> mustBeIn = null) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
            for (int x = startingTile.localPlace.x; x < startingTile.localPlace.x + size.X; x++) {
                for (int y = startingTile.localPlace.y; y < startingTile.localPlace.y + size.Y; y++) {
                    if (x > map.GetUpperBound(0) || y > map.GetUpperBound(1)) {
                        continue; //skip
                    }
                    if (mustBeIn != null && !mustBeIn.Contains(map[x, y])) {
                        continue; //skip
                    }
                    tiles.Add(map[x, y]);
                }
            }
            return tiles;
        }
        public void CleanUp() {
            Utilities.DestroyChildren(objectsParent);
        }
        #endregion

        #region Other
        public void Open() {
            //this.gameObject.SetActive(true);
            //SwitchFromEstimatedMovementToPathfinding();
        }
        public void Close() {
            //this.gameObject.SetActive(false);
            ////if (UIManager.Instance.areaInfoUI.isShowing) {
            ////    UIManager.Instance.areaInfoUI.ToggleMapMenu(false);
            ////}
            //isHovering = false;
            //SwitchFromPathfindingToEstimatedMovement();
        }
        #endregion

        #region For Testing
        public void ShowPath(List<Vector3> points) {
            pathLineRenderer.gameObject.SetActive(true);
            pathLineRenderer.positionCount = points.Count;
            Vector3[] positions = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++) {
                positions[i] = points[i];
            }
            pathLineRenderer.SetPositions(positions);
        }
        public void ShowPath(Character character) {
            List<Vector3> points = new List<Vector3>(character.marker.pathfindingAI.currentPath.vectorPath);
            int indexAt = 0; //the index that the character is at.
            float nearestDistance = 9999f;
            //refine the current path to remove points that the character has passed.
            //to do that, get the point in the list that the character is nearest to, then remove all other points before that point
            for (int i = 0; i < points.Count; i++) {
                Vector3 currPoint = points[i];
                float distance = Vector3.Distance(character.marker.transform.position, currPoint);
                if (distance < nearestDistance) {
                    indexAt = i;
                    nearestDistance = distance;
                }
            }
            //Debug.Log(character.name + " is at index " + indexAt.ToString() + ". current path length is " + points.Count);
            if (points.Count > 0) {
                for (int i = 0; i <= indexAt; i++) {
                    points.RemoveAt(0);
                }
            }
            //points.Insert(0, character.marker.transform.position);
            //Debug.Log(character.name + " new path length is " + points.Count);
            ShowPath(points);
        }
        public void HidePath() {
            pathLineRenderer.gameObject.SetActive(false);
        }
        #endregion

        #region Building Spots
        private BuildingSpot GetRandomOpenBuildingSpot() {
            List<BuildingSpot> choices = GetOpenBuildingSpots();
            if (choices.Count > 0) {
                return Utilities.GetRandomElement(choices);
            }
            return null;
        }
        private bool TryGetValidBuildSpotForStructure(LocationStructureObject structureObject, out BuildingSpot buildingSpot) {
            if (structureObject.IsBiggerThanBuildSpot()) {
                List<BuildingSpot> openSpots = GetOpenBuildingSpots();
                if (openSpots.Count > 0) {
                    List<BuildingSpot> choices = new List<BuildingSpot>();
                    for (int i = 0; i < openSpots.Count; i++) {
                        BuildingSpot buildSpot = openSpots[i];
                        if (buildSpot.CanPlaceStructureOnSpot(structureObject, this)) {
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
                buildingSpot = GetRandomOpenBuildingSpot();
                return buildingSpot != null;
            }
        }
        private BuildingSpot GetRandomBuildingSpotAtCenter(int allowance) {
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
        private List<BuildingSpot> GetOpenBuildingSpots() {
            List<BuildingSpot> open = new List<BuildingSpot>();
            for (int x = 0; x < buildingSpots.GetUpperBound(0); x++) {
                for (int y = 0; y < buildingSpots.GetUpperBound(1); y++) {
                    BuildingSpot currSpot = buildingSpots[x, y];
                    if (currSpot.isOpen) {
                        open.Add(currSpot);
                    }
                }
            }
            return open;
        }
        private BuildSpotTileObject GetRandomOpenBuildSpotTileObject() {
            List<BuildSpotTileObject> choices = GetOpenBuildpotTileObjects();
            if (choices.Count > 0) {
                return Utilities.GetRandomElement(choices);
            }
            return null;
        }
        public bool TryGetValidBuildSpotTileObjectForStructure(LocationStructureObject structureObject, out BuildSpotTileObject buildingSpot) {
            if (structureObject.IsBiggerThanBuildSpot()) {
                List<BuildSpotTileObject> openSpots = GetOpenBuildpotTileObjects();
                if (openSpots.Count > 0) {
                    List<BuildSpotTileObject> choices = new List<BuildSpotTileObject>();
                    for (int i = 0; i < openSpots.Count; i++) {
                        BuildSpotTileObject buildSpot = openSpots[i];
                        if (buildSpot.spot.CanPlaceStructureOnSpot(structureObject, this)) {
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
        private List<BuildSpotTileObject> GetOpenBuildpotTileObjects() {
            List<BuildSpotTileObject> spots = area.GetTileObjectsOfType(TILE_OBJECT_TYPE.BUILD_SPOT_TILE_OBJECT).Select(x => x as BuildSpotTileObject).ToList();
            List<BuildSpotTileObject> open = new List<BuildSpotTileObject>();
            for (int i = 0; i < spots.Count; i++) {
                BuildSpotTileObject buildSpotTileObject = spots[i];
                if (buildSpotTileObject.spot.isOpen) {
                    open.Add(buildSpotTileObject);
                }
            }
            return open;
        }
        public bool IsBuildSpotValidFor(LocationStructureObject structureObject, BuildingSpot spot) {
            bool isHorizontallyBig = structureObject.IsHorizontallyBig();
            bool isVerticallyBig = structureObject.IsVerticallyBig();
            BuildingSpot currSpot = spot;
            if (isHorizontallyBig && isVerticallyBig) {
                //if it is bigger both horizontally and vertically
                //only get build spots that do not have any occupied adjacent spots at their top and right
                bool hasUnoccupiedNorth = currSpot.neighbours.ContainsKey(GridNeighbourDirection.North) && currSpot.neighbours[GridNeighbourDirection.North].isOccupied == false;
                bool hasUnoccupiedEast = currSpot.neighbours.ContainsKey(GridNeighbourDirection.East) && currSpot.neighbours[GridNeighbourDirection.East].isOccupied == false;
                if (hasUnoccupiedNorth && hasUnoccupiedEast) {
                    return true;
                }
            } else if (isHorizontallyBig) {
                //if it is bigger horizontally
                //only get build spots that do not have any occupied adjacent spots at their right
                bool hasUnoccupiedEast = currSpot.neighbours.ContainsKey(GridNeighbourDirection.East) && currSpot.neighbours[GridNeighbourDirection.East].isOccupied == false;
                if (hasUnoccupiedEast) {
                    return true;
                }
            } else if (isVerticallyBig) {
                //if it is bigger vertically
                //only get build spots that do not have any occupied adjacent spots at their top
                bool hasUnoccupiedNorth = currSpot.neighbours.ContainsKey(GridNeighbourDirection.North) && currSpot.neighbours[GridNeighbourDirection.North].isOccupied == false;
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
    }

    [System.Serializable]
    public class SaveDataAreaInnerTileMap {
        public int width;
        public int height;
        public int areaID;
        public SaveDataLocationGridTile[][] map;
        public string usedTownCenterTemplateName;
        public TownMapSettings generatedTownMapSettings;
        public List<SaveDataBurningSource> burningSources;

        public void Save(AreaInnerTileMap innerMap) {
            width = innerMap.width;
            height = innerMap.height;
            areaID = innerMap.area.id;
            usedTownCenterTemplateName = innerMap.usedTownCenterTemplateName;
            generatedTownMapSettings = innerMap.generatedTownMapSettings;

            burningSources = new List<SaveDataBurningSource>();
            for (int i = 0; i < innerMap.activeBurningSources.Count; i++) {
                BurningSource bs = innerMap.activeBurningSources[i];
                SaveDataBurningSource source = new SaveDataBurningSource();
                source.Save(bs);
                burningSources.Add(source);
            }

            map = new SaveDataLocationGridTile[width][];
            for (int x = 0; x < innerMap.map.GetLength(0); x++) {
                map[x] = new SaveDataLocationGridTile[innerMap.map.GetLength(1)];
                for (int y = 0; y < innerMap.map.GetLength(1); y++) {
                    SaveDataLocationGridTile data = new SaveDataLocationGridTile();
                    data.Save(innerMap.map[x, y]);
                    map[x][y] = data;
                }
            }
        }
        public void Load(AreaInnerTileMap innerMap) {
            innerMap.width = width;
            innerMap.height = height;
            innerMap.Initialize(LandmarkManager.Instance.GetAreaByID(areaID));
            innerMap.LoadMap(this);
        }

        public void LoadTileTraits() {
            for (int x = 0; x < map.Length; x++) {
                for (int y = 0; y < map[x].Length; y++) {
                    map[x][y].LoadTraits();
                }
            }
        }
        public void LoadObjectHereOfTiles() {
            for (int x = 0; x < map.Length; x++) {
                for (int y = 0; y < map[x].Length; y++) {
                    map[x][y].LoadObjectHere();
                }
            }
        }
    }
}