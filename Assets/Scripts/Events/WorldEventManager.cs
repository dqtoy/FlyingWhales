using UnityEngine;
using System.Collections;

public class WorldEventManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		EventManager.Instance.onWeekEnd.AddListener (this.TickActions);
	}
	
	private void TickActions(){
		PlagueEventTrigger ();
	}

	private void PlagueEventTrigger(){
		if(GameManager.Instance.days == 13 || GameManager.Instance.days == 23){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 7){
				EventCreator.Instance.CreatePlagueEvent ();
			}
		}
	}
}
