using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnt : Trait {

    private Color burntColor {
        get {
            Color color = Color.black;
            color.a = 75f / 255f;
            return color;
        }
    }

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
            tile.parentTileMap.SetColor(tile.localPlace, burntColor);
            tile.SetDefaultTileColor(burntColor);
        } else if (addedTo is TileObject) {
            TileObject obj = addedTo as TileObject;
            obj.SetPOIState(POI_STATE.INACTIVE);
            obj.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(obj.gridTileLocation.localPlace, burntColor);
        } else if (addedTo is SpecialToken) {
            SpecialToken token = addedTo as SpecialToken;
            token.SetPOIState(POI_STATE.INACTIVE);
            token.gridTileLocation.parentAreaMap.objectsTilemap.SetColor(token.gridTileLocation.localPlace, burntColor);
        }
    }
}
