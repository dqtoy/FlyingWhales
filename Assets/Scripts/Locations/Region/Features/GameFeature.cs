using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class GameFeature : TileFeature {

    private const int MaxAnimals = 6;
    
    public GameFeature() {
        name = "Game";
        description = "Hunters can obtain food here.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }
    
    #region Overrides
    public override void PerformInitialActions(HexTile tile) {
        List<TileObject> animals = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.SMALL_ANIMAL);
        if (animals.Count <= MaxAnimals) {
            int missing = MaxAnimals - animals.Count;
            LocationStructure wilderness = tile.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i < missing; i++) {
                List<LocationGridTile> choices = tile.locationGridTiles
                    .Where(x => x.isOccupied == false 
                                && x.structure == wilderness)
                    .ToList();
                if (choices.Count > 0) {
                    LocationGridTile chosenTile = Utilities.GetRandomElement(choices);
                    wilderness.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.SMALL_ANIMAL),
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
