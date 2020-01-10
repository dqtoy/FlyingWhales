using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class MetalSourceFeature : TileFeature {
    private const int MaxOres = 4;
    
    public MetalSourceFeature() {
        name = "Metal Source";
        description = "Provides Metal";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }

    #region Overrides
    public override void PerformInitialActions(HexTile tile) {
        List<TileObject> ores = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.ORE);
        if (ores.Count < MaxOres) {
            int missingOres = MaxOres - ores.Count;
            LocationStructure wilderness = tile.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i <= missingOres; i++) {
                List<LocationGridTile> choices = tile.locationGridTiles
                    .Where(x => x.isOccupied == false 
                                && x.structure == wilderness)
                    .ToList();
                if (choices.Count > 0) {
                    LocationGridTile chosenTile = Utilities.GetRandomElement(choices);
                    wilderness.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ORE),
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
