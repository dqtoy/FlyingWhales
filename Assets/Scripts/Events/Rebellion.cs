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

	public Rebellion(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.rebelLeader = (Rebel)startedBy.assignedRole;
		this.targetKingdom = startedBy.city.kingdom;
//		this.targetCity = null;
//		this.sourceCityOrFort = null;
		this.conqueredCities = new List<City> ();
		this.targetKingdom.rebellions.Add (this);
		this.warPair.DefaultValues ();
		CreateRebelFort ();
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

	}
	#region Overrides
	internal override void PerformAction (){
		if(!this.targetKingdom.isAlive()){
			if(this.conqueredCities.Count > 0){
				//Victory Rebellion
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

	}

	#endregion
	private void CreateRebelFort(){
		HexTile hexTile = GetRandomBordreTileForFort ();
		this.rebelFort = new RebelFort (hexTile, this.targetKingdom, this);
//		this.rebelLeader.citizen.city.citizens.Remove (this.rebelLeader.citizen);
//		this.rebelFort.citizens.Add (this.rebelLeader.citizen);
		this.rebelLeader.citizen.city = this.rebelFort;
//		this.conqueredCities.Add (this.rebelFort);
	}
	private HexTile GetRandomBordreTileForFort(){
		return this.rebelLeader.citizen.city.borderTiles [UnityEngine.Random.Range (0, this.rebelLeader.citizen.city.borderTiles.Count)];
	}
	private void GetSourceAndTargetCity(){
		int nearestDistance = 0;
		City source = null;
		City target = null;
		for (int i = 0; i < this.conqueredCities.Count; i++) {
			for (int j = 0; j < this.targetKingdom.cities.Count; j++) {
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
	internal void CreateCityWarPair(){
		if(this.warPair.kingdom1City == null || this.warPair.kingdom2City == null){
			List<HexTile> path = null;
			City kingdom1CityToBeAttacked = null;
//			for (int i = 0; i < this.kingdom2.capitalCity.habitableTileDistance.Count; i++) {
//				if(this.kingdom2.capitalCity.habitableTileDistance[i].hexTile.city != null){
//					if(this.kingdom2.capitalCity.habitableTileDistance[i].hexTile.city.kingdom.id == this.kingdom1.id){
//						kingdom1CityToBeAttacked = this.kingdom2.capitalCity.habitableTileDistance [i].hexTile.city;
//						break;
//					}
//				}
//			}
			City kingdom2CityToBeAttacked = null;
//			for (int i = 0; i < this.kingdom1.capitalCity.habitableTileDistance.Count; i++) {
//				if(this.kingdom1.capitalCity.habitableTileDistance[i].hexTile.city != null){
//					if(this.kingdom1.capitalCity.habitableTileDistance[i].hexTile.city.kingdom.id == this.kingdom2.id){
//						path = PathGenerator.Instance.GetPath (kingdom1CityToBeAttacked.hexTile, this.kingdom1.capitalCity.habitableTileDistance [i].hexTile, PATHFINDING_MODE.AVATAR).ToList();
//						if(path != null){
//							kingdom2CityToBeAttacked = this.kingdom1.capitalCity.habitableTileDistance [i].hexTile.city;
//							break;
//						}
//
//					}
//				}
//			}

			if(kingdom1CityToBeAttacked != null && kingdom2CityToBeAttacked != null && path != null){
				this.warPair = new CityWarPair (kingdom1CityToBeAttacked, kingdom2CityToBeAttacked, path);
			}
		}
	}
	internal void UpdateWarPair(){
		this.warPair.DefaultValues ();
		CreateCityWarPair ();
	}
}
