using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RequestPeace : GameEvent {

	internal RelationshipKingdom targetKingdomRel;
	internal Kingdom targetKingdom;

	internal Citizen citizenSent;
	internal List<Citizen> saboteurs;

	public RequestPeace(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen citizenSent, Kingdom targetKingdom, List<Citizen> saboteurs) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REQUEST_PEACE;
		if (citizenSent.role == ROLE.KING) {
			this.description = startedBy.name + " has decided to go to " + targetKingdom.name + " to request peace.";
		} else {
			this.description = startedBy.name + " has sent " + citizenSent.name + " to " + targetKingdom.name + " to request peace.";
		}
		this.durationInWeeks = 4;
		this.remainingWeeks = this.durationInWeeks;
		this.citizenSent = citizenSent;
		this.targetKingdom = targetKingdom;
		this.targetKingdomRel = targetKingdom.GetRelationshipWithOtherKingdom(this.startedBy.city.kingdom);
		this.saboteurs = saboteurs;

		for (int i = 0; i < this.saboteurs.Count; i++) {
			((Envoy)this.saboteurs[i].assignedRole).inAction = true;
		}

		this.startedBy.city.hexTile.AddEventOnTile(this);
		this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			this.startedBy.name + " started a request peace event.", HISTORY_IDENTIFIER.NONE));

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.citizenSent.isDead) {
			this.resolution = this.citizenSent.name + " died before he could reach " + this.targetKingdom.name;
			this.DoneEvent();
			return;
		}

		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;
		}
		int targetWarExhaustion = this.targetKingdomRel.kingdomWar.exhaustion;
		if (this.remainingWeeks <= 0) {
			int chanceForSuccess = 0;

			if (targetWarExhaustion >= 75) {
				chanceForSuccess += 75;
			}else if (targetWarExhaustion >= 50) {
				chanceForSuccess += 50;
			}else{
				chanceForSuccess += 10;
			}

			if (this.citizenSent.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
				chanceForSuccess += 15;
			}

			for (int i = 0; i < this.saboteurs.Count; i++) {
				chanceForSuccess -= 15;
				if (this.saboteurs [i].skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
					chanceForSuccess -= 10;
				}
			}

			int chance = Random.Range(0, 100);
//			int chance = Random.Range(0, chanceForSuccess);
			if (chance < chanceForSuccess) {
				//request accepted
				KingdomManager.Instance.DeclarePeaceBetweenKingdoms (this.startedByKingdom, this.targetKingdom);
				this.resolution = this.targetKingdom.king.name + " accepted " + this.startedBy.name + "'s request for peace.";
			} else {
				//request rejected
				RelationshipKingdom relationshipOfRequester = this.startedByKingdom.GetRelationshipWithOtherKingdom(this.targetKingdom);
				int moveOnMonth = GameManager.Instance.month;
				for (int i = 0; i < 3; i++) {
					moveOnMonth += 1;
					if (moveOnMonth > 12) {
						moveOnMonth = 1;
					}
				}
				relationshipOfRequester.monthToMoveOnAfterRejection = (MONTH)(moveOnMonth);
				EventManager.Instance.onWeekEnd.AddListener(relationshipOfRequester.MoveOnAfterRejection);
				this.resolution = this.targetKingdom.king.name + " rejected " + this.startedBy.name + "'s request for peace.";
			}
			this.DoneEvent();
		}
	}

	internal override void DoneEvent(){
		Debug.Log (this.startedBy.name + "'s request peace event has ended. " + this.resolution);
		this.isActive = false;
		for (int i = 0; i < this.saboteurs.Count; i++) {
			((Envoy)this.saboteurs[i].assignedRole).inAction = false;
		}
		EventManager.Instance.onGameEventEnded.Invoke(this);
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
}
