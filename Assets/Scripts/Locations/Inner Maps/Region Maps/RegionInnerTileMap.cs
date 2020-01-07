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

        private BuildingSpot[,] _buildingSpots;
        private void CreateBuildSpots() {
            for (int x = 0; x <= region.hexTileMap.GetUpperBound(0); x++) {
                for (int y = 0; y <= region.hexTileMap.GetUpperBound(1); y++) {
                    HexTile tile = region.hexTileMap[x, y];
                    if (tile != null) {
                        int originX = (x + 1) * InnerMapManager.BuildingSpotSize.x;
                        int originY = (y + 1) * InnerMapManager.BuildingSpotSize.y;
                        
                        //create the 4 build spots per hex tile given the origin
                        //bottom left corner = Vector2(originX - BuildSpotSize.x / 2, originY - BuildSpotSize.y / 2)
                        //bottom right corner = Vector2(originX + BuildSpotSize.x / 2, originY - BuildSpotSize.y / 2)
                        //top left corner = Vector2(originX - BuildSpotSize.x / 2, originY + BuildSpotSize.y / 2)
                        //top right corner = Vector2(originX + BuildSpotSize.x / 2, originY + BuildSpotSize.y / 2)
                        int halfOfBuildSpotSizeX = InnerMapManager.BuildingSpotSize.x;
                        int halfOfBuildSpotSizeY = InnerMapManager.BuildingSpotSize.y;
                        
                        Vector2Int bottomLeft = new Vector2Int(originX - halfOfBuildSpotSizeX, originY - halfOfBuildSpotSizeY);
                        // CreateNewBuildSpotAt(map[bottomLeft.x, bottomLeft.y], );
                        
                    }
                }
            }
        }
        private void CreateNewBuildSpotAt(LocationGridTile tileLocation, Vector2Int posInBuildSpotGrid) {
            BuildingSpot actualSpot = new BuildingSpot(posInBuildSpotGrid.x, posInBuildSpotGrid.y,  tileLocation.localPlace);
            GameObject buildSpotGo = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
            BuildingSpotItem spotItem = buildSpotGo.GetComponent<BuildingSpotItem>();
            buildSpotGo.transform.localPosition = tileLocation.centeredLocalLocation;
            spotItem.SetBuildingSpot(actualSpot);
        }

        private void BlackOutTilesOutsideOfRegion() {
            for (int i = 0; i < allTiles.Count; i++) {
                LocationGridTile tile = allTiles[i];
                HexTile hexTile = GetHexTileInRegionThatTileBelongsTo(tile);
                if (hexTile == null) {
                    //tile is not part of this region
                    tile.SetDefaultTileColor(Color.black);
                    tile.HighlightTile(Color.black);
                }
            }
        }
    }
}