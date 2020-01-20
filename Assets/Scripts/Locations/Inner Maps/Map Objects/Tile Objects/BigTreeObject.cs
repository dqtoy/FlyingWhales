using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class BigTreeObject : TreeObject {
	
	public BigTreeObject() {
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
		SetYield(1000);
	}
	public BigTreeObject(SaveDataTileObject data) {
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(data);
	}

	public override string ToString() {
		return "Big Tree " + id.ToString();
	}

	public static bool CanBePlacedOnTile(LocationGridTile tile) {
		List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(new Point(2, 2), tile);
		int invalidOverlap = overlappedTiles.Count(t => t.hasDetail || t.objHere != null|| t.buildSpotOwner.isPartOfParentRegionMap == false);

		return invalidOverlap <= 0;
	}
}

