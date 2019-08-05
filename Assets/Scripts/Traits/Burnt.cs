using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnt : Trait {

    public Burnt() {
        name = "Burnt";
        description = "This is burnt.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if (addedTo is LocationGridTile) {
            LocationGridTile tile = addedTo as LocationGridTile;
            tile.parentTileMap.SetColor(tile.localPlace, Color.black);
        } else if (addedTo is TileObject) {
            TileObject obj = addedTo as TileObject;
            obj.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(obj.gridTileLocation.localPlace, Color.black);
        } else if (addedTo is SpecialToken) {
            SpecialToken token = addedTo as SpecialToken;
            token.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(token.gridTileLocation.localPlace, Color.black);
        }
    }
}
