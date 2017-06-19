using UnityEngine;
using System.Collections;

public class Reinforcement : GameEvent {

	internal Reinforcer reinforcer;
	internal City targetCity;
	internal City sourceCity;

	public Reinforcement(int startWeek, int startMonth, int startYear, Citizen startedBy, Reinforcer reinforcer, City targetCity, City sourceCity) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REINFORCEMENT;
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.reinforcer = reinforcer;
		this.targetCity = targetCity;
		this.sourceCity = sourceCity;
		this.SendReinforcement ();
		Debug.LogError (reinforcer.citizen.name + " of " + reinforcer.citizen.city.kingdom.name + " will reinforce " + targetCity.name);

	}

	#region Overrides
	internal override void DoneCitizenAction (Citizen citizen){
        base.DoneCitizenAction(citizen);
		if(this.reinforcer != null){
			if(citizen.id == this.reinforcer.citizen.id){
				//Reinforcement Function
				this.ArrivedReinforcement();
				this.DoneEvent ();
			}
		}
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByGeneral(General general){
		this.reinforcer.citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent ();
		//		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	private void SendReinforcement(){
		int amount = Mathf.CeilToInt((float)this.reinforcer.citizen.city.hp * 0.3f);
		int missingHP = this.targetCity.maxHP - this.targetCity.hp;
		if(amount > missingHP){
			amount = missingHP;
		}
		this.sourceCity.AdjustHP (-amount);
		this.reinforcer.reinforcementValue += amount;
	}

	private void ArrivedReinforcement(){
		if((this.reinforcer.isRebel && this.targetCity.rebellion == null) || (!this.reinforcer.isRebel && this.targetCity.rebellion != null)){
			return;
		}
		if(this.targetCity.isDead || this.targetCity == null){
			return;
		}
		this.targetCity.AdjustHP (this.reinforcer.reinforcementValue);
		this.reinforcer.reinforcementValue = 0;
	}
}
