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
		GenerateNewKingdom (RACE.HUMANS, cityForHumans);

		for (int i = 0; i < elligibleTilesForHumans.Count; i++) {
			habitableTiles.Remove (elligibleTilesForHumans[i]);
		}

		//Get Statrting City For Elves
		List<HexTile> cityForElves = new List<HexTile>();
		List<HexTile> elligibleTilesForElves = new List<HexTile>();
		for (int i = 0; i < habitableTiles.Count; i++) {

			List<HexTile> neighbours = habitableTiles[i].AllNeighbours.ToList();
			List<HexTile> tilesContainingBaseResource = new List<HexTile>();
			for (int j = 0; j < neighbours.Count; j++) {
				if (neighbours[j].specialResource == RESOURCE.NONE) {
					if (Utilities.GetBaseResourceType (neighbours[j].defaultResource) == BASE_RESOURCE_TYPE.WOOD) {
						tilesContainingBaseResource.Add(neighbours[j]);
					}
				} else {
					if (Utilities.GetBaseResourceType (neighbours[j].specialResource) == BASE_RESOURCE_TYPE.WOOD) {
						tilesContainingBaseResource.Add(neighbours[j]);
					}
				}
			}

			if (tilesContainingBaseResource.Count > 0) {
				elligibleTilesForElves.Add(habitableTiles[i]);
			}
		}
		cityForElves.Add (elligibleTilesForElves [Random.Range (0, elligibleTilesForElves.Count)]);
		GenerateNewKingdom (RACE.ELVES, cityForElves);
	}

	public void GenerateNewKingdom(RACE race, List<HexTile> cities){
		Kingdom newKingdom = new Kingdom (race, cities);
		allKingdoms.Add(newKingdom);
	}

	public void AddRelationshipToOtherKings(Citizen newKing){
		for (int i = 0; i < this.allKingdoms.Count; i++) {
			if (this.allKingdoms[i].id != newKing.city.kingdom.id) {
				this.allKingdoms[i].king.relationshipKings.Add (new RelationshipKings(newKing, 0));
			}
		}
	}

}
