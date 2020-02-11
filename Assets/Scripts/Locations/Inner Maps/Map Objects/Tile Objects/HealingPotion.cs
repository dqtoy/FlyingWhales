using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : TileObject {

    public HealingPotion() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.HEALING_POTION);
        RemoveCommonAdvertisements();
    }
    public HealingPotion(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
