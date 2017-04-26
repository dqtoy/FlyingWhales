using UnityEngine;
using System.Collections;

public class Envoy : Role {

	public int successfulMissions;
	public int unsuccessfulMissions;
	public GameEvent currentEvent;
	public int eventDuration;
	public bool inAction;

	protected delegate void DoAction();
	protected DoAction onDoAction;

	private RelationshipKingdom warExhaustiontarget;

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

	internal void StartIncreaseWarExhaustionTask(RelationshipKingdom targetKingdom){
		this.eventDuration = 2;
		this.warExhaustiontarget = targetKingdom;
		this.inAction = true;
		this.onDoAction += IncreaseWarExhaustion;
		EventManager.Instance.onWeekEnd.AddListener(WaitForAction);
	}

	protected void IncreaseWarExhaustion(){
		if (this.citizen.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
			this.warExhaustiontarget.kingdomWar.exhaustion += 20;
		} else {
			this.warExhaustiontarget.kingdomWar.exhaustion += 15;
		}
		this.inAction = false;
		this.onDoAction -= IncreaseWarExhaustion;
		EventManager.Instance.onWeekEnd.RemoveListener(WaitForAction);
	}

	protected void WaitForAction(){
		if (this.eventDuration > 0) {
			this.eventDuration -= 1;
		} else {
			this.onDoAction();
		}
	}
}
