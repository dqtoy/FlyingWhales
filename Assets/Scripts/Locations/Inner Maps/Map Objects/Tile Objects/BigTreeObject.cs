using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class BigTreeObject : TileObject {
	private int yield { get; set; }
	
	public BigTreeObject() {
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
		SetYield(100);
	}
	public BigTreeObject(SaveDataTileObject data) {
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(data);
	}

	public override string ToString() {
		return "Big Tree " + id.ToString();
	}
	
	public void AdjustYield(int amount) {
		yield += amount;
		yield = Mathf.Max(0, yield);
		if (yield == 0) {
			LocationGridTile loc = gridTileLocation;
			structureLocation.RemovePOI(this);
			SetGridTileLocation(loc); //so that it can still be targeted by aware characters.
		}
	}
	private void SetYield(int amount) {
		yield = amount;
	}

	public static bool CanBePlacedOnTile(LocationGridTile tile) {
		List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(new Point(2, 2), tile);
		int invalidOverlap = overlappedTiles.Count(t => t.hasDetail || t.objHere != null|| t.buildSpotOwner.isPartOfParentRegionMap == false);

		return invalidOverlap <= 0;
	}
}

