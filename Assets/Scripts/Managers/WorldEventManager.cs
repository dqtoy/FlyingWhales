using UnityEngine;
using System.Collections;

public class WorldEventManager : MonoBehaviour {
	public static WorldEventManager Instance;

	internal EVENT_TYPES currentInterveneEvent;

	internal Plague currentPlague;

	void Awake(){
		Instance = this;
	}

	void Start () {
		this.currentPlague = null;
		ResetCurrentInterveneEvent ();
		EventManager.Instance.onWeekEnd.AddListener (this.TickActions);
	}
	
	private void TickActions(){
		PlagueEventTrigger ();
	}

	private void PlagueEventTrigger(){
		if((GameManager.Instance.days == 13 || GameManager.Instance.days == 23) && this.currentPlague == null){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 30){
				EventCreator.Instance.CreatePlagueEvent ();
			}
		}
	}
	internal void ResetCurrentInterveneEvent(){
		this.currentInterveneEvent = EVENT_TYPES.NONE;
	}
}
