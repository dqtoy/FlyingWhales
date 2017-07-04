using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using System.Linq;

public class CityTaskManager : MonoBehaviour {

	public HexTile targetHexTileToPurchase = null;
	public List<HexTile> pathToTargetHexTile = new List<HexTile>();

	private City city = null;
	private General generalToUpgrade = null;
	public ROLE roleToCreate = ROLE.UNTRAINED;

	void Awake(){
		this.city = this.GetComponent<HexTile> ().city;
	}

	#region Expansion Functions
	[Task]
	private void GetTargetTile(){
		if (this.targetHexTileToPurchase != null) {
			Task.current.Succeed ();
		} else {
            //Check for tiles 3 tiles away
			List<HexTile> elligibleTiles = new List<HexTile> ();
			for (int i = 0; i < this.city.ownedTiles.Count; i++) {
                elligibleTiles = elligibleTiles.Union(this.city.ownedTiles [i].GetTilesInRange (3).Where (x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isHabitable)).ToList();
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
                    elligibleTiles = elligibleTiles.Union (this.city.ownedTiles [i].GetTilesInRange (5).Where (x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isHabitable)).ToList();
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
						elligibleTiles.AddRange (this.city.ownedTiles [i].elligibleNeighbourTilesForPurchase);
					}
					elligibleTiles.Distinct ();
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
	private void BuyNextTile(){
		if (this.targetHexTileToPurchase.isOccupied) {
			this.targetHexTileToPurchase = null;
			this.pathToTargetHexTile.Clear();
			Task.current.Fail();
			return;
		}

		HexTile tileToBuy = null;
		if (this.pathToTargetHexTile != null && this.pathToTargetHexTile.Count > 0) {
			//int tileToBuyIndex = 0;
			for (int i = 0; i < this.pathToTargetHexTile.Count; i++) {
                HexTile currentHexTile = this.pathToTargetHexTile[i];
                if (!currentHexTile.isOccupied) {
                    tileToBuy = currentHexTile;
                    break;
                }
                //				HexTile currentHexTile = this.pathToTargetHexTile [i];
                //				if (currentHexTile.isOccupied) {
                //					if (!this.city.ownedTiles.Contains (currentHexTile)) {
                ////						Debug.Log (this.city.name + " of " + this.city.kingdom.name + ": Path to target tile has tile already owned by another city. Choose another target tile.");
                //						break;
                //					}
                //				} else {
                //					if (!currentHexTile.isHabitable) {
                //						tileToBuy = currentHexTile;
                //						tileToBuyIndex = i;
                //						break;
                //					}
                //				}
            }
			if (tileToBuy == null) {
				this.targetHexTileToPurchase = null;
				this.pathToTargetHexTile.Clear ();
				Task.current.Fail ();
				return;
			}
			this.city.PurchaseTile (tileToBuy);
			//this.pathToTargetHexTile.RemoveRange (0, tileToBuyIndex + 1);
		} else {
			tileToBuy = this.targetHexTileToPurchase;
			this.city.PurchaseTile (tileToBuy);
		}

		if (tileToBuy.tileName == this.targetHexTileToPurchase.tileName) {
			this.targetHexTileToPurchase = null;
			this.pathToTargetHexTile.Clear();
		}

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
	#endregion

//	#region Hire Special Citizen Functions
//	[Task]
//	private void GetNextCitizenToHire(){
////		if (this.roleToCreate == ROLE.UNTRAINED) {
////			this.roleToCreate = this.city.GetNonProducingRoleToCreate ();
//////			this.roleToCreate = ROLE.SPY;
////		}
////		if (this.roleToCreate == ROLE.UNTRAINED) {
////			Task.current.Fail();
////		} else {
//			Task.current.Succeed();
////		}
//	}

//	[Task]
//	private void HireCitizen(){
//		GENDER gender = GENDER.MALE;
//		int randomGender = UnityEngine.Random.Range (0, 100);
//		if(randomGender < 20){
//			gender = GENDER.FEMALE;
//		}

//		Citizen newCitizen = new Citizen (this.city, Random.Range (16, 41), gender, 0);
//		newCitizen.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (GameManager.Instance.year - newCitizen.age));
//		newCitizen.AssignRole(this.roleToCreate);
//		this.roleToCreate = ROLE.UNTRAINED;
//		Task.current.Succeed ();

//	}
//	#endregion

	/*#region General Upgrade Functions
	[Task]
	private void GetGeneralToUpgrade(){
		List<Citizen> allGenerals = this.city.citizens.Where (x => x.role == ROLE.GENERAL && !((General)x.assignedRole).inAction).ToList ();
		if (allGenerals.Count > 0) {
			allGenerals.OrderBy (x => ((General)x.assignedRole).GetArmyHP ());
			int baseGeneralHP = 120;
			if (this.city.kingdom.race == RACE.ELVES) {
				baseGeneralHP = 100;
			} else if (this.city.kingdom.race == RACE.CROMADS) {
				baseGeneralHP = 160;
			}

			for (int i = 0; i < allGenerals.Count; i++) {
				General currGeneral = (General)allGenerals[i].assignedRole;
//				if (currGeneral.GetArmyHP () < (this.city.maxGeneralHP + baseGeneralHP)) {
//					this.generalToUpgrade = currGeneral;
//					break;
//				}
			}
			if (this.generalToUpgrade == null) {
				Task.current.Fail ();
			} else {
				Task.current.Succeed ();
			}
		} else {
			Task.current.Fail();
		}
	}

	[Task]
	private void UpgradeGeneral(){
		int amountToUpgrade = 0;
		if (this.city.kingdom.race == RACE.HUMANS || this.city.kingdom.race == RACE.MINGONS) {
			amountToUpgrade = 30;
		} else if (this.city.kingdom.race == RACE.ELVES) {
			amountToUpgrade = 25;
		} else if (this.city.kingdom.race == RACE.CROMADS) {
			amountToUpgrade = 40;
		}

		int baseGeneralHP = 120;
		if (this.city.kingdom.race == RACE.ELVES) {
			baseGeneralHP = 100;
		} else if (this.city.kingdom.race == RACE.CROMADS) {
			baseGeneralHP = 160;
		}

		this.generalToUpgrade.army.hp += amountToUpgrade;
//		if (this.generalToUpgrade.army.hp > (this.city.maxGeneralHP + baseGeneralHP)) {
//			this.generalToUpgrade.army.hp = (this.city.maxGeneralHP + baseGeneralHP);
//		}
		if (this.generalToUpgrade.generalAvatar != null) {
			this.generalToUpgrade.generalAvatar.GetComponent<GeneralObject>().UpdateUI();
		}
		this.city.AdjustResources (GetActionCost ("GENERALUP"));
		this.generalToUpgrade = null;
		Task.current.Succeed ();
	}
	#endregion*/

	//#region Common Functions
	//[Task]
	//private bool HasEnoughResourcesForAction(string action){
 //       return true;
	//	//List<Resource> actionCost = this.GetActionCost(action);
	//	//return this.city.kingdom.HasEnoughResourcesForAction (actionCost);
	//}

	//private List<Resource> GetActionCost(string action){
	//	List<Resource> actionCost = new List<Resource>();
	//	if (action == "EXPANSION") {
	//		actionCost.Add (new Resource (BASE_RESOURCE_TYPE.GOLD, 400));
	//	} else if (action == "RECRUITMENT") {
	//		actionCost = this.city.GetCitizenCreationCostPerType (this.roleToCreate);
	//	} else if (action == "GENERALUP") {
	//		actionCost.Add (new Resource (BASE_RESOURCE_TYPE.GOLD, 200));
	//	}
	//	return actionCost;
	//}
	//#endregion

}
