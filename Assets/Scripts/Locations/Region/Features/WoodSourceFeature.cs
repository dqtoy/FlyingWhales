using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class WoodSourceFeature : TileFeature {
    private const int MaxBigTrees = 4;
    private const int MaxSmallTrees = 8;
    
    public WoodSourceFeature() {
        name = "Wood Source";
        description = "Provides wood.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }  
    
    #region Overrides
    public override void PerformInitialActions(HexTile tile) {
        List<TileObject> bigTrees = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
        if (bigTrees.Count < MaxBigTrees) {
            int missingTrees = MaxBigTrees - bigTrees.Count;
            LocationStructure wilderness = tile.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i <= missingTrees; i++) {
                List<LocationGridTile> choices = tile.locationGridTiles
                    .Where(x => x.isOccupied == false 
                                && x.structure == wilderness)
                    .ToList();
                if (choices.Count > 0) {
                    LocationGridTile chosenTile = Utilities.GetRandomElement(choices);
                    wilderness.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT),
                        chosenTile);
                } else {
                    //no more tiles to place ore
                    break;
                }
            }
        }
        
        List<TileObject> smallTrees = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.TREE_OBJECT);
        if (smallTrees.Count < MaxSmallTrees) {
            int missingTrees = MaxSmallTrees - smallTrees.Count;
            LocationStructure wilderness = tile.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i <= missingTrees; i++) {
                List<LocationGridTile> choices = tile.locationGridTiles
                    .Where(x => x.isOccupied == false 
                                && x.structure == wilderness)
                    .ToList();
                if (choices.Count > 0) {
                    LocationGridTile chosenTile = Utilities.GetRandomElement(choices);
                    wilderness.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TREE_OBJECT),
                        chosenTile);
                } else {
                    //no more tiles to place ore
                    break;
                }
            }
        }
    }
    #endregion
}