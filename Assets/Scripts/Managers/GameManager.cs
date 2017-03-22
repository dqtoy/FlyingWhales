using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	void Awake(){
		Instance = this;
	}

	[ContextMenu("Start Progression")]
	public void StartProgression(){
		InvokeRepeating ("WeekEnded", 0f, 1f); 
	}

	public void WeekEnded(){
		EventManager.TriggerEvent ("ProduceResources");
	}

}
