using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SchedulingManager : MonoBehaviour {
	public static SchedulingManager Instance;

	private Dictionary<GameDate, List<Action>> schedules = new Dictionary<GameDate, List<Action>> ();
	private GameDate checkGameDate;

	void Awake(){
		Instance = this;
	}
	public void StartScheduleCalls(){
		this.checkGameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, GameManager.Instance.tick);
		Messenger.AddListener (Signals.DAY_ENDED, CheckSchedule);
	}
	private void CheckSchedule(){
		this.checkGameDate.month = GameManager.Instance.month;
		this.checkGameDate.day = GameManager.Instance.days;
		this.checkGameDate.year = GameManager.Instance.year;
        this.checkGameDate.tick = GameManager.Instance.tick;
		if(this.schedules.ContainsKey(this.checkGameDate)){
			DoAsScheduled (this.schedules [this.checkGameDate]);
			RemoveEntry (this.checkGameDate);
		}
	}

	internal void AddEntry(int month, int day, int year, int hour, Action act){
		GameDate gameDate = new GameDate (month, day, year, hour);
		if(this.schedules.ContainsKey(gameDate)){
			this.schedules [gameDate].Add (act);
		}else{
			List<Action> acts = new List<Action> ();
			acts.Add (act);
			this.schedules.Add (gameDate, acts);
		}
	}
	internal void AddEntry(GameDate gameDate, Action act){
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
	internal void RemoveSpecificEntry(int month, int day, int year, int hour, Action act){
		GameDate gameDate = new GameDate (month, day, year, hour);
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
    internal void RemoveSpecificEntry(GameDate date, Action act) {
        if (this.schedules.ContainsKey(date)) {
            List<Action> acts = this.schedules[date];
            for (int i = 0; i < acts.Count; i++) {
                if (acts[i].Target == act.Target) {
                    this.schedules[date].RemoveAt(i);
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
