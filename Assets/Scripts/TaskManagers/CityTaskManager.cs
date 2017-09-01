using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using System.Linq;

public class CityTaskManager : MonoBehaviour {

	public HexTile targetHexTileToPurchase = null;
	public List<HexTile> pathToTargetHexTile = new List<HexTile>();

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
            //Check for tiles 3 tiles away
			List<HexTile> elligibleTiles = new List<HexTile> ();
			for (int i = 0; i < this.city.ownedTiles.Count; i++) {
                elligibleTiles = elligibleTiles
                    .Union(this.city.ownedTiles[i].GetTilesInRange(3).Where (x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isHabitable 
                    && !x.isBorderOfCities.Except(this.city.kingdom.cities).Any())).ToList();
			}
			//elligibleTiles.Distinct ();

			List<HexTile> purchasableTilesWithSpecialResource = new List<HexTile> ();
			for (int i = 0; i < elligibleTiles.Count; i++) {
				HexTile currentHexTile = elligibleTiles [i];
				if (currentHexTile.specialResource != RESOURCE.NONE) {
					purchasableTilesWithSpecialResource.Add (currentHexTile);
				}
			}

			if (purchasableTilesWithSpecialResource.Count > 0) {
				purchasableTilesWithSpecialResource = Utilities.Shuffle (purchasableTilesWithSpecialResource);
				List<HexTile> path = new List<HexTile> ();
				for (int i = 0; i < purchasableTilesWithSpecialResource.Count; i++) {
					HexTile possibleTargetHextile = purchasableTilesWithSpecialResource[i];
					path = PathGenerator.Instance.GetPath (this.city.hexTile, possibleTargetHextile, PATHFINDING_MODE.RESOURCE_PRODUCTION);
					if (path != null) {
						this.targetHexTileToPurchase = possibleTargetHextile;
						this.pathToTargetHexTile = path;
						break;
					}
				}
			} else {
                //Check for tiles 5 tiles away
                elligibleTiles.Clear ();
				for (int i = 0; i < this.city.ownedTiles.Count; i++) {
                    elligibleTiles = elligibleTiles
                    .Union(this.city.ownedTiles[i].GetTilesInRange(5).Where(x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isHabitable
                         && !x.isBorderOfCities.Except(this.city.kingdom.cities).Any())).ToList();
                }
				//elligibleTiles.Distinct ();

				purchasableTilesWithSpecialResource.Clear ();
				for (int i = 0; i < elligibleTiles.Count; i++) {
					HexTile currentHexTile = elligibleTiles [i];
					if (currentHexTile.specialResource != RESOURCE.NONE) {
						purchasableTilesWithSpecialResource.Add (currentHexTile);
					}
				}
				if (purchasableTilesWithSpecialResource.Count > 0) {
					purchasableTilesWithSpecialResource = Utilities.Shuffle (purchasableTilesWithSpecialResource);
					List<HexTile> path = new List<HexTile> ();
					for (int i = 0; i < purchasableTilesWithSpecialResource.Count; i++) {
						HexTile possibleTargetHextile = purchasableTilesWithSpecialResource[i];
						path = PathGenerator.Instance.GetPath (this.city.hexTile, possibleTargetHextile, PATHFINDING_MODE.RESOURCE_PRODUCTION);
						if (path != null) {
							this.targetHexTileToPurchase = possibleTargetHextile;
							this.pathToTargetHexTile = path;
							break;
						}
					}
				} else {
                    //Just buy a tile without special resource
                    elligibleTiles.Clear ();
					for (int i = 0; i < this.city.ownedTiles.Count; i++) {
						elligibleTiles = elligibleTiles.Union (this.city.ownedTiles [i].elligibleNeighbourTilesForPurchase
                            .Where(x => !x.isBorderOfCities.Except(this.city.kingdom.cities).Any())).ToList();
					}
					if (elligibleTiles.Count > 0) {
						elligibleTiles = Utilities.Shuffle (elligibleTiles);
						List<HexTile> path = new List<HexTile> ();
						for (int i = 0; i < elligibleTiles.Count; i++) {
							HexTile possibleTargetHextile = elligibleTiles[i];
							path = PathGenerator.Instance.GetPath (this.city.hexTile, possibleTargetHextile, PATHFINDING_MODE.RESOURCE_PRODUCTION);
							if (path != null) {
								this.targetHexTileToPurchase = possibleTargetHextile;
								this.pathToTargetHexTile = path;
								break;
							}
						}
					}
				}
			}

			if (this.targetHexTileToPurchase != null && this.pathToTargetHexTile != null) {
				Task.current.Succeed();
			} else {
				Task.current.Fail();
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
			this.pathToTargetHexTile.Clear();
			Task.current.Fail();
			return;
		}

		HexTile tileToBuy = GetNextTileToPurchase();

        if (tileToBuy == null) {
            this.targetHexTileToPurchase = null;
            this.pathToTargetHexTile.Clear();
            Task.current.Fail();
            return;
        } else {
            city.PurchaseTile(tileToBuy);
            if (tileToBuy.tileName == this.targetHexTileToPurchase.tileName) {
                this.targetHexTileToPurchase = null;
                this.pathToTargetHexTile.Clear();
            }
            Task.current.Succeed();
        }
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
        return city.ownedTiles.Count >= city.hexTile.cityLevelCap;
    }
    #endregion

    /*
     * This will determine the next tile to purchase taking into account
     * if the path to the target tile already has occupied tiles.
     * */
    public HexTile GetNextTileToPurchase() {
        HexTile tileToBuy = null;
        if (this.pathToTargetHexTile != null && this.pathToTargetHexTile.Count > 0) {
            //There is a path to the target hex tile, check if the path contains 
            //already occupied tiles
            for (int i = 0; i < this.pathToTargetHexTile.Count; i++) {
                HexTile currentHexTile = this.pathToTargetHexTile[i];
                if (!currentHexTile.isOccupied) {
                    tileToBuy = currentHexTile;
                    break;
                }
            }
        } else {
            tileToBuy = this.targetHexTileToPurchase;
        }
        return tileToBuy;
    }
}
