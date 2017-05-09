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
			List<HexTile> elligibleTiles = new List<HexTile> ();
			for (int i = 0; i < this.city.ownedTiles.Count; i++) {
				elligibleTiles.AddRange(this.city.ownedTiles [i].GetTilesInRange (6.5f).Where (x => x.elevationType != ELEVATION.WATER && !x.isOwned && !x.isHabitable));
			}
			elligibleTiles.Distinct ();

			List<HexTile> purchasableTilesWithSpecialResource = new List<HexTile> ();
			for (int i = 0; i < elligibleTiles.Count; i++) {
				HexTile currentHexTile = elligibleTiles [i];
				if (currentHexTile.specialResource != RESOURCE.NONE) {
					purchasableTilesWithSpecialResource.Add (currentHexTile);
				}
			}

			if (purchasableTilesWithSpecialResource.Count > 0) {
				this.targetHexTileToPurchase = purchasableTilesWithSpecialResource [Random.Range (0, purchasableTilesWithSpecialResource.Count)];
				this.pathToTargetHexTile = PathGenerator.Instance.GetPath (this.city.hexTile, this.targetHexTileToPurchase, PATHFINDING_MODE.RESOURCE_PRODUCTION);
				Task.current.Succeed ();
			} else {
				elligibleTiles.Clear ();
				for (int i = 0; i < this.city.ownedTiles.Count; i++) {
					elligibleTiles.AddRange (this.city.ownedTiles [i].GetTilesInRange (10.5f).Where (x => x.elevationType != ELEVATION.WATER && !x.isOwned && !x.isHabitable));
				}
				elligibleTiles.Distinct ();

				purchasableTilesWithSpecialResource.Clear ();
				for (int i = 0; i < elligibleTiles.Count; i++) {
					HexTile currentHexTile = elligibleTiles [i];
					if (currentHexTile.specialResource != RESOURCE.NONE) {
						purchasableTilesWithSpecialResource.Add (currentHexTile);
					}
				}
				if (purchasableTilesWithSpecialResource.Count > 0) {
					this.targetHexTileToPurchase = purchasableTilesWithSpecialResource [Random.Range (0, purchasableTilesWithSpecialResource.Count)];
					this.pathToTargetHexTile = PathGenerator.Instance.GetPath (this.city.hexTile, this.targetHexTileToPurchase, PATHFINDING_MODE.RESOURCE_PRODUCTION);
					Task.current.Succeed ();
				} else {
					elligibleTiles.Clear ();
					for (int i = 0; i < this.city.ownedTiles.Count; i++) {
						elligibleTiles.AddRange (this.city.ownedTiles [i].elligibleNeighbourTilesForPurchase);
					}
					elligibleTiles.Distinct ();
					if (elligibleTiles.Count > 0) {
						this.targetHexTileToPurchase = elligibleTiles [Random.Range (0, elligibleTiles.Count)];
						this.pathToTargetHexTile = PathGenerator.Instance.GetPath (this.city.hexTile, this.targetHexTileToPurchase, PATHFINDING_MODE.RESOURCE_PRODUCTION);
						Task.current.Succeed ();
					} else {
						Task.current.Fail ();
					}
				}
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
		this.city.currentGrowth += this.city.dailyGrowth;
		if (this.city.currentGrowth >= this.city.maxGrowth) {
			this.city.currentGrowth = this.city.maxGrowth;
			Task.current.Succeed ();
		} else {
			Task.current.Fail ();
		}

	}

	[Task]
	private void BuyNextTile(){
		if (this.targetHexTileToPurchase.isOwned) {
			this.targetHexTileToPurchase = null;
			this.pathToTargetHexTile.Clear();
			Task.current.Fail();
			return;
		}

		HexTile tileToBuy = null;
		if (this.pathToTargetHexTile != null && this.pathToTargetHexTile.Count > 0) {
			int tileToBuyIndex = 0;
			for (int i = 0; i < this.pathToTargetHexTile.Count; i++) {
				HexTile currentHexTile = this.pathToTargetHexTile [i];
				if (!currentHexTile.isOwned && !currentHexTile.isHabitable && !currentHexTile.isOccupied) {
					tileToBuy = currentHexTile;
					tileToBuyIndex = i;
					break;
				}
			}
			if (tileToBuy == null) {
				this.targetHexTileToPurchase = null;
				this.pathToTargetHexTile.Clear();
				Task.current.Fail();
				return;
			}
			this.city.PurchaseTile (tileToBuy);
			this.city.AdjustResources (this.GetActionCost ("EXPANSION"));
			this.pathToTargetHexTile.RemoveRange (0, tileToBuyIndex + 1);
		} else {
			tileToBuy = this.targetHexTileToPurchase;
			this.city.PurchaseTile (tileToBuy);
			this.city.AdjustResources (this.GetActionCost ("EXPANSION"));
		}

		if (tileToBuy.tileName == this.targetHexTileToPurchase.tileName) {
			this.targetHexTileToPurchase = null;
			this.pathToTargetHexTile.Clear();
		}

		Task.current.Succeed();
	}

	[Task]
	private void ResetDailyGrowth(){
		this.city.currentGrowth = 0;
		Task.current.Succeed();
	}
	#endregion

	#region Hire Special Citizen Functions
	[Task]
	private void GetNextCitizenToHire(){
		if (this.roleToCreate == ROLE.UNTRAINED) {
			this.roleToCreate = this.city.GetNonProducingRoleToCreate ();
		}
		if (this.roleToCreate == ROLE.UNTRAINED) {
			Task.current.Fail();
		} else {
			Task.current.Succeed();
		}
	}

	[Task]
	private void HireCitizen(){
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}

		Citizen newCitizen = new Citizen (this.city, Random.Range (16, 41), gender, 0);
		newCitizen.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (GameManager.Instance.year - newCitizen.age));
		newCitizen.AssignRole(this.roleToCreate);
		this.city.UpdateCityConsumption ();
		List<Resource> actionCost = this.GetActionCost("RECRUITMENT");
		for (int i = 0; i < actionCost.Count; i++) {
			if (actionCost [i].resourceType == BASE_RESOURCE_TYPE.GOLD) {
				this.city.AdjustResourceCount (BASE_RESOURCE_TYPE.GOLD, (actionCost [i].resourceQuantity * -1));
				break;
			}
		}
		this.roleToCreate = ROLE.UNTRAINED;
		Task.current.Succeed ();

	}
	#endregion

	#region General Upgrade Functions
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
				if (currGeneral.GetArmyHP () < (this.city.maxGeneralHP + baseGeneralHP)) {
					this.generalToUpgrade = currGeneral;
					break;
				}
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
		if (this.generalToUpgrade.army.hp > (this.city.maxGeneralHP + baseGeneralHP)) {
			this.generalToUpgrade.army.hp = (this.city.maxGeneralHP + baseGeneralHP);
		}
		this.city.AdjustResources (GetActionCost ("GENERALUP"));
		Task.current.Succeed ();
	}
	#endregion

	#region Common Functions
	[Task]
	private bool HasEnoughResourcesForAction(string action){
		List<Resource> actionCost = this.GetActionCost(action);
		return this.city.HasEnoughResourcesForAction (actionCost);
	}

	[Task]
	private List<Resource> GetActionCost(string action){
		List<Resource> actionCost = new List<Resource>();
		if (action == "EXPANSION") {
			actionCost.Add (new Resource (BASE_RESOURCE_TYPE.GOLD, 400));
		} else if (action == "RECRUITMENT") {
			actionCost = this.city.GetCitizenCreationCostPerType (this.roleToCreate);
		} else if (action == "GENERALUP") {
			actionCost.Add (new Resource (BASE_RESOURCE_TYPE.GOLD, 200));
		}
		return actionCost;
	}
	#endregion

}
