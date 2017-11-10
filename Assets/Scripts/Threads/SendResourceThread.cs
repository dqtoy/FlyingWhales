using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class SendResourceThread {
	public enum TODO{
		FIND_PATH,
	}

	private TODO _todo;

	private City sourceCity;
	private City targetCity;
	private int foodAmount;
	private int materialAmount;
	private int oreAmount;
	private RESOURCE_TYPE resourceType;
	private HexTile sourceHextile;
	private HexTile targetHextile;
	private List <HexTile> path;

	public SendResourceThread(int foodAmount, int materialAmount, int oreAmount, RESOURCE_TYPE resourceType, HexTile sourceHextile, City sourceCity){
		this.foodAmount = foodAmount;
		this.materialAmount = materialAmount;
		this.oreAmount = oreAmount;
		this.resourceType = resourceType;
		this.sourceHextile = sourceHextile;
		this.sourceCity = sourceCity;
	}

	public void SendResource(){
//		List<City> orderedCities = this.sourceCity.kingdom.cities.OrderBy (x => x.hexTile.GetDistanceTo (this.sourceCity.hexTile)).ToList ();
//		orderedCities.Remove (this.sourceCity);
		City chosenCity = null;
//		int lowestResourceCount = 0;
		int shortestPath = 0;
		List<HexTile> path = new List<HexTile> ();
		if(resourceType == RESOURCE_TYPE.FOOD){
			for (int i = 0; i < this.sourceCity.kingdom.cities.Count; i++) {
				City city = this.sourceCity.kingdom.cities [i];
				if(city.virtualFoodCount < city.foodRequirement && this.sourceCity.id != city.id){
					List<HexTile> newPath = PathGenerator.Instance.GetPath (this.sourceCity.hexTile, city.hexTile, PATHFINDING_MODE.USE_ROADS_WITH_ALLIES, this.sourceCity.kingdom);
					if (newPath != null && newPath.Count > 0) {
						if(chosenCity == null){
							chosenCity = city;
//							lowestResourceCount = orderedCities[i].foodCount;
							path = newPath;
							shortestPath = newPath.Count;
						}else{
							if(newPath.Count < shortestPath){
								chosenCity = city;
//								lowestResourceCount = orderedCities[i].foodCount;
								path = newPath;
								shortestPath = newPath.Count;
							}
						}
					}
				}
			}
		}else if(resourceType == RESOURCE_TYPE.MATERIAL){
			for (int i = 0; i < this.sourceCity.kingdom.cities.Count; i++) {
				City city = this.sourceCity.kingdom.cities [i];
				if(city.virtualMaterialCount < city.materialRequirement && this.sourceCity.id != city.id){
					List<HexTile> newPath = PathGenerator.Instance.GetPath (this.sourceCity.hexTile, city.hexTile, PATHFINDING_MODE.USE_ROADS_WITH_ALLIES, this.sourceCity.kingdom);
					if (newPath != null && newPath.Count > 0) {
						if(chosenCity == null){
							chosenCity = city;
//							lowestResourceCount = orderedCities[i].foodCount;
							path = newPath;
							shortestPath = newPath.Count;
						}else{
							if(newPath.Count < shortestPath){
								chosenCity = city;
//								lowestResourceCount = orderedCities[i].foodCount;
								path = newPath;
								shortestPath = newPath.Count;
							}
						}
					}
				}
			}
		}else if(resourceType == RESOURCE_TYPE.ORE){
			for (int i = 0; i < this.sourceCity.kingdom.cities.Count; i++) {
				City city = this.sourceCity.kingdom.cities [i];
				if(city.virtualOreCount < city.oreRequirement && this.sourceCity.id != city.id){
					List<HexTile> newPath = PathGenerator.Instance.GetPath (this.sourceCity.hexTile, city.hexTile, PATHFINDING_MODE.USE_ROADS_WITH_ALLIES, this.sourceCity.kingdom);
					if (newPath != null && newPath.Count > 0) {
						if(chosenCity == null){
							chosenCity = city;
//							lowestResourceCount = orderedCities[i].foodCount;
							path = newPath;
							shortestPath = newPath.Count;
						}else{
							if(newPath.Count < shortestPath){
								chosenCity = city;
//								lowestResourceCount = orderedCities[i].foodCount;
								path = newPath;
								shortestPath = newPath.Count;
							}
						}
					}
				}
			}
		}



		if(chosenCity != null){
			this.targetCity = chosenCity;
			this.targetHextile = chosenCity.hexTile;
			this.path = path;
		}
	}

	public void ReturnResource(){
		this.sourceCity.ReceiveSendResourceThread (this.foodAmount, this.materialAmount, this.oreAmount, this.resourceType, this.sourceHextile, this.targetHextile, this.targetCity, this.path);
	}
}
