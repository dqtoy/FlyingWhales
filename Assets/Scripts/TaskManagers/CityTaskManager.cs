using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using System.Linq;

public class CityTaskManager : MonoBehaviour {

	public HexTile targetHexTileToPurchase = null;

	private City city = null;

    internal void Initialize(City city) {
        this.city = city;
    }

	#region Expansion Functions
	[Task]
	public void GetTargetTile(){
		if (this.targetHexTileToPurchase != null) {
			Task.current.Succeed ();
		} else {
            List<HexTile> unoccupiedTilesInSameRegion = new List<HexTile>();
            for (int i = 0; i < city.ownedTiles.Count; i++) {
                HexTile currOwnedTile = city.ownedTiles[i];
                unoccupiedTilesInSameRegion.AddRange(currOwnedTile.AllNeighbours
                    .Where(x => x.elevationType == ELEVATION.PLAIN && x.region == city.region && !x.isOccupied && !unoccupiedTilesInSameRegion.Contains(x)));
            }
            if(unoccupiedTilesInSameRegion.Count > 0) {
                targetHexTileToPurchase = unoccupiedTilesInSameRegion[Random.Range(0, unoccupiedTilesInSameRegion.Count)];
            } else {
                List<HexTile> tilesToChooseFrom = new List<HexTile>(city.region.tilesInRegion);
                //eliminate tiles that are already occupied and are not plains
                for (int i = 0; i < city.region.tilesInRegion.Count; i++) {
                    HexTile currTileInRegion = city.region.tilesInRegion[i];
                    if(currTileInRegion.isOccupied || currTileInRegion.elevationType != ELEVATION.PLAIN) {
                        tilesToChooseFrom.Remove(currTileInRegion);
                    }
                }
                targetHexTileToPurchase = tilesToChooseFrom.OrderBy(x => x.GetDistanceTo(city.hexTile)).FirstOrDefault();
            }
            if(targetHexTileToPurchase == null) {
                Task.current.Fail();
            } else {
                Task.current.Succeed();
            }
        }
    }

	[Task]
	private bool IsDailyGrowthFull(){
		if (this.city.currentGrowth >= this.city.maxGrowth) {
			return true;
		}
		return false;
	}

	[Task]
	private void AddToDailyGrowthUntilFull(){
		this.city.AddToDailyGrowth();
		if (this.city.currentGrowth >= this.city.maxGrowth) {
			Task.current.Succeed ();
		} else {
			Task.current.Fail ();
		}

	}

	[Task]
	public void BuyNextTile(){
		if (this.targetHexTileToPurchase.isOccupied) {
			this.targetHexTileToPurchase = null;
			Task.current.Fail();
			return;
		}

        if (targetHexTileToPurchase == null) {
            Task.current.Fail();
            return;
        }

        city.PurchaseTile(targetHexTileToPurchase);
        this.targetHexTileToPurchase = null;
        Task.current.Succeed();
        
	}

	[Task]
	private void ResetDailyGrowth(){
		this.city.ResetDailyGrowth();
		Task.current.Succeed();
	}

    [Task]
    private bool HasReachedLevelCap() {
        return city.cityLevel >= city.region.cityLevelCap;
    }
    #endregion
}
