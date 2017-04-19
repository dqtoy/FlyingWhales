using UnityEngine;
using System.Collections;

public class Envoy : Role {

	public int successfulMissions;
	public int unsuccessfulMissions;
	public GameEvent currentEvent;
	public int eventDuration;
	public bool inAction;

	public Envoy(Citizen citizen): base(citizen){
		this.successfulMissions = 0;
		this.unsuccessfulMissions = 0;
		this.currentEvent = null;
		this.eventDuration = 0;
		this.inAction = false;
	}

	public void WeeklyAction(){
		this.eventDuration -= 1;
		if(this.eventDuration <= 0){
			EventManager.Instance.onWeekEnd.RemoveListener (WeeklyAction);
			this.eventDuration = 0;
			this.inAction = false;
			currentEvent.DoneCitizenAction (this);
			currentEvent = null;
		}
	}

	internal override void OnDeath(){
		EventManager.Instance.onWeekEnd.RemoveListener (WeeklyAction);
		if(currentEvent != null){
			currentEvent.DoneCitizenAction (this);
		}
	}
}
