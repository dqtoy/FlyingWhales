using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomManager : MonoBehaviour {

	public static KingdomManager Instance = null;

	public List<Kingdom> allKingdoms;

	void Awake(){
		Instance = this;
	}

	public void GenerateInitialKingdoms(List<HexTile> elligibleTiles){
		List<HexTile> habitableTiles = new List<HexTile> (elligibleTiles);

		Debug.Log ("Generate Initial Kingdoms");
		//Get Starting City For Humans
		List<HexTile> cityForHumans = new List<HexTile>();
		List<HexTile> elligibleTilesForHumans = new List<HexTile>();
		for (int i = 0; i < habitableTiles.Count; i++) {
			
			List<HexTile> neighbours = habitableTiles[i].AllNeighbours.ToList();
			List<HexTile> tilesContainingBaseResource = new List<HexTile>();
			for (int j = 0; j < neighbours.Count; j++) {
				if (neighbours[j].specialResource == RESOURCE.NONE) {
					if (Utilities.GetBaseResourceType (neighbours[j].defaultResource) == BASE_RESOURCE_TYPE.STONE) {
						tilesContainingBaseResource.Add(neighbours[j]);
					}
				} else {
					if (Utilities.GetBaseResourceType (neighbours[j].specialResource) == BASE_RESOURCE_TYPE.STONE) {
						tilesContainingBaseResource.Add(neighbours[j]);
					}
				}
			}
			if (tilesContainingBaseResource.Count > 0) {
				elligibleTilesForHumans.Add(habitableTiles[i]);
			}
		}
		cityForHumans.Add (elligibleTilesForHumans [Random.Range (0, elligibleTilesForHumans.Count)]);
		GenerateNewKingdom (RACE.HUMANS, cityForHumans, true);

		for (int i = 0; i < elligibleTilesForHumans.Count; i++) {
			habitableTiles.Remove (elligibleTilesForHumans[i]);
		}

//		//Get Statrting City For Elves
//		List<HexTile> cityForElves = new List<HexTile>();
//		List<HexTile> elligibleTilesForElves = new List<HexTile>();
//		for (int i = 0; i < habitableTiles.Count; i++) {
//
//			List<HexTile> neighbours = habitableTiles[i].AllNeighbours.ToList();
//			List<HexTile> tilesContainingBaseResource = new List<HexTile>();
//			for (int j = 0; j < neighbours.Count; j++) {
//				if (neighbours[j].specialResource == RESOURCE.NONE) {
//					if (Utilities.GetBaseResourceType (neighbours[j].defaultResource) == BASE_RESOURCE_TYPE.WOOD) {
//						tilesContainingBaseResource.Add(neighbours[j]);
//					}
//				} else {
//					if (Utilities.GetBaseResourceType (neighbours[j].specialResource) == BASE_RESOURCE_TYPE.WOOD) {
//						tilesContainingBaseResource.Add(neighbours[j]);
//					}
//				}
//			}
//
//			if (tilesContainingBaseResource.Count > 0) {
//				elligibleTilesForElves.Add(habitableTiles[i]);
//			}
//		}
//		cityForElves.Add (elligibleTilesForElves [Random.Range (0, elligibleTilesForElves.Count)]);
//		GenerateNewKingdom (RACE.ELVES, cityForElves);
		CreateInitialRelationshipKings ();
	}

	internal void CreateInitialRelationshipKings(){
		for(int i = 0; i < this.allKingdoms.Count; i++){
			this.allKingdoms [i].king.CreateInitialRelationshipsToKings ();
		}
	}
	public void GenerateNewKingdom(RACE race, List<HexTile> cities, bool isForInitial = false){
		Kingdom newKingdom = new Kingdom (race, cities);
		allKingdoms.Add(newKingdom);
		this.UpdateKingdomAdjacency();
		EventManager.Instance.onCreateNewKingdomEvent.Invoke(newKingdom);
		if (isForInitial) {
			cities [0].city.CreateInitialFamilies();
		}
	}

	public void DeclareWarBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
		kingdom1.GetRelationshipWithOtherKingdom(kingdom2).isAtWar = true;
		kingdom2.GetRelationshipWithOtherKingdom(kingdom1).isAtWar = true;
	}

	public void DeclarePeaceBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2){
		kingdom1.GetRelationshipWithOtherKingdom(kingdom2).isAtWar = false;
		kingdom2.GetRelationshipWithOtherKingdom(kingdom1).isAtWar = false;
	}

	public void AddRelationshipToOtherKings(Citizen newKing){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].id != newKing.city.kingdom.id) {
				this.allKingdoms[i].king.relationshipKings.Add (new RelationshipKings(this.allKingdoms[i].king, newKing, 0));
			}
		}
	}
	public void RemoveRelationshipToOtherKings(Citizen oldKing){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].id != oldKing.city.kingdom.id) {
				this.allKingdoms[i].king.relationshipKings.RemoveAll (x => x.king.id == oldKing.id);
			}
		}
	}

	public void MakeKingdomDead(Kingdom kingdomToDie){
		this.allKingdoms.Remove(kingdomToDie);
		RemoveRelationshipToOtherKingdoms (kingdomToDie);
	}

	public void RemoveRelationshipToOtherKingdoms(Kingdom kingdomToRemove){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			for (int j = 0; j < this.allKingdoms[i].relationshipsWithOtherKingdoms.Count; j++) {
				if (this.allKingdoms[i].relationshipsWithOtherKingdoms[j].objectInRelationship.id == kingdomToRemove.id) {
					this.allKingdoms[i].relationshipsWithOtherKingdoms.RemoveAt(j);
					break;
				}
			}
		}
	}

	public void UpdateKingdomAdjacency(){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			Kingdom currentKingdom = this.allKingdoms[i];
			currentKingdom.ResetAdjacencyWithOtherKingdoms();
			for (int j = 0; j < currentKingdom.cities.Count; j++) {
				City currentCity = currentKingdom.cities[j];
				for (int k = 0; k < currentCity.hexTile.connectedTiles.Count; k++) {
					HexTile currentConnectedTile = currentCity.hexTile.connectedTiles[k];
					if (currentConnectedTile.isOccupied && currentConnectedTile.city != null) {
						if (currentConnectedTile.city.kingdom.id != currentKingdom.id) {
							currentKingdom.GetRelationshipWithOtherKingdom(currentConnectedTile.city.kingdom).isAdjacent = true;
							currentConnectedTile.city.kingdom.GetRelationshipWithOtherKingdom(currentKingdom).isAdjacent = true;
						}
					}
				}
			}
		}
	}
}
