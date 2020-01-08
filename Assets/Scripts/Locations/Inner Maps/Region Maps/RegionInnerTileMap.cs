using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class RegionInnerTileMap : InnerTileMap {
        public override bool isSettlementMap => false;
        private Region region { get; set; }
        
        public override void Initialize(ILocation location) {
            base.Initialize(location);
            this.region = location as Region;
        }
        public IEnumerator GenerateMap(int width, int height) {
            this.name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTilemaps();

            CreateBuildSpotGrid();
            
            
            
            yield return StartCoroutine(GenerateGrid(width, height));

            // LocationStructure structure = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            // RegionTileObject rto = InnerMapManager.Instance.CreateNewTileObject<RegionTileObject>(TILE_OBJECT_TYPE.REGION_TILE_OBJECT); 
            // structure.AddPOI(rto);
            // region.SetRegionTileObject(rto);
            // rto.SetName(region);
            // rto.UpdateAdvertisements(region);
        }
        private void AssignWilderness() {
            LocationStructure structure = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
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
        private BuildingSpot CreateNewBuildSpotAt(LocationGridTile tileLocation, Vector2Int locationInBuildSpotGrid) {
            BuildingSpot actualSpot = new BuildingSpot(tileLocation.localPlace, locationInBuildSpotGrid);
            GameObject buildSpotGo = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
            BuildingSpotItem spotItem = buildSpotGo.GetComponent<BuildingSpotItem>();
            buildSpotGo.transform.localPosition = tileLocation.centeredLocalLocation;
            spotItem.SetBuildingSpot(actualSpot);
            actualSpot.Initialize(this);
            return actualSpot;
        }

        #region Build Spots
        private void CreateBuildSpotGrid() {
            int width;
            int leftMostRow = region.GetLocalRowOf(region.GetLeftMostTile());
            int rightMostRow = region.GetLocalRowOf(region.GetRightMostTile());
            
            int maxX = region.tiles.Max(t => t.data.xCoordinate);
            int minX = region.tiles.Min(t => t.data.xCoordinate);

            int difference = (maxX - minX) + 1;

            if ((Utilities.IsEven(leftMostRow) && Utilities.IsEven(rightMostRow)) || 
                (Utilities.IsEven(leftMostRow) == false && Utilities.IsEven(rightMostRow) == false)) {
                width = difference;
            } else {
                width = difference + 1;
            }
            
            int maxY = region.tiles.Max(t => t.data.yCoordinate);
            int minY = region.tiles.Min(t => t.data.yCoordinate);
            int height = maxY - minY + 1;
            
            buildingSpots = new BuildingSpot[width, height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    GameObject buildSpotGo = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
                    // BuildingSpotItem spotItem = buildSpotGo.GetComponent<BuildingSpotItem>();
                    float xPos = (x + 1) * (InnerMapManager.BuildingSpotSize.x);
                    float yPos = (y + 1) * (InnerMapManager.BuildingSpotSize.y);
                    buildSpotGo.transform.localPosition = new Vector2(xPos, yPos);
                }
            }
            
        }
        #endregion
    }
}