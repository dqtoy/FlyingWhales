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

        //Building spots
        public Settlement settlement { get; private set; }
        private List<LocationGridTile> outsideTiles { get; set; }
        private List<LocationGridTile> insideTiles { get; set; }
        public TownMapSettings generatedTownMapSettings { get; private set; }
        public string usedTownCenterTemplateName { get; private set; }

        #region Map Generation
        public void Initialize(Settlement settlement) {
            this.settlement = settlement;

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
        public IEnumerator DrawMap(TownMapSettings generatedSettings) {
            // generatedTownMapSettings = generatedSettings;
            // yield return StartCoroutine(GenerateGrid(generatedSettings));
            // SplitMap();
            // Vector3Int startPoint = new Vector3Int(_eastOutsideTiles, _southOutsideTiles, 0);
            // DrawTownMap(generatedSettings, startPoint);
            // CreateBuildingSpots(generatedSettings, startPoint);
            // AssignOuterAreas(insideTiles, outsideTiles);
            yield return null;
        }
        public void LoadMap(SaveDataAreaInnerTileMap data) {
            // outsideTiles = new List<LocationGridTile>();
            // insideTiles = new List<LocationGridTile>();
            //
            // LoadBurningSources(data.burningSources);
            //
            // LoadGrid(data);
            // SplitMap(false);
            //Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
            //DrawTownMap(data.generatedTownMapSettings, startPoint);
            //No need to Place Structures since structure of tile is loaded upon loading grid tile
            //AssignOuterAreas(insideTiles, outsideTiles); //no need for this because structure reference is already saved per location grid tile, and this only assigns the tile to either the wilderness or work settlement structure
        }
        // private IEnumerator GenerateGrid(TownMapSettings settings) {
        //     Point determinedSize = GetWidthAndHeightForSettings(settings);
        //     yield return StartCoroutine(GenerateGrid(determinedSize.X, determinedSize.Y));
        // }
        // private void SplitMap(bool changeFloorAssets = true) {
        //     //assign outer and inner areas
        //     //outer areas should always be 7 tiles from the edge (except for the east side that is 14 tiles from the edge)
        //     //values are all +1 to accomodate walls that take 1 tile
        //     for (int x = 0; x < width; x++) {
        //         for (int y = 0; y < height; y++) {
        //             LocationGridTile currTile = map[x, y];
        //             //determine if tile is outer or inner
        //             if (x < _eastOutsideTiles || x >= width - _westOutsideTiles || y < _southOutsideTiles || y >= height - _northOutsideTiles) {
        //                 //outside
        //                 currTile.SetIsInside(false);
        //                 //GetOutsideFloorTileForArea(settlement)
        //                 if (changeFloorAssets) {
        //                     groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTile(settlement));
        //                 }
        //                 outsideTiles.Add(currTile);
        //             } else {
        //                 //inside
        //                 currTile.SetIsInside(true);
        //                 //GetOutsideFloorTileForArea(settlement)
        //                 if (changeFloorAssets) {
        //                     groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTile(settlement));
        //                 }
        //                 insideTiles.Add(currTile);
        //             }
        //         }
        //     }
        // }
        #endregion

        #region Structures
        public TownMapSettings GenerateTownMap(out string log) {
            log = "Generating Inner Structures for " + settlement.name;
            insideTiles = new List<LocationGridTile>();
            outsideTiles = new List<LocationGridTile>();
            if (settlement.locationType != LOCATION_TYPE.DUNGEON && settlement.locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
                //if this settlement is not a dungeon type
                //first get a town center template that has the needed connections for the structures in the settlement
                List<StructureTemplate> validTownCenters = GetValidTownCenterTemplates(settlement);
                if (validTownCenters.Count == 0) {
                    string error = "There are no valid town center structures for settlement " + settlement.name + ". Needed connectors are: ";
                    foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in settlement.structures) {
                        error += "\n" + keyValuePair.Key.ToString() + " - " + keyValuePair.Value.Count.ToString();
                    }
                    throw new System.Exception(error);
                }
                //Once a town center is chosen
                StructureTemplate chosenTownCenter = validTownCenters[Utilities.rng.Next(0, validTownCenters.Count)];
                usedTownCenterTemplateName = chosenTownCenter.name;
                log += "\nChosen town center template is " + usedTownCenterTemplateName;
                //Place that template in the settlement generation tilemap
                Dictionary<int, Dictionary<int, LocationGridTileSettings>> mainGeneratedSettings = InnerMapManager.Instance.GenerateTownCenterTemplateForGeneration(chosenTownCenter, Vector3Int.zero);
                chosenTownCenter.UpdatePositionsGivenOrigin(Vector3Int.zero);
                Debug.Log(log);
                //once all structures are placed, get the occupied bounds in the settlement generation tilemap, and use that size to generate the actual grid for this map
                return InnerMapManager.Instance.GetTownMapSettings(mainGeneratedSettings);
            }
            return default(TownMapSettings);
        }
        private void AssignOuterAreas(List<LocationGridTile> inTiles, List<LocationGridTile> outTiles) {
            if (settlement.locationType != LOCATION_TYPE.DUNGEON) {
                if (settlement.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
                    for (int i = 0; i < inTiles.Count; i++) {
                        LocationGridTile currTile = inTiles[i];
                        currTile.CreateGenericTileObject();
                        currTile.SetStructure(settlement.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                    }
                    //gate.SetStructure(settlement.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                } else {
                    Debug.LogWarning(settlement.name + " doesn't have a structure for work settlement");
                }
            }

            if (settlement.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
                for (int i = 0; i < outTiles.Count; i++) {
                    LocationGridTile currTile = outTiles[i];
                    if (currTile.IsAtEdgeOfMap() || currTile.tileType == LocationGridTile.Tile_Type.Wall) {
                        continue; //skip
                    }
                    if (!Utilities.IsInRange(currTile.localPlace.x, 0, WestEdge) && 
                        !Utilities.IsInRange(currTile.localPlace.x, width - EastEdge, width)) {
                        currTile.CreateGenericTileObject();
                        currTile.SetStructure(settlement.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                    }

                }
            } else {
                Debug.LogWarning(settlement.name + " doesn't have a structure for wilderness");
            }
        }
        /// <summary>
        /// Generate a dictionary of settings per structure in an settlement. This is used to determine the size of each map
        /// </summary>
        /// <param name="area">The settlement to generate settings for</param>
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
        #endregion

        #region Town Generation
        private List<StructureTemplate> GetValidTownCenterTemplates(Settlement settlement) {
            List<StructureTemplate> valid = new List<StructureTemplate>();
            string extension = "Default";
            List<StructureTemplate> choices = InnerMapManager.Instance.GetStructureTemplates("TOWN CENTER/" + extension);
            for (int i = 0; i < choices.Count; i++) {
                StructureTemplate currTemplate = choices[i];
                if (currTemplate.HasEnoughBuildSpotsForArea(settlement)) {
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
            areaID = innerMap.settlement.id;
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