using UnityEngine;
using System.Collections;

public class EventCreator: MonoBehaviour {
	public static EventCreator Instance;

	void Awake(){
		Instance = this;
	}
	internal void CreateExpansionEvent(Kingdom kingdom){
		Citizen expander = kingdom.cities [0].CreateAgent (ROLE.EXPANDER);
		HexTile hexTileToExpandTo = CityGenerator.Instance.GetNearestHabitableTile (kingdom.cities [0]);
		if (hexTileToExpandTo != null && expander != null) {
			Expansion newExpansionEvent = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, expander, hexTileToExpandTo);
			expander.assignedRole.Initialize (newExpansionEvent);
		}
	}

	internal void CreateRaidEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		Citizen raider = firstKingdom.cities [0].CreateAgent (ROLE.RAIDER);
		City city = null;
		if(secondKingdom.cities.Count > 0){
			city = secondKingdom.cities [UnityEngine.Random.Range (0, secondKingdom.cities.Count)];
		}
		if(city != null && raider != null){
			Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, firstKingdom.king, city);
			raider.assignedRole.Initialize (raid);
		}
	}
}
