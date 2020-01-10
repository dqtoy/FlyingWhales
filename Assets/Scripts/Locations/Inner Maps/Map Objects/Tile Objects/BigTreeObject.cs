using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class BigTreeObject : TileObject {
	public int yield { get; private set; }
	
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

	//public int GetSupplyPerMine() {
	//    if (yield < Supply_Per_Mine) {
	//        return yield;
	//    }
	//    return Supply_Per_Mine;
	//}
	public void AdjustYield(int amount) {
		yield += amount;
		yield = Mathf.Max(0, yield);
		if (yield == 0) {
			LocationGridTile loc = gridTileLocation;
			structureLocation.RemovePOI(this);
			SetGridTileLocation(loc); //so that it can still be targetted by aware characters.
		}
	}
	private void SetYield(int amount) {
		yield = amount;
	}
}

