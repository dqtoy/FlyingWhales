using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Espionage : GameEvent {
	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;
	public Citizen spy;
	public GameEvent chosenEvent;
	public List<GameEvent> allEventsAffectingTarget;
	public bool hasFound;
	public int successRate;

	public Espionage(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen spy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ESPIONAGE;
		this.description = startedBy.name + " is having an espionage event.";
		this.durationInWeeks = 2;
		this.remainingWeeks = this.durationInWeeks;
		this.sourceKingdom = startedBy.city.kingdom;
		this.spy = spy;
		this.allEventsAffectingTarget = new List<GameEvent> ();
		this.targetKingdom = GetTargetKingdom ();
		this.chosenEvent = this.GetEventToExpose(true);
		this.hasFound = false;
		this.successRate = 75;
		if(this.spy.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			this.successRate += 10;
		}
		if (this.targetKingdom != null) {
			this.targetKingdom.cities[0].hexTile.AddEventOnTile(this);
		}
		EventManager.Instance.AddEventToDictionary(this);
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}
	internal override void PerformAction(){
		this.remainingWeeks -= 1;
		if(this.remainingWeeks <= 0){
			this.remainingWeeks = 0;
			ActualEspionage ();
			DoneEvent ();
		}
	}
	internal override void DoneEvent(){
		if(this.spy != null){
			((Spy)this.spy.assignedRole).inAction = false;
		}
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		this.endMonth = GameManager.Instance.month;
		this.endWeek = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;
		if(this.chosenEvent != null && this.hasFound){
			this.resolution = ((MONTH)this.endMonth).ToString() + " " + this.endWeek + ", " + this.endYear + ". " + this.spy.name + " discovered a Hidden Event: ";
		}
//		EventManager.Instance.allEvents [EVENT_TYPES.ESPIONAGE].Remove (this);
	}
	private Citizen GetSpy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> spies = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.SPY) {
							if (kingdom.cities [i].citizens [j].assignedRole is Spy) {
								if (!((Spy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
									spies.Add (kingdom.cities [i].citizens [j]);
								}
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
				return this.sourceKingdom;
			}
		}
	}
	private void ActualEspionage(){
		if(this.spy == null){
			Debug.Log ("CAN'T ESPIONAGE NO SPIES AVAILABLE");
			return;
		}
		if(this.spy.isDead){
			Debug.Log ("CAN'T ESPIONAGE, SPY IS DEAD!");
			return;
		}

		if(chosenEvent == null){
			return;
		}

		int chance = UnityEngine.Random.Range (0, 100);

		if(chance < this.successRate){
			hasFound = true;
			UncoverHiddenEvent (this.chosenEvent, this.sourceKingdom.king);
			if(this.targetKingdom.king.id == this.sourceKingdom.king.id){
				this.targetKingdom.king.InformedAboutHiddenEvent (chosenEvent, this.spy);
			}else{
				Kingdom targetKingdomSpecific = null;
				Kingdom sourceKingdomSpecific = null;
				if(chosenEvent is Assassination){
					AssassinationExposed (chosenEvent, ref targetKingdomSpecific, ref sourceKingdomSpecific);
				}else if(chosenEvent is InvasionPlan){
					InvasionPlanExposed (chosenEvent, ref targetKingdomSpecific, ref sourceKingdomSpecific);
				}else if(chosenEvent is JoinWar){
					JoinWar (chosenEvent, ref targetKingdomSpecific, ref sourceKingdomSpecific);
				}else if(chosenEvent is Militarization){
					Militarization (chosenEvent, ref targetKingdomSpecific, ref sourceKingdomSpecific);
				}else if(chosenEvent is PowerGrab){
					PowerGrab (chosenEvent, ref targetKingdomSpecific, ref sourceKingdomSpecific);
				}
				if(targetKingdomSpecific != null && sourceKingdomSpecific != null){
					EventExposed (chosenEvent, targetKingdomSpecific, sourceKingdomSpecific);
				}
			}

		}

	}
	private void AssassinationExposed(GameEvent chosenEvent, ref Kingdom target, ref Kingdom source){
		target = ((Assassination)chosenEvent).targetCitizen.city.kingdom;
		source = ((Assassination)chosenEvent).assassinKingdom;
	}
	private void InvasionPlanExposed(GameEvent chosenEvent, ref Kingdom target, ref Kingdom source){
		target = ((InvasionPlan)chosenEvent).targetKingdom;
		source = ((InvasionPlan)chosenEvent).sourceKingdom;
	}
	private void JoinWar(GameEvent chosenEvent, ref Kingdom target, ref Kingdom source){
		target = ((JoinWar)chosenEvent).candidateForAlliance.city.kingdom;
		source = ((JoinWar)chosenEvent).startedBy.city.kingdom;
	}
	private void Militarization(GameEvent chosenEvent, ref Kingdom target, ref Kingdom source){
		target = ((Militarization)chosenEvent).startedBy.city.kingdom;
		source = ((Militarization)chosenEvent).startedBy.city.kingdom;
	}
	private void PowerGrab(GameEvent chosenEvent, ref Kingdom target, ref Kingdom source){
		target = ((PowerGrab)chosenEvent).kingToOverthrow.city.kingdom;
		source = ((PowerGrab)chosenEvent).startedBy.city.kingdom;
	}
	private void EventExposed(GameEvent chosenEvent, Kingdom targetKingdomSpecific, Kingdom sourceKingdomSpecific){
		int chance = UnityEngine.Random.Range (0, 100);
		Citizen target = targetKingdomSpecific.king;
		Citizen source = sourceKingdomSpecific.king;

		if(target.isKing){
			RelationshipKings relationship = this.sourceKingdom.king.SearchRelationshipByID (target.id);
			RelationshipKings relationshipToCreator = this.sourceKingdom.king.SearchRelationshipByID (source.id);
			if(relationship == null){
				return;
			}
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
						RelationshipKings relationshipReverse = target.SearchRelationshipByID (this.sourceKingdom.king.id);
						relationshipReverse.AdjustLikeness (10);
						relationshipReverse.relationshipHistory.Add (new History (
							GameManager.Instance.month,
							GameManager.Instance.days,
							GameManager.Instance.year,
							this.sourceKingdom.king + " helped reveal a " + chosenEvent.ToString() + " against " + target.name + "'s kingdom.",
							HISTORY_IDENTIFIER.KING_RELATIONS,
							true
						));
						target.InformedAboutHiddenEvent (chosenEvent, this.spy);
						UncoverHiddenEvent (chosenEvent, target);
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
						RelationshipKings relationshipReverse = target.SearchRelationshipByID (this.sourceKingdom.king.id);
						relationshipReverse.AdjustLikeness (10);
						relationshipReverse.relationshipHistory.Add (new History (
							GameManager.Instance.month,
							GameManager.Instance.days,
							GameManager.Instance.year,
							this.sourceKingdom.king + " helped reveal a " + chosenEvent.ToString() + " against " + target.name + "'s kingdom.",
							HISTORY_IDENTIFIER.KING_RELATIONS,
							true
						));
						target.InformedAboutHiddenEvent (chosenEvent, this.spy);
						UncoverHiddenEvent (chosenEvent, target);
					}
					relationshipToCreator.AdjustLikeness (-10, EVENT_TYPES.ESPIONAGE);
					relationshipToCreator.relationshipHistory.Add (new History (
						GameManager.Instance.month,
						GameManager.Instance.days,
						GameManager.Instance.year,
						relationshipToCreator.sourceKing + " found out about a " + chosenEvent.ToString() + " against his friend " + target.name + " launched by " + relationshipToCreator.king,
						HISTORY_IDENTIFIER.KING_RELATIONS,
						false
					));
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
						RelationshipKings relationshipReverse = target.SearchRelationshipByID (this.sourceKingdom.king.id);
						relationshipReverse.AdjustLikeness (10);
						relationshipReverse.relationshipHistory.Add (new History (
							GameManager.Instance.month,
							GameManager.Instance.days,
							GameManager.Instance.year,
							this.sourceKingdom.king + " helped reveal a " + chosenEvent.ToString() + " against " + target.name + "'s kingdom.",
							HISTORY_IDENTIFIER.KING_RELATIONS,
							true
						));
						target.InformedAboutHiddenEvent (chosenEvent, this.spy);
						UncoverHiddenEvent (chosenEvent, target);
					}
					relationshipToCreator.AdjustLikeness (-15, EVENT_TYPES.ESPIONAGE);
					relationshipToCreator.relationshipHistory.Add (new History (
						GameManager.Instance.month,
						GameManager.Instance.days,
						GameManager.Instance.year,
						relationshipToCreator.sourceKing + " found out about a " + chosenEvent.ToString() + " against his friend " + target.name + " launched by " + relationshipToCreator.king,
						HISTORY_IDENTIFIER.KING_RELATIONS,
						false
					));

				}

				
			}
		}
	}
	private GameEvent GetEventToExpose(bool isGetAllEvents = false){
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

		if(invasionPlanEvents != null){
			for(int i = 0; i < invasionPlanEvents.Count; i++){
				if(((InvasionPlan)invasionPlanEvents[i]).targetKingdom.id == this.targetKingdom.id && ((InvasionPlan)invasionPlanEvents[i]).startedBy.city.kingdom.id != this.sourceKingdom.id){
					allEventsAffectionTarget.Add (assassinationEvents [i]);
				}
			}
		}

		if(joinWarEvents != null){
			for(int i = 0; i < joinWarEvents.Count; i++){
				if(((JoinWar)joinWarEvents[i]).candidateForAlliance.city.kingdom.id == this.targetKingdom.id && ((JoinWar)joinWarEvents[i]).startedBy.city.kingdom.id != this.sourceKingdom.id){
					allEventsAffectionTarget.Add (joinWarEvents [i]);
				}
			}
		}

		if(militarizationEvents != null){
			for(int i = 0; i < militarizationEvents.Count; i++){
				if(((Militarization)militarizationEvents[i]).startedBy.city.kingdom.id == this.targetKingdom.id && ((Militarization)militarizationEvents[i]).startedBy.city.kingdom.id != this.sourceKingdom.id){
					allEventsAffectionTarget.Add (militarizationEvents [i]);
				}
			}
		}
		if(assassinationEvents != null){
			for(int i = 0; i < assassinationEvents.Count; i++){
				if(((Assassination)assassinationEvents[i]).targetCitizen.city.kingdom.id == this.targetKingdom.id && ((Assassination)assassinationEvents[i]).assassinKingdom.id != this.sourceKingdom.id){
					allEventsAffectionTarget.Add (assassinationEvents [i]);
				}
			}
		}

		if(rebellionPlotEvents != null){
			for(int i = 0; i < rebellionPlotEvents.Count; i++){

			}
		}

		if(powerGrabEvents != null){
			for(int i = 0; i < powerGrabEvents.Count; i++){
				if(((PowerGrab)powerGrabEvents[i]).kingToOverthrow.city.kingdom.id == this.targetKingdom.id){
					allEventsAffectionTarget.Add (powerGrabEvents [i]);
				}
			}
		}


		if(allEventsAffectionTarget.Count > 0){
			if(isGetAllEvents){
				this.allEventsAffectingTarget.Clear();
				this.allEventsAffectingTarget.AddRange(allEventsAffectionTarget);
			}
			return allEventsAffectionTarget [UnityEngine.Random.Range (0, allEventsAffectionTarget.Count)];
		}else{
			Debug.Log ("NO HIDDEN EVENT TO EXPOSE HERE!");
			return null;
		}

	}

	internal void UncoverHiddenEvent(GameEvent chosenEvent, Citizen uncoverer){
		if(chosenEvent is Assassination){
			((Assassination)chosenEvent).uncovered.Add(uncoverer);
		}else if(chosenEvent is InvasionPlan){
			((InvasionPlan)chosenEvent).uncovered.Add(uncoverer);
		}else if(chosenEvent is JoinWar){
			((JoinWar)chosenEvent).uncovered.Add(uncoverer);
		}else if(chosenEvent is Militarization){
			((Militarization)chosenEvent).uncovered.Add(uncoverer);
		}else if(chosenEvent is PowerGrab){
			((PowerGrab)chosenEvent).uncovered.Add(uncoverer);
		}
	}
}
