using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InvasionPlan : GameEvent {

	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;

	public InvasionPlan(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom, Kingdom targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.INVASION_PLAN;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + ".";
		this.durationInWeeks = 0;
		this.remainingWeeks = this.durationInWeeks;
		this.sourceKingdom = sourceKingdom;
		this.targetKingdom = targetKingdom;

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
				JoinWar newJoinWarRequest = new JoinWar (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this.startedBy, 
					citizenToPersuade, envoyToSend, this.targetKingdom);
			} else {
				Debug.Log ("Cannot send envoy because there are none or all of them are busy or there is no one to send envoy to");
			}
		}
	}

	internal override void DoneCitizenAction(Citizen citizen){
		
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
		this.resolution = "Invasion plan was successful and war is now declared between " + this.sourceKingdom.name + " and " + this.targetKingdom.name;
		KingdomManager.Instance.DeclareWarBetweenKingdoms(this.sourceKingdom, this.targetKingdom);
		this.DoneEvent();
	}

	private void StartMilitarizationEvent(){
		Militarization currentMilitarization = null;
		if(IsThereMilitarizationActive(ref currentMilitarization)){
			currentMilitarization.remainingWeeks = currentMilitarization.durationInWeeks;
		}else{
			//New Militarization
			Militarization newMilitarization  = new Militarization(this.startWeek, this.startMonth, this.startYear, this.startedBy);
			newMilitarization.invasionPlanThatTriggeredEvent = this;
		}
	}

	private bool IsThereMilitarizationActive(ref Militarization currentMilitarization){
		List<GameEvent> militarizationEvents = EventManager.Instance.GetEventsOfType (EVENT_TYPES.MILITARIZATION);
		if(militarizationEvents != null){
			for (int i = 0; i < militarizationEvents.Count; i++) {
				if (((Militarization)militarizationEvents[i]).startedBy.id == sourceKingdom.king.id) {
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
