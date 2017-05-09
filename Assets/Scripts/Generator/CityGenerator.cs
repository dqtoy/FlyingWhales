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
		Debug.Log ("elligible Tiles: " + elligibleTiles.Count.ToString ());
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

			List<HexTile> tilesInRange = currentHexTile.GetTilesInRange(9f);
			List<HexTile> checkForHabitableTilesInRange = currentHexTile.GetTilesInRange (11f);
			if (checkForHabitableTilesInRange.Where (x => x.isHabitable).Count () > 0) {
				continue;
			}
			int basicResourceCount = 0;
			for (int j = 0; j < tilesInRange.Count; j++) {
				if (tilesInRange [j].specialResource != RESOURCE.NONE) {
					if (Utilities.GetBaseResourceType (tilesInRange[j].specialResource) == BASE_RESOURCE_TYPE.STONE ||
						Utilities.GetBaseResourceType (tilesInRange[j].specialResource) == BASE_RESOURCE_TYPE.WOOD) {
						basicResourceCount++;
					}
				} 
//				else {
//					if (Utilities.GetBaseResourceType (tilesInRange[j].defaultResource) == BASE_RESOURCE_TYPE.STONE ||
//						Utilities.GetBaseResourceType (tilesInRange[j].defaultResource) == BASE_RESOURCE_TYPE.WOOD) {
//						basicResourceCount++;
//					}
//				}
			}

			if (basicResourceCount >= 3) {
				SetTileAsHabitable(currentHexTile);
				elligibleTiles.Remove(currentHexTile.gameObject);
				for (int j = 0; j < tilesInRange.Count; j++) {
					if (elligibleTiles.Contains (tilesInRange [j].gameObject)) {
						elligibleTiles.Remove (tilesInRange [j].gameObject);
					}
				}
			}
//			List<HexTile> foodTiles = new List<HexTile>();
//			for (int j = 0; j < adjacentTiles.Count; j++) {
//				HexTile possibleFoodTile = adjacentTiles[j];
//				if (possibleFoodTile.specialResource != RESOURCE.NONE) {
//					if (Utilities.GetBaseResourceType (possibleFoodTile.specialResource) == BASE_RESOURCE_TYPE.FOOD) {
//						foodTiles.Add(possibleFoodTile);
//					}
//				} else {
//					if (Utilities.GetBaseResourceType(possibleFoodTile.defaultResource) == BASE_RESOURCE_TYPE.FOOD) {
//						foodTiles.Add(possibleFoodTile);
//					}
//				}
//				if (foodTiles.Count == 2) {
//					break;
//				}
//			}

//			if (foodTiles.Count >= 2) {
//				List<HexTile> basicResourceTiles = new List<HexTile>();
//				for (int j = 0; j < adjacentTiles.Count; j++) {
//					if (!foodTiles.Contains (adjacentTiles [j])) {
//						HexTile possibleBasicTile = adjacentTiles [j];
//						if (possibleBasicTile.specialResource != RESOURCE.NONE) {
//							if (Utilities.GetBaseResourceType (possibleBasicTile.specialResource) == BASE_RESOURCE_TYPE.STONE ||
//								Utilities.GetBaseResourceType (possibleBasicTile.specialResource) == BASE_RESOURCE_TYPE.WOOD) {
//								basicResourceTiles.Add(possibleBasicTile);
//							}
//						} else {
//							if (Utilities.GetBaseResourceType (possibleBasicTile.defaultResource) == BASE_RESOURCE_TYPE.STONE ||
//								Utilities.GetBaseResourceType (possibleBasicTile.defaultResource) == BASE_RESOURCE_TYPE.WOOD) {
//								basicResourceTiles.Add(possibleBasicTile);
//							}
//						}
//						if (basicResourceTiles.Count == 1) {
//							break;
//						}
//					}
//				}
//				if (basicResourceTiles.Count >= 1) {
//					List<HexTile> nonSpecialTiles = new List<HexTile>();
//					for (int j = 0; j < adjacentTiles.Count; j++) {
//						if (!foodTiles.Contains (adjacentTiles [j]) && !basicResourceTiles.Contains (adjacentTiles [j])) {
//							HexTile possibleNonSpecialTile = adjacentTiles[j];
//							if (possibleNonSpecialTile.specialResource == RESOURCE.NONE) {
//								nonSpecialTiles.Add (possibleNonSpecialTile);
//							}
//							if (nonSpecialTiles.Count == 1) {
//								break;
//							}
//						}
//					}
//					if (nonSpecialTiles.Count >= 1) {
//						List<HexTile> specialTiles = new List<HexTile>();
//						List<HexTile> nearCityTiles = new List<HexTile>();
//						for (int j = 0; j < tilesInRange.Count; j++) {
//							if (tilesInRange [j].specialResource != RESOURCE.NONE) {
//								specialTiles.Add (tilesInRange [j]);
//							}
//
//							if (tilesInRange [j].isHabitable) {
//								nearCityTiles.Add (tilesInRange [j]);
//							}
//						}
//						if (specialTiles.Count >= 3 && nearCityTiles.Count <= 0) {
//							SetTileAsHabitable(currentHexTile);
//							elligibleTiles.Remove(currentHexTile.gameObject);
//							for (int j = 0; j < tilesInRange.Count; j++) {
//								if (elligibleTiles.Contains (tilesInRange [j].gameObject)) {
//									elligibleTiles.Remove (tilesInRange [j].gameObject);
//								}
//							}
//						}
//					}
//				}
//			}
		}
	}

	private void SetTileAsHabitable(HexTile hexTile){
		hexTile.isHabitable = true;
		habitableTiles.Add(hexTile);
		hexTile.GetComponent<SpriteRenderer>().color = Color.black;
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
}
