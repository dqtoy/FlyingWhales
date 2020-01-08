using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class RegionInnerTileMap : InnerTileMap {
        public override bool isSettlementMap => false;
        public Region region { get; private set; }

        public IEnumerator GenerateMap(Region region, int mapWidth, int mapHeight) {
            this.name = $"{region.name}'s Inner Map";
            this.region = region;
            region.SetRegionInnerMap(this);
            ClearAllTilemaps();
            yield return StartCoroutine(GenerateGrid(mapWidth, mapHeight));
            AssignStructures();
            CreateBuildSpots();
            BlackOutTilesOutsideOfRegion();


            // LocationStructure structure = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            // RegionTileObject rto = InnerMapManager.Instance.CreateNewTileObject<RegionTileObject>(TILE_OBJECT_TYPE.REGION_TILE_OBJECT); 
            // structure.AddPOI(rto);
            // region.SetRegionTileObject(rto);
            // rto.SetName(region);
            // rto.UpdateAdvertisements(region);
        }
        private void AssignStructures() {
            LocationStructure structure = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS); //since regions only have wilderness
            for (int i = 0; i < allTiles.Count; i++) {
                LocationGridTile tile = allTiles[i];
                if (!Utilities.IsInRange(tile.localPlace.x, 0, WestEdge) &&
                    !Utilities.IsInRange(tile.localPlace.x, width - EastEdge, width) &&
                    !Utilities.IsInRange(tile.localPlace.y, 0, SouthEdge) &&
                    !Utilities.IsInRange(tile.localPlace.y, height - NorthEdge, width)) {
                    tile.CreateGenericTileObject();
                    tile.SetStructure(structure);
                }
            }
        }
        private void CreateBuildSpots() {
            buildingSpots = new BuildingSpot[width / InnerMapManager.BuildingSpotSize.x, height / InnerMapManager.BuildingSpotSize.y];
            for (int x = 0; x <= buildingSpots.GetUpperBound(0); x++) {
                for (int y = 0; y <= buildingSpots.GetUpperBound(1); y++) {
                    buildingSpots[x, y] = null;
                }
            }
            
            //this function basically creates 4 build spots per hextile in the region
            //1 for each corner in a square.
            int halfOfBuildSpotSizeX = InnerMapManager.BuildingSpotSize.x / 2;
            int halfOfBuildSpotSizeY = InnerMapManager.BuildingSpotSize.y / 2;
            for (int x = 0; x <= region.hexTileMap.GetUpperBound(0); x++) {
                for (int y = 0; y <= region.hexTileMap.GetUpperBound(1); y++) {
                    HexTile tile = region.hexTileMap[x, y];
                    int originX = ((x + 1) * (InnerMapManager.BuildingSpotSize.x * 2)) -
                                  InnerMapManager.BuildingSpotSize.x;
                    int originY = ((y + 1) * (InnerMapManager.BuildingSpotSize.y * 2)) -
                                  InnerMapManager.BuildingSpotSize.y;
                    if (tile != null) {
                        //create the 4 build spots per hex tile given the origin
                        //bottom left corner = Vector2(originX - BuildSpotSize.x / 2, originY - BuildSpotSize.y / 2)
                        //bottom right corner = Vector2(originX + BuildSpotSize.x / 2, originY - BuildSpotSize.y / 2)
                        //top left corner = Vector2(originX - BuildSpotSize.x / 2, originY + BuildSpotSize.y / 2)
                        //top right corner = Vector2(originX + BuildSpotSize.x / 2, originY + BuildSpotSize.y / 2
                        Vector2Int bottomLeft = new Vector2Int((originX - halfOfBuildSpotSizeX) - 1, (originY - halfOfBuildSpotSizeY) - 1);
                        Vector2Int bottomRight = new Vector2Int(originX + halfOfBuildSpotSizeX, (originY - halfOfBuildSpotSizeY) - 1);
                        Vector2Int topLeft = new Vector2Int((originX - halfOfBuildSpotSizeX) - 1, originY + halfOfBuildSpotSizeY);
                        Vector2Int topRight = new Vector2Int(originX + halfOfBuildSpotSizeX, originY + halfOfBuildSpotSizeY);
                        
                        Vector2Int bottomLeftBuildSpotGridPos = new Vector2Int(
                            bottomLeft.x / InnerMapManager.BuildingSpotSize.x, bottomLeft.y / InnerMapManager.BuildingSpotSize.y    
                        );
                        Vector2Int bottomRightBuildSpotGridPos = new Vector2Int(
                            bottomRight.x / InnerMapManager.BuildingSpotSize.x, bottomRight.y / InnerMapManager.BuildingSpotSize.y    
                        );
                        Vector2Int topLeftBuildSpotGridPos = new Vector2Int(
                            topLeft.x / InnerMapManager.BuildingSpotSize.x, topLeft.y / InnerMapManager.BuildingSpotSize.y    
                        );
                        Vector2Int topRightBuildSpotGridPos = new Vector2Int(
                            topRight.x / InnerMapManager.BuildingSpotSize.x, topRight.y / InnerMapManager.BuildingSpotSize.y    
                        );
                        
                        BuildingSpot bottomLeftSpot = CreateNewBuildSpotAt(map[bottomLeft.x, bottomLeft.y], bottomLeftBuildSpotGridPos);
                        BuildingSpot bottomRightSpot = CreateNewBuildSpotAt(map[bottomRight.x, bottomRight.y], bottomRightBuildSpotGridPos);
                        BuildingSpot topLeftSpot = CreateNewBuildSpotAt(map[topLeft.x, topLeft.y], topLeftBuildSpotGridPos);
                        BuildingSpot topRightSpot = CreateNewBuildSpotAt(map[topRight.x, topRight.y], topRightBuildSpotGridPos);

                        buildingSpots[bottomLeftBuildSpotGridPos.x, bottomLeftBuildSpotGridPos.y] = bottomLeftSpot;
                        buildingSpots[bottomRightBuildSpotGridPos.x, bottomRightBuildSpotGridPos.y] = bottomRightSpot;
                        buildingSpots[topLeftBuildSpotGridPos.x, topLeftBuildSpotGridPos.y] = topLeftSpot;
                        buildingSpots[topRightBuildSpotGridPos.x, topRightBuildSpotGridPos.y] = topRightSpot;
                    }
                }
            }
            
            for (int x = 0; x <= buildingSpots.GetUpperBound(0); x++) {
                for (int y = 0; y <= buildingSpots.GetUpperBound(1); y++) {
                    BuildingSpot spot = buildingSpots[x, y];
                    spot?.FindNeighbours(this);
                }
            }
        }
        private BuildingSpot CreateNewBuildSpotAt(LocationGridTile tileLocation, Vector2Int locationInBuildSpotGrid) {
            BuildingSpot actualSpot = new BuildingSpot(tileLocation.localPlace, locationInBuildSpotGrid);
            GameObject buildSpotGo = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
            BuildingSpotItem spotItem = buildSpotGo.GetComponent<BuildingSpotItem>();
            buildSpotGo.transform.localPosition = tileLocation.centeredLocalLocation;
            spotItem.SetBuildingSpot(actualSpot);
            return actualSpot;
        }

        private void BlackOutTilesOutsideOfRegion() {
            for (int i = 0; i < allTiles.Count; i++) {
                LocationGridTile tile = allTiles[i];
                HexTile hexTile = GetHexTileInRegionThatTileBelongsTo(tile);
                if (hexTile == null) {
                    //tile is not part of this region
                    Color color = Color.black;
                    color.a = 175f / 255f;
                    tile.SetDefaultTileColor(color);
                    tile.HighlightTile(color);
                }
            }
        }
    }
}