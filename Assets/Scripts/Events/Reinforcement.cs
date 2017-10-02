using UnityEngine;
using System.Collections;

public class Reinforcement : GameEvent {

	internal Reinforcer reinforcer;
	internal City targetCity;
	internal City sourceCity;
	internal int reinforcementAmount;
	internal Wars war;

	public Reinforcement(int startWeek, int startMonth, int startYear, Citizen startedBy, Reinforcer reinforcer, City targetCity, City sourceCity, int reinforcementAmount, Wars war = null) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REINFORCEMENT;
		this.name = "Reinforcement";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.reinforcer = reinforcer;
		this.targetCity = targetCity;
		this.sourceCity = sourceCity;
		this.reinforcementAmount = reinforcementAmount;
		this.war = war;
		this.SendReinforcement ();
		Debug.Log (reinforcer.citizen.name + " of " + reinforcer.citizen.city.kingdom.name + " will reinforce " + targetCity.name);

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
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent ();
		//		Messenger.RemoveListener("OnDayEnd", this.PerformAction);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	private void SendReinforcement(){
		if(this.reinforcementAmount == -1){
			this.reinforcementAmount = Mathf.CeilToInt(this.reinforcer.citizen.city.weapons * 0.3f);
		}
		if(this.sourceCity.weapons < this.reinforcementAmount){
			this.reinforcementAmount = this.sourceCity.weapons;
		}
		this.sourceCity.AdjustWeapons (-this.reinforcementAmount);
		this.reinforcer.reinforcementValue += this.reinforcementAmount;
		if(this.war != null){
			this.war.AdjustMobilizedReinforcementsCount (1);
		}
	}

	private void ArrivedReinforcement(){
		if((this.reinforcer.isRebel && this.targetCity.rebellion == null) || (!this.reinforcer.isRebel && this.targetCity.rebellion != null)){
			return;
		}
		if(this.targetCity.isDead || this.targetCity == null){
			return;
		}
		this.targetCity.AdjustWeapons (this.reinforcer.reinforcementValue);
		this.reinforcer.reinforcementValue = 0;
		if(this.war != null){
			this.war.AdjustMobilizedReinforcementsCount (-1);
		}
	}
}
