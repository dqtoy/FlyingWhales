using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class RegionInnerTileMap : InnerTileMap {

        private const int MapWidth = 15;
        private const int MapHeight = 15;
        
        public override bool isSettlementMap => false;
        
        public IEnumerator Initialize(Region region) {
            this.name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTilemaps();
            yield return StartCoroutine(GenerateGrid(MapWidth, MapHeight, region));
            AssignStructures();
            LocationStructure structure = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            RegionTileObject rto = InnerMapManager.Instance.CreateNewTileObject<RegionTileObject>(TILE_OBJECT_TYPE.REGION_TILE_OBJECT); 
            structure.AddPOI(rto);
            region.SetRegionTileObject(rto);
            rto.SetName(region);
            rto.UpdateAdvertisements(region);
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
    }
}