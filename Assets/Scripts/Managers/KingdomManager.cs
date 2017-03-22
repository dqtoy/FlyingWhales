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

	public void GenerateInitialKingdoms(List<HexTile> habitableTiles){
		Debug.Log ("Generate Initial Kingdoms");
		//Get Starting City For Humans
		List<HexTile> cityForHumans = new List<HexTile>();
		List<HexTile> elligibleTilesForHumans = new List<HexTile>();
		for (int i = 0; i < habitableTiles.Count; i++) {
			List<HexTile> tilesInRange = habitableTiles[i].GetTilesInRange(14f).ToList();
			tilesInRange = tilesInRange.Where (t => t != null && Utilities.GetBaseResourceType(t.defaultResource) == BASE_RESOURCE_TYPE.STONE).ToList();
			if (tilesInRange.Count > 0) {
				elligibleTilesForHumans.Add(habitableTiles[i]);
			}
		}
		cityForHumans.Add (elligibleTilesForHumans [Random.Range (0, elligibleTilesForHumans.Count)]);
		GenerateNewKingdom (RACE.HUMANS, cityForHumans);

		//Get Statrting City For Elves
		List<HexTile> cityForElves = new List<HexTile>();
		List<HexTile> elligibleTilesForElves = new List<HexTile>();
		for (int i = 0; i < habitableTiles.Count; i++) {
			List<HexTile> tilesInRange = habitableTiles[i].GetTilesInRange(14f).ToList();
			tilesInRange = tilesInRange.Where (t => t != null && Utilities.GetBaseResourceType(t.defaultResource) == BASE_RESOURCE_TYPE.WOOD).ToList();
			if (tilesInRange.Count > 0) {
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

}
