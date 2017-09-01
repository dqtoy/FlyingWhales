using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SchedulingManager : MonoBehaviour {
	public static SchedulingManager Instance;

	private Dictionary<GameDate, List<Action>> schedules = new Dictionary<GameDate, List<Action>> ();

	void Awake(){
		Instance = this;
	}
	void Start(){
		Messenger.AddListener ("OnDayEnd", CheckSchedule);
	}
	private void CheckSchedule(){
		GameDate gameDate;
		gameDate.month = GameManager.Instance.month;
		gameDate.day = GameManager.Instance.days;
		gameDate.year = GameManager.Instance.year;
		if(this.schedules.ContainsKey(gameDate)){
			DoAsScheduled (this.schedules [gameDate]);
			RemoveEntry (gameDate);
		}
	}

	internal void AddEntry(int month, int day, int year, Action act){
		GameDate gameDate;
		gameDate.month = month;
		gameDate.day = day;
		gameDate.year = year;
		if(this.schedules.ContainsKey(gameDate)){
			this.schedules [gameDate].Add (act);
		}else{
			List<Action> acts = new List<Action> ();
			acts.Add (act);
			this.schedules.Add (gameDate, acts);
		}
	}
	internal void RemoveEntry(GameDate gameDate){
		this.schedules.Remove (gameDate);
	}
	internal void RemoveSpecificEntry(int month, int day, int year, Action act){
		GameDate gameDate;
		gameDate.month = month;
		gameDate.day = day;
		gameDate.year = year;
		if(this.schedules.ContainsKey(gameDate)){
			List<Action> acts = this.schedules[gameDate];
			for (int i = 0; i < acts.Count; i++) {
				if(acts[i].Target == act.Target){
					this.schedules[gameDate].RemoveAt(i);
					break;
				}
			}
		}
	}
	private void DoAsScheduled(List<Action> acts){
		for (int i = 0; i < acts.Count; i++) {
			if(acts[i].Target != null){
				acts [i] ();
			}
		}
	}
}
