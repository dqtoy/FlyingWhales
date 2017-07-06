using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldEventManager : MonoBehaviour {
	public static WorldEventManager Instance;

	internal EVENT_TYPES currentInterveneEvent;

	private List<GameEvent> currentWorldEvents;

	void Awake(){
		Instance = this;
	}

	void Start () {
		this.currentWorldEvents = new List<GameEvent>();
		ResetCurrentInterveneEvent ();
		EventManager.Instance.onWeekEnd.AddListener (this.TickActions);
	}
	
	private void TickActions(){
		PlagueEventTrigger ();
	}

	private void PlagueEventTrigger(){
		if((GameManager.Instance.days == 13 || GameManager.Instance.days == 23) && !HasEventOfType(EVENT_TYPES.PLAGUE) == null){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 15){
				EventCreator.Instance.CreatePlagueEvent ();
			}
		}
	}
	internal void ResetCurrentInterveneEvent(){
		this.currentInterveneEvent = EVENT_TYPES.NONE;
	}

	internal void BoonOfPowerTrigger(){
		int chance = UnityEngine.Random.Range (0, 2);
		if(chance == 0){
			List<HexTile> filteredHextile = new List<HexTile> ();
			for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
				HexTile hexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
				if(!hexTile.isBorder && !hexTile.isOccupied && hexTile.gameEventInTile == null && hexTile.elevationType != ELEVATION.MOUNTAIN && hexTile.elevationType != ELEVATION.WATER){
					filteredHextile.Add (hexTile);
				}
			}
			HexTile targetHextile = filteredHextile [UnityEngine.Random.Range (0, filteredHextile.Count)];
			EventCreator.Instance.CreateBoonOfPowerEvent (targetHextile);
		}
	}

	internal void FirstAndKeystoneTrigger(){
		int chance = UnityEngine.Random.Range (0, 2);
		if(chance == 0){
			List<HexTile> filteredHextile = new List<HexTile> ();
			for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
				HexTile hexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
				if(!hexTile.isBorder && !hexTile.isOccupied && hexTile.gameEventInTile == null){
					filteredHextile.Add (hexTile);
				}
			}
			HexTile targetHextile = filteredHextile [UnityEngine.Random.Range (0, filteredHextile.Count)];
			EventCreator.Instance.CreateFirstAndKeystoneEvent (targetHextile);
		}
	}
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
