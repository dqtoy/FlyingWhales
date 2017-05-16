using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RequestPeace : GameEvent {

	private RelationshipKingdom _targetKingdomRel;
	private Kingdom _targetKingdom;

	private Citizen _citizenSent;
	private List<Citizen> _saboteurs;

	#region getters/setters
	public Kingdom targetKingdom {
		get { return this._targetKingdom; }
	}

	public Citizen citizenSent {
		get { return this._citizenSent; }
	}

	public List<Citizen> saboteurs {
		get { return this._saboteurs; }
	}
	#endregion

	public RequestPeace(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen _citizenSent, Kingdom _targetKingdom, List<Citizen> _saboteurs) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REQUEST_PEACE;
		if (_citizenSent.role == ROLE.KING) {
			this.description = startedBy.name + " has decided to go to " + _targetKingdom.name + " to request peace.";
		} else {
			this.description = startedBy.name + " has sent " + _citizenSent.name + " to " + _targetKingdom.name + " to request peace.";
		}
		this.durationInDays = 4;
		this.remainingDays = this.durationInDays;
		this._citizenSent = _citizenSent;
		this._targetKingdom = _targetKingdom;
		this._targetKingdomRel = _targetKingdom.GetRelationshipWithOtherKingdom(this.startedBy.city.kingdom);
		this._saboteurs = _saboteurs;

		for (int i = 0; i < this._saboteurs.Count; i++) {
			((Envoy)this._saboteurs[i].assignedRole).inAction = true;
		}

		this.startedBy.city.hexTile.AddEventOnTile(this);
		this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			this.startedBy.name + " started a request peace event.", HISTORY_IDENTIFIER.NONE));

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this._citizenSent.isDead) {
			this.resolution = this._citizenSent.name + " died before he could reach " + this._targetKingdom.name;
			this.DoneEvent();
			return;
		}

		if (this.remainingDays > 0) {
			this.remainingDays -= 1;
		}
		int targetWarExhaustion = this._targetKingdomRel.kingdomWar.exhaustion;
		if (this.remainingDays <= 0) {
			int chanceForSuccess = 0;

			if (targetWarExhaustion >= 75) {
				chanceForSuccess += 75;
			}else if (targetWarExhaustion >= 50) {
				chanceForSuccess += 50;
			}else{
				chanceForSuccess += 10;
			}

			if (this._citizenSent.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
				chanceForSuccess += 15;
			}

			for (int i = 0; i < this._saboteurs.Count; i++) {
				chanceForSuccess -= 15;
				if (this._saboteurs [i].skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
					chanceForSuccess -= 10;
				}
			}

			int chance = Random.Range(0, 100);
//			int chance = Random.Range(0, chanceForSuccess);
			if (chance < chanceForSuccess) {
				//request accepted
				KingdomManager.Instance.GetWarBetweenKingdoms(this.startedByKingdom, this._targetKingdom).DeclarePeace();
//				KingdomManager.Instance.DeclarePeaceBetweenKingdoms (this.startedByKingdom, this._targetKingdom);
				this.resolution = this._targetKingdom.king.name + " accepted " + this.startedBy.name + "'s request for peace.";
			} else {
				//request rejected
				RelationshipKingdom relationshipOfRequester = this.startedByKingdom.GetRelationshipWithOtherKingdom(this._targetKingdom);
				int moveOnMonth = GameManager.Instance.month;
				for (int i = 0; i < 3; i++) {
					moveOnMonth += 1;
					if (moveOnMonth > 12) {
						moveOnMonth = 1;
					}
				}
				relationshipOfRequester.SetMoveOnPeriodAfterRequestPeaceRejection(moveOnMonth);
				this.resolution = this._targetKingdom.king.name + " rejected " + this.startedBy.name + "'s request for peace.";
			}
			this.DoneEvent();
		}
	}

	internal override void DoneEvent(){
		Debug.Log (this.startedBy.name + "'s request peace event has ended. " + this.resolution);
		this.isActive = false;
		for (int i = 0; i < this._saboteurs.Count; i++) {
			((Envoy)this._saboteurs[i].assignedRole).inAction = false;
		}
		EventManager.Instance.onGameEventEnded.Invoke(this);
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		this.endDay = GameManager.Instance.days;
		this.endMonth = GameManager.Instance.month;
		this.endYear = GameManager.Instance.year;
	}
}
