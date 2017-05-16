using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InvasionPlan : GameEvent {

	private Kingdom _sourceKingdom;
	private Kingdom _targetKingdom;
	public List<Citizen> uncovered;
	internal War war;
	internal Militarization militarizationEvent = null;

	public Kingdom sourceKingdom {
		get { 
			return this._sourceKingdom; 
		}
	}

	public Kingdom targetKingdom {
		get { 
			return this._targetKingdom; 
		}
	}

	public InvasionPlan(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom, Kingdom targetKingdom, GameEvent gameEventTrigger, War war) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.INVASION_PLAN;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + ".";
		this.durationInWeeks = 0;
		this.remainingWeeks = this.durationInWeeks;
		this._sourceKingdom = sourceKingdom;
		this._targetKingdom = targetKingdom;
		this.war = war;
		this.uncovered = new List<Citizen>();

		if (gameEventTrigger is Assassination) {
			this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " after discovering that " + gameEventTrigger.startedBy.name
				+ " sent an assassin to kill " + (gameEventTrigger as Assassination).targetCitizen.name;
			startedBy.history.Add(new History(startMonth, startWeek, startYear, description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is BorderConflict){
			this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " in response to worsening Border Conflict.";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is DiplomaticCrisis){
			this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " in the aftermath of a recent Diplomatic Crisis.";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is Espionage){
			this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " after finding out that " + gameEventTrigger.startedBy.name + " spied on " + (gameEventTrigger as Espionage).targetKingdom.name + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is Raid){
			this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " after the raid of " + (gameEventTrigger as Raid).raidedCity.name + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is JoinWar){
			this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " at the request of " + (gameEventTrigger as JoinWar).startedByKingdom.name  + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else {
			this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} 

		this.sourceKingdom.cities[0].hexTile.AddEventOnTile(this);
		this.targetKingdom.cities[0].hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
		this.StartMilitarizationEvent();
	}

	internal override void PerformAction(){
		if (this.startedBy.isDead) {
			this.resolution = "Invasion plan was cancelled because " + this.startedBy.name + " died.";
			this.DoneEvent();
			return;
		}
		if (!this.startedBy.isKing) {
			this.resolution = "Invasion plan was cancelled because " + this.startedBy.name + " is no longer king.";
			this.DoneEvent();
			return;
		}
		int chanceToSendJoinWarRequest = Random.Range (0, 100);
		if (chanceToSendJoinWarRequest < 5) {
			//Send envoy for Join War
			List<Citizen> envoys = this.startedByKingdom.GetAllCitizensOfType(ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
			List<RelationshipKings> friends = this.startedBy.friends.Where(x => x.king.city.kingdom.IsKingdomAdjacentTo(this.targetKingdom)).ToList();
			if (envoys.Count > 0 && friends.Count > 0) {
				Envoy envoyToSend = (Envoy)envoys[Random.Range (0, envoys.Count)].assignedRole;
				Citizen citizenToPersuade = friends[Random.Range(0, friends.Count)].king;
				envoyToSend.inAction = true;
				JoinWar newJoinWarRequest = new JoinWar (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.startedBy, 
					citizenToPersuade, envoyToSend, this.targetKingdom);
			} else {
				Debug.Log ("Cannot send envoy because there are none or all of them are busy or there is no one to send envoy to");
			}
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
	}

	internal override void CancelEvent (){
		this.resolution = "Event was cancelled.";
		this.DoneEvent();
	}

	internal void MilitarizationDone(){
		//TODO: position generals appropriately
		this.resolution = "Invasion plan was successful and war is now declared between " + this._sourceKingdom.name + " and " + this._targetKingdom.name;
		this.war.DeclareWar (this._sourceKingdom);
		this.DoneEvent();
	}

	private void StartMilitarizationEvent(){
		Militarization currentMilitarization = null;
		if(IsThereMilitarizationActive(ref currentMilitarization)){
			currentMilitarization.remainingWeeks = currentMilitarization.durationInWeeks;
		}else{
			//New Militarization
			Militarization newMilitarization  = new Militarization(this.startWeek, this.startMonth, this.startYear, this.startedBy);
			this.militarizationEvent = newMilitarization;
			newMilitarization.invasionPlanThatTriggeredEvent = this;
		}
	}

	private bool IsThereMilitarizationActive(ref Militarization currentMilitarization){
		List<GameEvent> militarizationEvents = EventManager.Instance.GetEventsOfType (EVENT_TYPES.MILITARIZATION).Where(x => x.isActive).ToList();
		if(militarizationEvents != null){
			for (int i = 0; i < militarizationEvents.Count; i++) {
				if (((Militarization)militarizationEvents[i]).startedBy.id == _sourceKingdom.king.id) {
					currentMilitarization = (Militarization) militarizationEvents [i];
					return true;
				}
			}
			return false;
		}else{
			return false;
		}
	}
}
