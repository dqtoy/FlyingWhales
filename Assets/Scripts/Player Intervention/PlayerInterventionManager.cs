using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInterventionManager : MonoBehaviour {
	public static PlayerInterventionManager Instance;

	void Awake () {
		Instance = this;
	}

	//internal void SpawnMonsterLair(Region region){
	//	List<HexTile> unoccupiedTiles = new List<HexTile> ();
	//	for (int i = 0; i < region.tilesInRegion.Count; i++) {
	//		if(!region.tilesInRegion[i].isOccupied){
	//			unoccupiedTiles.Add (region.tilesInRegion [i]);
	//		}
	//	}
	//	if(unoccupiedTiles.Count > 0){
	//		HexTile chosenTile = unoccupiedTiles [UnityEngine.Random.Range (0, unoccupiedTiles.Count)];
	//		LAIR lairType = MonsterManager.Instance.GetRandomLairType ();
	//		Lair lair = MonsterManager.Instance.CreateLair (lairType, chosenTile);
	//		if(lair != null){
	//			if(chosenTile.region.occupant != null){
	//				Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerIntervention", "MonsterLair", "spawn_with_city");
	//				//newLog.AddToFillers (lair, lair.name, LOG_IDENTIFIER.LAIR_NAME);
	//				newLog.AddToFillers (chosenTile.region.occupant, chosenTile.region.occupant.name, LOG_IDENTIFIER.LANDMARK_1);
	//				UIManager.Instance.ShowNotification (newLog);
	//			}else{
	//				Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerIntervention", "MonsterLair", "spawn_without_city");
	//				//newLog.AddToFillers (lair, lair.name, LOG_IDENTIFIER.LAIR_NAME);
	//				UIManager.Instance.ShowNotification (newLog);
	//			}
	//		}
	//	}
	//}
}
