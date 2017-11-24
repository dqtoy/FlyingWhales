using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class CaravaneerThread {
	public enum TODO{
		FIND_PATH,
	}

	private TODO _todo;

	private City sourceCity;
	private City targetCity;
	private List <HexTile> path;

	private Kingdom sourceKingdom;
	private Caravaneer caravaneer;
	private RESOURCE_TYPE neededResource;

	public CaravaneerThread(Caravaneer caravaneer){
		this.caravaneer = caravaneer;
		this.neededResource = caravaneer.neededResource;
		this.sourceCity = caravaneer.sourceCity;
		this.sourceKingdom = caravaneer.sourceCity.kingdom;
	}

	public void ObtainCity(){
		List<City> citiesToChooseFrom = new List<City> ();
		if(neededResource == RESOURCE_TYPE.FOOD){
			citiesToChooseFrom = this.sourceKingdom.cities.Where (x => x.id != this.sourceCity.id && !this.sourceCity.blacklist.Contains(x) && x.region.tileWithSpecialResource.specialResourceType == neededResource && x.foodForTrade >= x.foodRequirement).ToList();
		}else if(neededResource == RESOURCE_TYPE.MATERIAL){
			if(this.sourceKingdom.race == RACE.HUMANS){
				citiesToChooseFrom = this.sourceKingdom.cities.Where (x => x.id != this.sourceCity.id && !this.sourceCity.blacklist.Contains(x) && (x.region.tileWithSpecialResource.specialResource == RESOURCE.SLATE || x.region.tileWithSpecialResource.specialResource == RESOURCE.GRANITE) && x.materialForTrade >= x.materialRequirement).ToList();
			}else if(this.sourceKingdom.race == RACE.ELVES){
				citiesToChooseFrom = this.sourceKingdom.cities.Where (x => x.id != this.sourceCity.id && !this.sourceCity.blacklist.Contains(x) && (x.region.tileWithSpecialResource.specialResource == RESOURCE.OAK || x.region.tileWithSpecialResource.specialResource == RESOURCE.EBONY) && x.materialForTrade >= x.materialRequirement).ToList();
			}
		}else if(neededResource == RESOURCE_TYPE.ORE){
			citiesToChooseFrom = this.sourceKingdom.cities.Where (x => x.id != this.sourceCity.id && !this.sourceCity.blacklist.Contains(x) && x.region.tileWithSpecialResource.specialResourceType == neededResource && x.oreForTrade >= x.oreRequirement).ToList();
		}

		City chosenCity = null;
		int shortestPath = 0;
		for (int i = 0; i < citiesToChooseFrom.Count; i++) {
			City city = citiesToChooseFrom [i];
			List<HexTile> newPath = PathGenerator.Instance.GetPath (this.sourceCity.hexTile, city.hexTile, PATHFINDING_MODE.USE_ROADS_TRADE, this.sourceKingdom);
			if (newPath != null && newPath.Count > 0) {
				if(chosenCity == null){
					chosenCity = city;
					path = newPath;
					shortestPath = newPath.Count;
				}else{
					if(newPath.Count < shortestPath){
						chosenCity = city;
						path = newPath;
						shortestPath = newPath.Count;
					}
				}
			}
		}
		if(chosenCity != null){
			this.targetCity = chosenCity;
			this.path = path;
		}
	}
	public void Return(){
		this.caravaneer.ReceiveCityToObtainResource (this.targetCity, this.path);
	}
}
