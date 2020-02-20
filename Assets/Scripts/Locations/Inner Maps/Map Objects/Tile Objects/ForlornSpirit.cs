using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForlornSpirit : TileObject {

    public ForlornSpirit() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.FORLORN_SPIRIT);
        traitContainer.AddTrait(this, "Forlorn");
    }
    public ForlornSpirit(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
        traitContainer.AddTrait(this, "Forlorn");
    }

    #region Overrides
    public override string ToString() {
        return $"Forlorn Spirit {id}";
    }
    #endregion
}