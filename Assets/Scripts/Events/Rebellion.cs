using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Rebellion : GameEvent {
	internal RebelFort rebelFort;
	internal Rebel rebelLeader;

	internal Kingdom targetKingdom;
//	internal City targetCity;
//	internal City sourceCityOrFort;
	internal List<City> conqueredCities;
	internal CityWarPair warPair;
	internal int attackRate;
	private bool isInitialAttack;

	public Rebellion(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.rebelLeader = (Rebel)startedBy.assignedRole;
		this.targetKingdom = startedBy.city.kingdom;
//		this.targetCity = null;
//		this.sourceCityOrFort = null;
		this.conqueredCities = new List<City> ();
		this.attackRate = 0;
		this.isInitialAttack = false;
		this.targetKingdom.rebellions.Add (this);
		this.warPair.DefaultValues ();
		CreateRebelFort ();
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.onUpdatePath.AddListener (UpdatePath);

	}
	#region Overrides
	internal override void PerformAction (){
		if(!this.targetKingdom.isAlive()){
			if(this.conqueredCities.Count > 1){
				//Victory Rebellion
				KillFort();
				Kingdom newKingdom = KingdomManager.Instance.SplitKingdom(this.targetKingdom, this.conqueredCities);
				newKingdom.AssignNewKing (this.rebelLeader.citizen);
			}
			this.DoneEvent();
			return;
		}else{
			if(this.conqueredCities.Count <= 0){
				//Victory Kingdom
				this.DoneEvent();
				return;
			}
		}
		Attack ();
//		if(this.sourceCityOrFort == null || this.targetCity == null){
//			GetSourceAndTargetCity ();
//		}
//		if (this.sourceCityOrFort != null && this.targetCity != null) {
//			this.sourceCityOrFort.AttackCityEvent (this.targetCity);
//		}
	}
	internal override void DoneEvent (){
		base.DoneEvent ();
		this.targetKingdom.rebellions.Remove (this);
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		EventManager.Instance.onUpdatePath.RemoveListener (UpdatePath);

	}

	#endregion
	private void CreateRebelFort(){
		HexTile hexTile = GetRandomBorderTileForFort ();
//		this.rebelFort = new RebelFort (hexTile, this.targetKingdom, this);
		this.rebelFort = (RebelFort)CityGenerator.Instance.CreateNewCity (hexTile, this.targetKingdom, this);
//		this.rebelLeader.citizen.city.citizens.Remove (this.rebelLeader.citizen);
//		this.rebelFort.citizens.Add (this.rebelLeader.citizen);
		this.rebelLeader.citizen.city = this.rebelFort;
//		this.conqueredCities.Add (this.rebelFort);
	}
	private void KillFort(){
		this.rebelFort.KillCity();
		this.conqueredCities.RemoveAt(0);
		this.rebelLeader.citizen.city = this.conqueredCities [0];
	}
	private HexTile GetRandomBorderTileForFort(){
		return this.rebelLeader.citizen.city.borderTiles [UnityEngine.Random.Range (0, this.rebelLeader.citizen.city.borderTiles.Count)];
	}
	private void GetSourceAndTargetCity(){
		int nearestDistance = 0;
		City source = null;
		City target = null;
		for (int i = 0; i < this.conqueredCities.Count; i++) {
			for (int j = 0; j < this.targetKingdom.cities.Count; j++) {
				if(this.targetKingdom.cities[i].rebellion != null){
					continue;
				}
				List<HexTile> path = PathGenerator.Instance.GetPath (this.conqueredCities [i].hexTile, this.targetKingdom.cities [j].hexTile, PATHFINDING_MODE.AVATAR).ToList();
				if(path != null){
					int distance = path.Count;
					if(source == null && target == null){
						source = this.conqueredCities [i];
						target = this.targetKingdom.cities [j];
						nearestDistance = distance;
					}else{
						
						if(distance < nearestDistance){
							source = this.conqueredCities [i];
							target = this.targetKingdom.cities [j];
							nearestDistance = distance;
						}
					}
				}

			}
		}

//		this.sourceCityOrFort = source;
//		this.targetCity = target;
	}
	private City GetNearestCityFrom(HexTile hexTile, ref List<HexTile> path){
		City nearestCity = null;
		int nearestDistance = 0;
		int distance = 0;
//		List<HexTile> newPath = new List<HexTile>();
		for (int i = 0; i < this.targetKingdom.cities.Count; i++) {
			if(this.targetKingdom.cities[i].rebellion != null){
				continue;
			}
			List<HexTile> newPath = PathGenerator.Instance.GetPath (hexTile, this.targetKingdom.cities[i].hexTile, PATHFINDING_MODE.COMBAT).ToList();
			if(newPath != null){
				if(nearestCity == null){
					nearestCity = this.targetKingdom.cities [i];
					nearestDistance = newPath.Sum (x => x.movementDays);
					path = newPath;
				}else{
					distance = newPath.Sum (x => x.movementDays);
					if(distance < nearestDistance){
						nearestCity = this.targetKingdom.cities [i];
						nearestDistance = distance;
						path = newPath;
					}
				}
			}
		}
		return nearestCity;
	}
	internal void CreateCityWarPair(){
		if(this.warPair.kingdom1City == null || this.warPair.kingdom2City == null){
			List<HexTile> path = null;
			City kingdom1CityToBeAttacked = null;
			if(this.conqueredCities.Count > 0){
				kingdom1CityToBeAttacked = this.conqueredCities [this.conqueredCities.Count - 1];
			}
			if(kingdom1CityToBeAttacked == null){
				return;
			}
			City kingdom2CityToBeAttacked = GetNearestCityFrom(kingdom1CityToBeAttacked.hexTile, ref path);

			if(kingdom1CityToBeAttacked != null && kingdom2CityToBeAttacked != null && path != null){
				this.warPair = new CityWarPair (kingdom1CityToBeAttacked, kingdom2CityToBeAttacked, path);
			}
		}
	}
	internal void UpdateWarPair(){
		this.warPair.DefaultValues ();
		CreateCityWarPair ();
	}
	private void Attack(){
		this.attackRate += 1;
		if((this.warPair.kingdom1City == null || this.warPair.kingdom1City.isDead) || (this.warPair.kingdom2City == null || this.warPair.kingdom2City.isDead)){
			UpdateWarPair ();
			if(this.warPair.path == null){
				return;
			}
		}
		if(this.isInitialAttack){
			if(this.attackRate < KingdomManager.Instance.initialSpawnRate){
				return;
			}else{
				this.isInitialAttack = false;
			}
		}else{
			if(this.attackRate < this.warPair.spawnRate){
				return;
			}
		}
		this.attackRate = 0;
		if ((this.warPair.kingdom1City != null && !this.warPair.kingdom1City.isDead) && (this.warPair.kingdom2City != null && !this.warPair.kingdom2City.isDead)) {
			this.warPair.kingdom1City.AttackCity (this.warPair.kingdom2City, this.warPair.path, true);
			this.warPair.kingdom2City.AttackCity (this.warPair.kingdom1City, this.warPair.path);
			Reinforcement ();
		}
	}
	private void Reinforcement(){
		List<City> safeCitiesKingdom1 = this.conqueredCities.Where (x => !x.isUnderAttack && !x.hasReinforced && x.hp >= 100).ToList (); 
		List<City> safeCitiesKingdom2 = this.targetKingdom.cities.Where (x => !x.isUnderAttack && !x.hasReinforced && x.hp >= 100).ToList ();
		int chance = 0;
		int value = 0;
		if(safeCitiesKingdom1 != null){
			for(int i = 0; i < safeCitiesKingdom1.Count; i++){
				chance = UnityEngine.Random.Range (0, 100);
				value = 1 * safeCitiesKingdom1 [i].ownedTiles.Count;
				if(chance < value){
					safeCitiesKingdom1 [i].hasReinforced = true;
					safeCitiesKingdom1 [i].ReinforceCity (this.warPair.kingdom1City);
				}
			}
		}
		if(safeCitiesKingdom2 != null){
			for(int i = 0; i < safeCitiesKingdom2.Count; i++){
				chance = UnityEngine.Random.Range (0, 100);
				value = 1 * safeCitiesKingdom2 [i].ownedTiles.Count;
				if(chance < value){
					safeCitiesKingdom2 [i].hasReinforced = true;
					safeCitiesKingdom2 [i].ReinforceCity (this.warPair.kingdom2City);
				}
			}
		}
	}
	private void UpdatePath(HexTile hexTile){
		if(this.warPair.path != null && this.warPair.path.Count > 0){
			if(this.warPair.path.Contains(hexTile)){
				this.warPair.UpdateSpawnRate();
			}
		}
	}
}
