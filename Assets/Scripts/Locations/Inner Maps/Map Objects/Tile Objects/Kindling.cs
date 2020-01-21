using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kindling : TileObject{
	public Kindling() {
		advertisedActions = new List<INTERACTION_TYPE>();
		Initialize(TILE_OBJECT_TYPE.KINDLING);
		RemoveCommonAdvertisements();
	}
	public Kindling(SaveDataTileObject data) {
		advertisedActions = new List<INTERACTION_TYPE>();
		Initialize(data);
	}
}
