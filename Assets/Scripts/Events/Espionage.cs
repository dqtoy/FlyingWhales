using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Espionage : GameEvent {
	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;
	public Citizen spy;
	public Espionage(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.description = startedBy.name + " is having an espionage event.";
		this.durationInWeeks = 2;
		this.remainingWeeks = this.durationInWeeks;
		this.sourceKingdom = startedBy.city.kingdom;
		this.spy = GetSpy (this.sourceKingdom);
		this.targetKingdom = GetTargetKingdom ();
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}
	internal override void PerformAction(){
		this.durationInWeeks -= 1;
		if(this.durationInWeeks <= 0){
			this.durationInWeeks = 0;
			ActualEspionage ();
			DoneEvent ();
		}
	}
	internal override void DoneEvent(){
		if(this.spy != null){
			((Spy)this.spy.assignedRole).inAction = false;
		}
		this.spy = null;
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		EventManager.Instance.allEvents [EVENT_TYPES.ESPIONAGE].Remove (this);
	}
	private Citizen GetSpy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> spies = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.SPY) {
							if (!((Spy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
								spies.Add (kingdom.cities [i].citizens [j]);
							}
						}
					}
				}
			}
		}

		if(spies.Count > 0){
			int random = UnityEngine.Random.Range (0, spies.Count);
			((Spy)spies [random].assignedRole).inAction = true;
			return spies [random];
		}else{
			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}
	private Kingdom GetTargetKingdom(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 65){
			return this.sourceKingdom;
		}else{
			List<Kingdom> adjacentKingdoms = new List<Kingdom> ();
			for(int i = 0; i < this.sourceKingdom.relationshipsWithOtherKingdoms.Count; i++){
				if(this.sourceKingdom.relationshipsWithOtherKingdoms[i].isAdjacent){
					adjacentKingdoms.Add (this.sourceKingdom.relationshipsWithOtherKingdoms [i].objectInRelationship);
				}
			}

			if(adjacentKingdoms.Count > 0){
				return adjacentKingdoms [UnityEngine.Random.Range (0, adjacentKingdoms.Count)];
			}else{
				return null;
			}
		}
	}
	private void ActualEspionage(){
		if(this.spy == null){
			Debug.Log ("CAN'T ASSASSINATE NO SPIES AVAILABLE");
			return;
		}
		if(this.spy.isDead){
			Debug.Log ("CAN'T ASSASSINATE, SPY IS DEAD!");
			return;
		}

		GameEvent chosenEvent = GetEventToExpose();
		if(chosenEvent == null){
			return;
		}

		int chance = UnityEngine.Random.Range (0, 100);
		int value = 75;
		if(this.spy.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value += 10;
		}
		if(chance < value){
			if(this.targetKingdom.king.id == this.sourceKingdom.king.id){
				this.targetKingdom.king.InformedAboutHiddenEvent (chosenEvent, this.sourceKingdom.king);
			}else{
				RelationshipKings relationship = this.sourceKingdom.king.SearchRelationshipByID (this.targetKingdom.king.id);
				if(chosenEvent is Assassination){

				}
			}

		}

	}
	private void AssassinationExposed(GameEvent chosenEvent){
		int chance = UnityEngine.Random.Range (0, 100);
		Citizen target = ((Assassination)chosenEvent).targetCitizen;
		Citizen assassin = ((Assassination)chosenEvent).assassinKingdom.king;

		if(target.isKing){
			RelationshipKings relationship = this.sourceKingdom.king.SearchRelationshipByID (target.id);
			RelationshipKings relationshipToCreator = this.sourceKingdom.king.SearchRelationshipByID (assassin.id);
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.WARM || relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND || relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				if(relationship.lordRelationship == RELATIONSHIP_STATUS.WARM){
					int value = 25;
					if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.WARM){
						value -= 10;
					}else if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
						value -= 20;
					}else if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.ALLY){
						value -= 40;
					}
					if(chance < value){
						target.InformedAboutHiddenEvent (chosenEvent);
					}

				}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
					int value = 50;
					if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.WARM){
						value -= 10;
					}else if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
						value -= 20;
					}else if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.ALLY){
						value -= 40;
					}
					if(chance < value){
						target.InformedAboutHiddenEvent (chosenEvent);
					}
					relationshipToCreator.AdjustLikeness (-10);
				}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
					int value = 80;
					if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.WARM){
						value -= 10;
					}else if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
						value -= 20;
					}else if(relationshipToCreator.lordRelationship == RELATIONSHIP_STATUS.ALLY){
						value -= 40;
					}
					if(chance < value){
						target.InformedAboutHiddenEvent (chosenEvent);
					}
					relationshipToCreator.AdjustLikeness (-15);

				}

				
			}
		}
	}
	private GameEvent GetEventToExpose(){
		if(this.targetKingdom == null){
			return null;
		}
		List<GameEvent> invasionPlanEvents = EventManager.Instance.GetEventsOfType(EVENT_TYPES.INVASION_PLAN);
		List<GameEvent> joinWarEvents = EventManager.Instance.GetEventsOfType(EVENT_TYPES.JOIN_WAR_REQUEST);
		List<GameEvent> militarizationEvents = EventManager.Instance.GetEventsOfType(EVENT_TYPES.MILITARIZATION);
		List<GameEvent> assassinationEvents = EventManager.Instance.GetEventsOfType(EVENT_TYPES.ASSASSINATION);
		List<GameEvent> rebellionPlotEvents = EventManager.Instance.GetEventsOfType(EVENT_TYPES.REBELLION_PLOT);
		List<GameEvent> powerGrabEvents = EventManager.Instance.GetEventsOfType(EVENT_TYPES.POWER_GRAB);

		List<GameEvent> allEventsAffectionTarget = new List<GameEvent> ();

		for(int i = 0; i < invasionPlanEvents.Count; i++){
			
		}
		for(int i = 0; i < joinWarEvents.Count; i++){

		}
		for(int i = 0; i < militarizationEvents.Count; i++){

		}
		for(int i = 0; i < assassinationEvents.Count; i++){
			if(((Assassination)assassinationEvents[i]).targetCitizen.city.kingdom.id == this.targetKingdom.id && ((Assassination)assassinationEvents[i]).assassinKingdom.id != this.sourceKingdom.id){
				allEventsAffectionTarget.Add (assassinationEvents [i]);
			}
		}
		for(int i = 0; i < rebellionPlotEvents.Count; i++){

		}
		for(int i = 0; i < powerGrabEvents.Count; i++){

		}

		if(allEventsAffectionTarget.Count > 0){
			return allEventsAffectionTarget [UnityEngine.Random.Range (0, allEventsAffectionTarget.Count)];
		}else{
			Debug.Log ("NO HIDDEN EVENT TO EXPOSE HERE!");
			return null;
		}

	}
}
