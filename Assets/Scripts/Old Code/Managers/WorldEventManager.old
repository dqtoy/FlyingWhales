using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WorldEventManager : MonoBehaviour {
	public static WorldEventManager Instance;

    [SerializeField] private List<InitialWorldEvent> initialWorldEventSetup;

	//public int altarOfBlessingQuantity;

	internal EVENT_TYPES currentInterveneEvent;

	private List<GameEvent> currentWorldEvents;

    #region Monobehaviours
    void Awake() {
        Instance = this;
        this.currentWorldEvents = new List<GameEvent>();
    }

    void Start() {
        ResetCurrentInterveneEvent();
//        Messenger.AddListener(Signals.DAY_END, this.TickActions);
    }
    #endregion

    //internal void TriggerInitialWorldEvents() {
    //    for (int i = 0; i < initialWorldEventSetup.Count; i++) {
    //        EVENT_TYPES eventToCreate = initialWorldEventSetup[i].eventType;
    //        int numOfTimesToCreate = initialWorldEventSetup[i].quantity;
    //        switch (eventToCreate) {
    //            case EVENT_TYPES.BOON_OF_POWER:
    //                BoonOfPowerTrigger(numOfTimesToCreate);
    //                break;
    //            case EVENT_TYPES.ALTAR_OF_BLESSING:
    //                AltarOfBlessingTrigger(numOfTimesToCreate);
    //                break;
    //            case EVENT_TYPES.FIRST_AND_KEYSTONE:
    //                FirstAndKeystoneTrigger(numOfTimesToCreate);
    //                break;
    //            //case EVENT_TYPES.DEVELOP_WEAPONS:
    //            //    DevelopWeaponsTrigger(numOfTimesToCreate);
    //            //    break;
    //            case EVENT_TYPES.ANCIENT_RUIN:
    //                CreateAncientRuins(numOfTimesToCreate);
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //}

//    private void TickActions(){
//		PlagueEventTrigger ();
//	}
//
//	private void PlagueEventTrigger(){
//		if((GameManager.Instance.days == 13 || GameManager.Instance.days == 23) && !HasEventOfType(EVENT_TYPES.PLAGUE)){
//			int chance = UnityEngine.Random.Range (0, 100);
//			if(chance < 1){
//				EventCreator.Instance.CreatePlagueEvent ();
//			}
//		}
//	}
	internal void ResetCurrentInterveneEvent(){
		this.currentInterveneEvent = EVENT_TYPES.NONE;
	}

//	internal void BoonOfPowerTrigger(int quantity = 1){
//		int chance = UnityEngine.Random.Range (0, 2);
//		if(chance == 0){
//			List<HexTile> filteredHextile = new List<HexTile> ();
//			for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
//				HexTile hexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
//				if(!hexTile.isBorder && !hexTile.isOccupied && hexTile.gameEventInTile == null && hexTile.elevationType != ELEVATION.MOUNTAIN && hexTile.elevationType != ELEVATION.WATER && hexTile.specialResource == RESOURCE.NONE){
//                    //List<HexTile> checkForHabitableTilesInRange = hexTile.GetTilesInRange (3);
//                    //if (checkForHabitableTilesInRange.FirstOrDefault(x => x.isHabitable) == null) {
//                    //	filteredHextile.Add (hexTile);
//                    //}
//                    List<HexTile> tilesInRange = hexTile.GetTilesInRange(3);
//                    if (!tilesInRange.Where(x => x.gameEventInTile != null || x.isHabitable || x.isOccupied).Any()) {
//                        filteredHextile.Add(hexTile);
//                    }
//                }
//			}

//            if (filteredHextile.Count > 0) {
//                for (int i = 0; i < quantity; i++) {
//                    int index = UnityEngine.Random.Range(0, filteredHextile.Count);
//                    HexTile targetHextile = filteredHextile[index];
//                    filteredHextile.RemoveAt(index);
////                    EventCreator.Instance.CreateBoonOfPowerEvent(targetHextile);
//                }
//            }			
//		}
//	}

//	internal void FirstAndKeystoneTrigger(int quantity = 1) {
//		int chance = UnityEngine.Random.Range (0, 2);
//		if(chance == 0){
//			List<HexTile> filteredHextile = new List<HexTile> ();
//			for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
//				HexTile hexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
//				if(!hexTile.isBorder && !hexTile.isOccupied && hexTile.gameEventInTile == null){
//                    //List<HexTile> checkForHabitableTilesInRange = hexTile.GetTilesInRange (3);
//                    //if (checkForHabitableTilesInRange.FirstOrDefault(x => x.isHabitable) == null) {
//                    //	filteredHextile.Add (hexTile);
//                    //}
//                    List<HexTile> tilesInRange = hexTile.GetTilesInRange(3);
//                    if (!tilesInRange.Where(x => x.gameEventInTile != null || x.isHabitable || x.isOccupied).Any()) {
//                        filteredHextile.Add(hexTile);
//                    }
//                }
//			}

//            if(filteredHextile.Count > 0) {
//                for (int i = 0; i < quantity; i++) {
//                    int index = UnityEngine.Random.Range(0, filteredHextile.Count);
//                    HexTile targetHextile = filteredHextile[index];
//                    filteredHextile.RemoveAt(index);
////                    EventCreator.Instance.CreateFirstAndKeystoneEvent(targetHextile);
//                }
//            }
//		}
//	}

//	internal void AltarOfBlessingTrigger(int quantity = 1) {
//		List<HexTile> filteredHextile = new List<HexTile> ();
//		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
//			HexTile hexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
//			if(!hexTile.isBorder && !hexTile.isOccupied && hexTile.gameEventInTile == null && hexTile.elevationType != ELEVATION.MOUNTAIN && hexTile.elevationType != ELEVATION.WATER && hexTile.specialResource == RESOURCE.NONE){
//                List<HexTile> tilesInRange = hexTile.GetTilesInRange(3);
//                if (!tilesInRange.Where(x => x.gameEventInTile != null || x.isHabitable || x.isOccupied).Any()) {
//                    filteredHextile.Add(hexTile);
//                }
//            }
//		}
//		if(filteredHextile.Count > 0){
//			for (int i = 0; i < quantity; i++) {
//				int index = UnityEngine.Random.Range (0, filteredHextile.Count);
//				HexTile targetHextile = filteredHextile [index];
//				filteredHextile.RemoveAt (index);
////				EventCreator.Instance.CreateAltarOfBlessingEvent (targetHextile);
//			}
//		}
//	}

    //internal void DevelopWeaponsTrigger(int quantity = 1, HexTile[] overrideLocations = null) {
    //    List<HexTile> filteredHextile = new List<HexTile>();
    //    if (overrideLocations != null) {
    //        filteredHextile = overrideLocations.ToList();
    //    } else {
    //        for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
    //            HexTile hexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
    //            if (!hexTile.isBorder && !hexTile.isOccupied && hexTile.gameEventInTile == null && hexTile.elevationType != ELEVATION.MOUNTAIN && hexTile.elevationType != ELEVATION.WATER && hexTile.specialResource == RESOURCE.NONE) {
    //                List<HexTile> checkForHabitableTilesInRange = hexTile.GetTilesInRange(3);
    //                if (checkForHabitableTilesInRange.FirstOrDefault(x => x.isHabitable) == null) {
    //                    filteredHextile.Add(hexTile);
    //                }
    //            }
    //        }
    //    }
        
    //    if (filteredHextile.Count > 0) {
    //        for (int i = 0; i < quantity; i++) {
    //            int index = UnityEngine.Random.Range(0, filteredHextile.Count);
    //            HexTile targetHextile = filteredHextile[index];
    //            filteredHextile.RemoveAt(index);
    //            EventCreator.Instance.CreateDevelopWeaponsEvent(targetHextile);
    //        }
    //    }
    //}

//    internal void CreateAncientRuins(int quantity = 1) {
//        List<HexTile> filteredHextile = new List<HexTile>();
//        for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
//            HexTile hexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
//            if (!hexTile.isBorder && !hexTile.isOccupied && hexTile.gameEventInTile == null && hexTile.elevationType != ELEVATION.MOUNTAIN && hexTile.elevationType != ELEVATION.WATER && hexTile.specialResource == RESOURCE.NONE) {
//                List<HexTile> tilesInRange = hexTile.GetTilesInRange(3);
//                if (!tilesInRange.Where(x => x.gameEventInTile != null || x.isHabitable || x.isOccupied).Any()) {
//                    filteredHextile.Add(hexTile);
//                }
//            }
//        }
//        if (filteredHextile.Count > 0) {
//            for (int i = 0; i < quantity; i++) {
//                int index = UnityEngine.Random.Range(0, filteredHextile.Count);
//                HexTile targetHextile = filteredHextile[index];
//                filteredHextile.RemoveAt(index);
////                EventCreator.Instance.CreateAncientRuinEvent(targetHextile);
//            }
//        }
//    }

	internal void AddWorldEvent(GameEvent gameEvent){
		this.currentWorldEvents.Add(gameEvent);
	}
	internal void RemoveWorldEvent(GameEvent gameEvent){
		this.currentWorldEvents.Remove(gameEvent);
	}
	internal bool HasEventOfType(EVENT_TYPES eventType){
		for (int i = 0; i < this.currentWorldEvents.Count; i++) {
			if(this.currentWorldEvents[i].eventType == eventType){
				return true;
			}
		}
		return false;
	}
	internal GameEvent SearchEventOfType(EVENT_TYPES eventType){
		for (int i = 0; i < this.currentWorldEvents.Count; i++) {
			if(this.currentWorldEvents[i].eventType == eventType){
				return this.currentWorldEvents[i];
			}
		}
		return null;
	}
 }
