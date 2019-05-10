using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tombstone : TileObject {
    public Character character { get; private set; }
    public LocationStructure location { get; private set; }

    public Tombstone(Character character, LocationStructure structure) {
        this.character = character;
        location = structure;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMEMBER_FALLEN, INTERACTION_TYPE.SPIT };
        Initialize(TILE_OBJECT_TYPE.TOMBSTONE);
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        base.SetGridTileLocation(tile);
        if (tile != null) {
            //when a tombstone of a character has been placed, assume that his/her marker needs to be disabled
            character.DestroyMarker();
            character.SetGrave(this);
        }
    }

    public override void OnClickAction() {
        base.OnClickAction();
        UIManager.Instance.ShowCharacterInfo(character);
    }

    public override string ToString() {
        return "Tombstone of " + character.name;
    }
}