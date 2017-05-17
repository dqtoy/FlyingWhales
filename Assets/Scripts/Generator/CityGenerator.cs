using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class CityGenerator : MonoBehaviour {

	public static CityGenerator Instance = null;

	public List<HexTile> habitableTiles;

	public Sprite elfCitySprite;
	public Sprite elfFarmSprite;
	public Sprite elfQuarrySprite;
	public Sprite elfLumberyardSprite;
	public Sprite elfTraderSprite;
	public Sprite elfHuntingLodgeSprite;
	public Sprite elfMiningSprite;
	public Sprite elfBarracks;
	public Sprite elfSpyGuild;
	public Sprite elfMinistry;
	public Sprite elfKeep;

	public TextAsset cityBehaviourTree;

	void Awake(){
		Instance = this;
	}

	public void GenerateHabitableTiles(List<GameObject> allHexes){
		habitableTiles = new List<HexTile>();

		List<GameObject> elligibleTiles = new List<GameObject>(allHexes);
//		Debug.Log ("elligible Tiles: " + elligibleTiles.Count.ToString ());
		for (int i = 0; i < elligibleTiles.Count; i++) {

			HexTile currentHexTile = elligibleTiles [i].GetComponent<HexTile>();
			if (currentHexTile.xCoordinate >= (GridMap.Instance.width - 3) || currentHexTile.xCoordinate < 3 || 
				currentHexTile.yCoordinate >= (GridMap.Instance.width - 3) || currentHexTile.yCoordinate < 3 ||
				currentHexTile.elevationType == ELEVATION.WATER || currentHexTile.elevationType == ELEVATION.MOUNTAIN || 
				currentHexTile.specialResource != RESOURCE.NONE) {
				//skip hextiles within 3 tiles of the edge
				continue;
			}

//			List<HexTile> adjacentTiles = currentHexTile.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER).OrderBy(x => x.specialResource == RESOURCE.NONE).ToList();
//			if (adjacentTiles.Count < 4) {
//				continue;
//			}


			List<HexTile> checkForHabitableTilesInRange = currentHexTile.GetTilesInRange (4);
			if (checkForHabitableTilesInRange.Where (x => x.isHabitable).Count () > 0) {
				continue;
			}

			List<HexTile> tilesInRange = currentHexTile.GetTilesInRange(2);
			for (int j = 0; j < tilesInRange.Count; j++) {
				if (tilesInRange [j].specialResource != RESOURCE.NONE) {
					if (Utilities.GetBaseResourceType (tilesInRange [j].specialResource) == BASE_RESOURCE_TYPE.STONE) {
						currentHexTile.nearbyStoneCount++;
					} else if (Utilities.GetBaseResourceType (tilesInRange [j].specialResource) == BASE_RESOURCE_TYPE.WOOD) {
						currentHexTile.nearbyWoodCount++;
					} else {
						currentHexTile.nearbySpecialCount++;
					}
				}
			}

			if (currentHexTile.nearbyStoneCount >= 2 || currentHexTile.nearbyWoodCount >= 2 
					|| (currentHexTile.nearbyStoneCount >= 1 && currentHexTile.nearbySpecialCount >= 1) 
					|| (currentHexTile.nearbyWoodCount >= 1 && currentHexTile.nearbySpecialCount >= 1)) {

				SetTileAsHabitable(currentHexTile);
				elligibleTiles.Remove(currentHexTile.gameObject);
				for (int j = 0; j < tilesInRange.Count; j++) {
					if (elligibleTiles.Contains (tilesInRange [j].gameObject)) {
						elligibleTiles.Remove (tilesInRange [j].gameObject);
					}
				}
			}
		}
	}

	private void SetTileAsHabitable(HexTile hexTile){
		hexTile.isHabitable = true;
		habitableTiles.Add(hexTile);
		hexTile.GetComponent<SpriteRenderer>().color = Color.black;
	}

	// This will return the nearest habitable tile that matches the following criteria
	//	- unoccupied
	//	- has a nearby basic resource needed by the expanding race
	public HexTile GetNearestHabitableTile(City city) {
		int shortestDistance = 99999, currentDistance = 0;
		HexTile nearestTile = null;

		if (city.kingdom.basicResource == BASE_RESOURCE_TYPE.STONE) {
			for (int i = 0; i < this.habitableTiles.Count; i++) {
				
				if (this.habitableTiles [i].nearbyStoneCount >= 1 && !this.habitableTiles [i].isOccupied && !this.habitableTiles [i].isBorder) {
					List<HexTile> checkForBorderTilesInRange = this.habitableTiles [i].GetTilesInRange (2);
					if (checkForBorderTilesInRange.Where (x => (x.ownedByCity != null && x.ownedByCity.kingdom != city.kingdom)).Count () > 1) {
						continue;
					} else {						
						currentDistance = PathGenerator.Instance.GetDistanceBetweenTwoTiles (city.hexTile, this.habitableTiles [i]);
						if (currentDistance < shortestDistance) {
							shortestDistance = currentDistance;
							nearestTile = this.habitableTiles [i];
						}
					}
				}
			}

		} else {
			for (int i = 0; i < this.habitableTiles.Count; i++) {
				if (this.habitableTiles [i].nearbyWoodCount >= 1 && !this.habitableTiles [i].isOccupied && !this.habitableTiles [i].isBorder) {
					List<HexTile> checkForBorderTilesInRange = this.habitableTiles [i].GetTilesInRange (2);
					if (checkForBorderTilesInRange.Where (x => (x.ownedByCity != null && x.ownedByCity.kingdom != city.kingdom)).Count () > 1) {
						continue;
					} else {
						currentDistance = PathGenerator.Instance.GetDistanceBetweenTwoTiles (city.hexTile, this.habitableTiles [i]);
						if (currentDistance < shortestDistance) {
							shortestDistance = currentDistance;
							nearestTile = this.habitableTiles [i];
						}
					}
				}
			}

		}
		return nearestTile;
	}

	public City CreateNewCity(HexTile hexTile, Kingdom kingdom){
		hexTile.city = new City (hexTile, kingdom);
		kingdom.AddCityToKingdom(hexTile.city);
		hexTile.ShowCitySprite();
		hexTile.ShowNamePlate();
		if (hexTile.gameObject.GetComponent<CityTaskManager> () != null) {
			Destroy (hexTile.gameObject.GetComponent<CityTaskManager> ());
		}
		if (hexTile.gameObject.GetComponent<PandaBehaviour> () != null) {
			Destroy (hexTile.gameObject.GetComponent<PandaBehaviour> ());
		}

		hexTile.gameObject.AddComponent<CityTaskManager>();
		hexTile.gameObject.AddComponent<PandaBehaviour>();
		hexTile.gameObject.GetComponent<PandaBehaviour>().tickOn = BehaviourTree.UpdateOrder.Manual;
		hexTile.gameObject.GetComponent<PandaBehaviour>().Compile (cityBehaviourTree.text);
		BehaviourTreeManager.Instance.allTrees.Add(hexTile.gameObject.GetComponent<PandaBehaviour> ());
		return hexTile.city;
	}

	public City GetCityByID(int id){
		for (int i = 0; i < this.habitableTiles.Count; i++) {
			HexTile currHabitableTile = this.habitableTiles[i];
			if (currHabitableTile.isOccupied && currHabitableTile.city.id == id) {
				return currHabitableTile.city;
			}
		}
		return null;
	}
}
