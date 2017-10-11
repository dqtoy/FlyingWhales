using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rebellions : GameEvent {
	internal RebelFort rebelFort;
	internal Rebel rebelLeader;

	internal Kingdom targetKingdom;
	internal List<City> conqueredCities;

	private int _dividedPower;
	private int _turns;

	public int dividedPower{
		get{ return this._dividedPower; }
	}
	public Rebellions(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen provokerKing) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REBELLION;
		this.name = "Rebellion";
		this.rebelLeader = (Rebel)startedBy.assignedRole;
		this.targetKingdom = startedBy.city.kingdom;
		this._dividedPower = 0;
		this._turns = 6;
		this.conqueredCities = new List<City> ();
		this.targetKingdom.rebellions.Add (this);
		startedBy.SetImmortality (true);
		//startedBy.ChangeCharacterValues (provokerKing.dictCharacterValues);
		City cityWhereRebelFortIsCreated = startedBy.city;
		CreateRebelFort ();
		Debug.Log (startedBy.name + " has started a rebellion in " + this.targetKingdom.name);

		//Decrease Presitge of target Kingdom
		targetKingdom.AdjustPrestige(-100);
	}
	#region Overrides
	internal override void DoneEvent (){
		base.DoneEvent ();
	}
	#endregion
	private void CreateRebelFort(){
		HexTile hexTile = GetRandomBorderTileForFort ();
		this.rebelFort = (RebelFort)CityGenerator.Instance.CreateNewCity (hexTile, this.targetKingdom, this);
		this.rebelLeader.citizen.city = this.rebelFort;
		int power = GetRebelFortPower ();
		this.rebelFort.AdjustWeapons (power);
		this._dividedPower = Mathf.CeilToInt (power / this._turns);

		GameDate gameDate = new GameDate (GameManager.Instance.month, 5, GameManager.Instance.year);
		gameDate.AddMonths (1);
		SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => AttackNonRebelCity());
	}
	private void AttackNonRebelCity(){
		if(this._turns > 0 && this.isActive){
			List<City> allCities = this.targetKingdom.nonRebellingCities;
			if(allCities.Count > 0){
				City targetCity = allCities [UnityEngine.Random.Range (0, allCities.Count)];
				List<HexTile> path = PathGenerator.Instance.GetPath (this.rebelFort.hexTile, targetCity.hexTile, PATHFINDING_MODE.AVATAR);
				if(path != null){
					this.rebelFort.AttackCity (targetCity, path, this, true);
					this._turns -= 1;
					GameDate gameDate = new GameDate (GameManager.Instance.month, 5, GameManager.Instance.year);
					gameDate.AddMonths (1);
					SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => AttackNonRebelCity());
				}
			}else{
				CheckForSplit ();
			}
		}
	}
	internal void KillFort(){
//		this.rebelFort.KillCity();
//		this.conqueredCities.RemoveAt(0);
//		this.rebelLeader.citizen.city = this.conqueredCities [0];
		this.rebelFort.RemoveListeners();
		this.rebelFort.hexTile.Unoccupy(true);
		this.rebelFort.isDead = true;
		this.rebelFort.hexTile.city = null;
	}
	private HexTile GetRandomBorderTileForFort(){
		List<HexTile> adjacentBorderTiles = new List<HexTile>(this.rebelLeader.citizen.city.hexTile.AllNeighbours);
		List<HexTile> filteredBorderTiles = new List<HexTile>(this.rebelLeader.citizen.city.borderTiles);
		for (int i = 0; i < adjacentBorderTiles.Count; i++) {
			filteredBorderTiles.Remove (adjacentBorderTiles [i]);
		}
		return filteredBorderTiles [UnityEngine.Random.Range (0, filteredBorderTiles.Count)];
	}
	private int GetRebelFortPower(){
		return (int)((this.targetKingdom.baseWeapons + this.targetKingdom.baseArmor) / this.targetKingdom.cities.Count);
	}
	internal void CheckTurns(){
		if(this._turns <= 0){
			CheckForSplit ();
		}
	}
	private void CheckForSplit(){
		//if (this.conqueredCities.Count > 0) {
		//	//Victory Rebellion
		//	this.rebelLeader.citizen.SetImmortality (false);
		//	KillFort ();
		//	Kingdom newKingdom = KingdomManager.Instance.SplitKingdom (this.targetKingdom, this.conqueredCities, this.rebelLeader.citizen);
		//	ResetConqueredCitiesToCityFunctionality (newKingdom);
		//}else{
		//	this.rebelLeader.citizen.SetImmortality (false);
		//	this.rebelLeader.citizen.Death (DEATH_REASONS.REBELLION);
		//	KillFort ();
		//}
		DoneEvent ();
	}
	private void ResetConqueredCitiesToCityFunctionality(Kingdom newKingdom){
		for (int i = 0; i < newKingdom.cities.Count; i++) {
			newKingdom.cities[i].ChangeToCity ();
		}
	}
}
