using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Militarization : GameEvent {

	internal InvasionPlan invasionPlanThatTriggeredEvent;
	public List<Citizen> uncovered;

	public Militarization(int startWeek, int startMonth, int startYear, Citizen startedBy) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.MILITARIZATION;
		this.description = startedBy.name + " prioritizing the training of his generals and army, in preparation for war.";
		this.durationInWeeks = Random.Range(16, 24);
		this.remainingWeeks = this.durationInWeeks;
		this.uncovered = new List<Citizen>();

		this.startedBy.city.hexTile.AddEventOnTile(this);
		this.startedBy.history.Add (new History (startMonth, startWeek, startYear, this.startedBy.name + " started a Militarization for his/her Invasion Plan.", HISTORY_IDENTIFIER.NONE));
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		this.remainingWeeks -= 1;
		int envoyChance = Random.Range (0, 100);
		if (envoyChance < 20) {
			//Send envoy for Join War
			List<Citizen> envoys = this.startedByKingdom.GetAllCitizensOfType(ROLE.ENVOY).Where(x => !((Envoy)x.assignedRole).inAction).ToList();
			List<RelationshipKings> friends = this.startedBy.friends.Where(x => x.king.city.kingdom.IsKingdomAdjacentTo(invasionPlanThatTriggeredEvent.targetKingdom)).ToList();
			if (envoys.Count > 0 && friends.Count > 0) {
				Envoy envoyToSend = (Envoy)envoys [Random.Range (0, envoys.Count)].assignedRole;
				Citizen citizenToPersuade = friends[Random.Range(0, friends.Count)].king;
				JoinWar newJoinWarRequest = new JoinWar (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this.startedBy, 
					citizenToPersuade, envoyToSend, invasionPlanThatTriggeredEvent.targetKingdom);
			} else {
				Debug.Log ("Cannot send envoy because there are none or all of them are busy or there is no one to send envoy to");
			}
		}
		if (this.remainingWeeks <= 0) {
			this.DoneEvent();
		}
	}

	internal override void DoneEvent(){
		this.isActive = false;
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		if (invasionPlanThatTriggeredEvent.isActive) {
			invasionPlanThatTriggeredEvent.MilitarizationDone ();
		}
		EventManager.Instance.onGameEventEnded.Invoke(this);
	}
}
