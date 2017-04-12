using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

			HexTile[] adjacentTiles = currentHexTile.AllNeighbours.ToArray();
			HexTile[] tilesInRange = currentHexTile.GetTilesInRange(25f);

			HexTile[] foodTiles = adjacentTiles.Where(x => (x.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType (x.defaultResource) == BASE_RESOURCE_TYPE.FOOD) || 
				(x.specialResource != RESOURCE.NONE && Utilities.GetBaseResourceType (x.specialResource) == BASE_RESOURCE_TYPE.FOOD)).ToArray();
			
			HexTile[] basicResourceTiles = adjacentTiles.Where(x => (x.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType(x.defaultResource) == BASE_RESOURCE_TYPE.STONE) ||
				(x.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType(x.defaultResource) == BASE_RESOURCE_TYPE.WOOD) || 
				(x.specialResource != RESOURCE.NONE && Utilities.GetBaseResourceType(x.specialResource) == BASE_RESOURCE_TYPE.STONE) ||
				(x.specialResource != RESOURCE.NONE && Utilities.GetBaseResourceType(x.specialResource) == BASE_RESOURCE_TYPE.WOOD)).ToArray();


			List<HexTile> specialTiles = new List<HexTile>();
			List<HexTile> nearCityTiles = new List<HexTile>();
			for (int j = 0; j < tilesInRange.Length; j++) {
				if (tilesInRange [j] != null) {
					if (tilesInRange [j].specialResource != RESOURCE.NONE) {
						specialTiles.Add (tilesInRange [j]);
					}

					if (tilesInRange [j].isHabitable) {
						nearCityTiles.Add (tilesInRange [j]);
					}
				}
			}


			if (foodTiles.Length >= 2 && basicResourceTiles.Length >= 1 && specialTiles.Count >= 3 && nearCityTiles.Count <= 0) {
				SetTileAsHabitable(currentHexTile);
				elligibleTiles.Remove(currentHexTile.gameObject);
				for (int j = 0; j < tilesInRange.Length; j++) {
					if (tilesInRange [j] != null) {
						if (elligibleTiles.Contains (tilesInRange [j].gameObject)) {
							elligibleTiles.Remove (tilesInRange [j].gameObject);
						}
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
		
}
