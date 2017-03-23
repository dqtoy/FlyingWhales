using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	public int month;
	public int week;
	public int year;

	void Awake(){
		Instance = this;
	}

	[ContextMenu("Start Progression")]
	public void StartProgression(){
		InvokeRepeating ("WeekEnded", 0f, 1f); 
	}

	public void WeekEnded(){
		this.week += 1;
		if (week > 4) {
			this.week = 1;
			this.month += 1;
			if (this.month > 12) {
				this.month = 1;
				this.year += 1;
			}
		}
		EventManager.TriggerEvent ("CitizenTurnActions");
		EventManager.TriggerEvent ("CityTurnActions");
	}

}
