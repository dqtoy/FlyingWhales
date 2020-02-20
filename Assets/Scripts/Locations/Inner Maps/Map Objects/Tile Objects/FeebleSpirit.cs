using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeebleSpirit : TileObject {

    public FeebleSpirit() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.FEEBLE_SPIRIT);
        traitContainer.AddTrait(this, "Feeble");
    }
    public FeebleSpirit(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
        traitContainer.AddTrait(this, "Feeble");
    }

    #region Overrides
    public override string ToString() {
        return $"Feeble Spirit {id}";
    }
    #endregion
}