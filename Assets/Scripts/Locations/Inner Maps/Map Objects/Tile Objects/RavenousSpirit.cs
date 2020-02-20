using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RavenousSpirit : TileObject {

    public RavenousSpirit() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.RAVENOUS_SPIRIT);
        traitContainer.AddTrait(this, "Ravenous");
    }
    public RavenousSpirit(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
        traitContainer.AddTrait(this, "Ravenous");
    }

    #region Overrides
    public override string ToString() {
        return $"Ravenous Spirit {id}";
    }
    #endregion
}
