using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TradeManager {

	public City city;
	public Kingdom kingdom;

	public int lastMonthUpdated;
	public int numberOfTimesStarved;
	public List<BASE_RESOURCE_TYPE> neededResources;
	public List<BASE_RESOURCE_TYPE> offeredResources;

	public int sustainabilityBuff = 0;

	public TradeManager(City belongsToCity, Kingdom belongsToKingdom){
		this.city = belongsToCity;
		this.kingdom = belongsToKingdom;
		this.numberOfTimesStarved = 0;
		this.neededResources = new List<BASE_RESOURCE_TYPE>();
		this.offeredResources = new List<BASE_RESOURCE_TYPE>();
//		this.UpdateNeededResources();
	}

//	internal void UpdateNeededResources(){
//		neededResources.Clear();
//		offeredResources.Clear();
//		//Determine needed resources
//		if (this.numberOfTimesStarved > 0 || this.city.citizens.Count >= this.city.sustainability) {
//			neededResources.Add (BASE_RESOURCE_TYPE.FOOD);
//		}
//
////		if (this.city.IsProducingResource (this.kingdom.rareResource)) {
//			neededResources.Add (this.kingdom.basicResource);
////		} else {
//			neededResources.Add (this.kingdom.rareResource);
////		}
//		this.numberOfTimesStarved = 0;
//		this.lastMonthUpdated = GameManager.Instance.month;
//	}
//
//	internal List<BASE_RESOURCE_TYPE> DetermineOfferedResources(){
//		//Determine offered resources
//		this.offeredResources.Clear();
//		if (this.city.ownedTiles.Where (x => x.structureOnTile == STRUCTURE.FARM || x.structureOnTile == STRUCTURE.HUNTING_LODGE).ToList ().Count >= 3) {
//			offeredResources.Add(BASE_RESOURCE_TYPE.FOOD);
//		}
//		if (this.city.IsProducingResource (BASE_RESOURCE_TYPE.COBALT)) {
//			offeredResources.Add(BASE_RESOURCE_TYPE.COBALT);
//		}
//		if (this.city.IsProducingResource (BASE_RESOURCE_TYPE.MANA_STONE)) {
//			offeredResources.Add(BASE_RESOURCE_TYPE.MANA_STONE);
//		}
//		if (this.city.IsProducingResource (BASE_RESOURCE_TYPE.MITHRIL)) {
//			offeredResources.Add(BASE_RESOURCE_TYPE.MITHRIL);
//		}
//		if (this.city.IsProducingResource (BASE_RESOURCE_TYPE.STONE)) {
//			offeredResources.Add(BASE_RESOURCE_TYPE.STONE);
//		}
//		if (this.city.IsProducingResource (BASE_RESOURCE_TYPE.WOOD)) {
//			offeredResources.Add(BASE_RESOURCE_TYPE.WOOD);
//		}
//		return this.offeredResources;
//	}

//	internal City GetTargetCity(){
//		//allied kingdoms
//		List<City> citiesOrderedByDistance = new List<City>();
//		List<Kingdom> alliedKingdoms = this.kingdom.GetKingdomsByRelationship(RELATIONSHIP_STATUS.ALLY);
//		if (alliedKingdoms.Count > 0) {
//			for (int i = 0; i < alliedKingdoms.Count; i++) {
//				citiesOrderedByDistance = alliedKingdoms [i].cities.
//					OrderByDescending (x => Vector2.Distance (this.city.hexTile.transform.position, x.hexTile.transform.position)).ToList ();
//				for (int j = 0; j < citiesOrderedByDistance.Count; j++) {
//					List<BASE_RESOURCE_TYPE> tradeableResources = this.offeredResources.Intersect(citiesOrderedByDistance[j].tradeManager.neededResources).ToList();
//					if (tradeableResources.Count > 0) {
//						if (PathGenerator.Instance.GetPath (this.city.hexTile, citiesOrderedByDistance[j].hexTile, PATHFINDING_MODE.NORMAL) != null) {
//							return citiesOrderedByDistance [j];
//						}
//					}
//				}
//			}
//		}
//
//		//other cities from same kingdoms
//		citiesOrderedByDistance = this.kingdom.cities.
//			OrderByDescending (x => Vector2.Distance (this.city.hexTile.transform.position, x.hexTile.transform.position)).ToList ();
//		for (int i = 0; i < citiesOrderedByDistance.Count; i++) {
//			if (citiesOrderedByDistance[i].id != this.city.id) {
//				List<BASE_RESOURCE_TYPE> tradeableResources = this.offeredResources.Intersect(citiesOrderedByDistance[i].tradeManager.neededResources).ToList();
//				if (tradeableResources.Count > 0) {
//					if (PathGenerator.Instance.GetPath (this.city.hexTile, citiesOrderedByDistance[i].hexTile, PATHFINDING_MODE.NORMAL) != null) {
//						return citiesOrderedByDistance [i];
//					}
//				}
//			}
//		}
//
//		//friendly kingdoms
//		List<Kingdom> friendlyKingdoms = this.kingdom.GetKingdomsByRelationship(RELATIONSHIP_STATUS.FRIEND);
//		if (friendlyKingdoms.Count > 0) {
//			for (int i = 0; i < friendlyKingdoms.Count; i++) {
//				citiesOrderedByDistance = friendlyKingdoms[i].cities.
//					OrderByDescending (x => Vector2.Distance (this.city.hexTile.transform.position, x.hexTile.transform.position)).ToList ();
//				for (int j = 0; j < citiesOrderedByDistance.Count; j++) {
//					List<BASE_RESOURCE_TYPE> tradeableResources = this.offeredResources.Intersect(citiesOrderedByDistance[j].tradeManager.neededResources).ToList();
//					if (tradeableResources.Count > 0) {
//						if (PathGenerator.Instance.GetPath (this.city.hexTile, citiesOrderedByDistance [j].hexTile, PATHFINDING_MODE.NORMAL) != null) {
//							return citiesOrderedByDistance [j];
//						}
//					}
//				}
//			}
//		}
//
//		//warm kingdoms
//		List<Kingdom> warmKingdoms = this.kingdom.GetKingdomsByRelationship(RELATIONSHIP_STATUS.WARM);
//		if (warmKingdoms.Count > 0) {
//			for (int i = 0; i < warmKingdoms.Count; i++) {
//				citiesOrderedByDistance = warmKingdoms[i].cities.
//					OrderByDescending (x => Vector2.Distance (this.city.hexTile.transform.position, x.hexTile.transform.position)).ToList ();
//				for (int j = 0; j < citiesOrderedByDistance.Count; j++) {
//					List<BASE_RESOURCE_TYPE> tradeableResources = this.offeredResources.Intersect(citiesOrderedByDistance[j].tradeManager.neededResources).ToList();
//					if (tradeableResources.Count > 0) {
//						if (PathGenerator.Instance.GetPath (this.city.hexTile, citiesOrderedByDistance [j].hexTile, PATHFINDING_MODE.NORMAL) != null) {
//							return citiesOrderedByDistance [j];
//						}
//					}
//				}
//			}
//		}
//
//		//neutral kingdoms
//		List<Kingdom> neutralKingdoms = this.kingdom.GetKingdomsByRelationship(RELATIONSHIP_STATUS.NEUTRAL);
//		if (neutralKingdoms.Count > 0) {
//			for (int i = 0; i < neutralKingdoms.Count; i++) {
//				citiesOrderedByDistance = neutralKingdoms[i].cities.
//					OrderByDescending (x => Vector2.Distance (this.city.hexTile.transform.position, x.hexTile.transform.position)).ToList ();
//				for (int j = 0; j < citiesOrderedByDistance.Count; j++) {
//					List<BASE_RESOURCE_TYPE> tradeableResources = this.offeredResources.Intersect(citiesOrderedByDistance[j].tradeManager.neededResources).ToList();
//					if (tradeableResources.Count > 0) {
//						if (PathGenerator.Instance.GetPath (this.city.hexTile, citiesOrderedByDistance [j].hexTile, PATHFINDING_MODE.NORMAL) != null) {
//							return citiesOrderedByDistance [j];
//						}
//					}
//				}
//			}
//		}
//
//		//cold kingdoms
//		List<Kingdom> coldKingdoms = this.kingdom.GetKingdomsByRelationship(RELATIONSHIP_STATUS.COLD);
//		if (coldKingdoms.Count > 0) {
//			for (int i = 0; i < coldKingdoms.Count; i++) {
//				citiesOrderedByDistance = coldKingdoms[i].cities.
//					OrderByDescending (x => Vector2.Distance (this.city.hexTile.transform.position, x.hexTile.transform.position)).ToList ();
//				for (int j = 0; j < citiesOrderedByDistance.Count; j++) {
//					List<BASE_RESOURCE_TYPE> tradeableResources = this.offeredResources.Intersect(citiesOrderedByDistance[j].tradeManager.neededResources).ToList();
//					if (tradeableResources.Count > 0) {
//						if (PathGenerator.Instance.GetPath (this.city.hexTile, citiesOrderedByDistance [j].hexTile, PATHFINDING_MODE.NORMAL) != null) {
//							return citiesOrderedByDistance [j];
//						}
//					}
//				}
//			}
//		}
//
//		return null;
//	}
}
