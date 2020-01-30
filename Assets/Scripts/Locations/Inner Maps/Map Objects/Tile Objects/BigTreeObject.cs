using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class BigTreeObject : TreeObject {
	
	public BigTreeObject() {
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
		SetYield(300);
	}
	public BigTreeObject(SaveDataTileObject data) {
		advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
		Initialize(data);
	}

	public override string ToString() {
		return "Big Tree " + id.ToString();
	}

	public static bool CanBePlacedOnTile(LocationGridTile tile) {
		if (tile.isOccupied) {
			return false;
		}
		if (tile.structure != null && tile.structure.structureType.IsOpenSpace() == false) {
			return false;
		}
		if (tile.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)) {
			return false;
		}
		List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(new Point(2, 2), tile);
		int invalidOverlap = overlappedTiles.Count(t => t.hasDetail || t.objHere != null|| t.buildSpotOwner.canBeBuiltOnByNPC == false || t.tileType == LocationGridTile.Tile_Type.Wall);

		return invalidOverlap <= 0;
	}
}

