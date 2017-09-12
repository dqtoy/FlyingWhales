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
                    .Where(x => x.region == city.region && !x.isOccupied && !unoccupiedTilesInSameRegion.Contains(x)));
            }
            targetHexTileToPurchase = unoccupiedTilesInSameRegion[Random.Range(0, unoccupiedTilesInSameRegion.Count)];
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
	private void HasRebellion(){
		if(this.city.rebellion != null){
			Task.current.Succeed();
		}else{
			Task.current.Fail();
		}
	}

    [Task]
    private bool HasReachedLevelCap() {
        return city.ownedTiles.Count >= city.region.cityLevelCap;
    }
    #endregion
}
