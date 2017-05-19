﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class War : GameEvent {

	private Kingdom _kingdom1;
	private Kingdom _kingdom2;

	private RelationshipKingdom _kingdom1Rel;
	private RelationshipKingdom _kingdom2Rel;

	private bool _isAtWar;
//	internal List<InvasionPlan> invasionPlans;
//	internal Militarization militarizationOfWar;

	#region getters/setters
	public Kingdom kingdom1 {
		get { return _kingdom1; }
	}

	public Kingdom kingdom2{
		get { return _kingdom2; }
	}

	public RelationshipKingdom kingdom1Rel {
		get { return _kingdom1Rel; }
	}

	public RelationshipKingdom kingdom2Rel {
		get { return _kingdom2Rel; }
	}

	public bool isAtWar {
		get { return _isAtWar; }
	}
	#endregion

	public War(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _kingdom1, Kingdom _kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.KINGDOM_WAR;
		this.description = "War between " + _kingdom1.name + " and " + _kingdom2.name + ".";
		this._kingdom1 = _kingdom1;
		this._kingdom2 = _kingdom2;
		this._kingdom1Rel = _kingdom1.GetRelationshipWithOtherKingdom(_kingdom2);
		this._kingdom2Rel = _kingdom2.GetRelationshipWithOtherKingdom(_kingdom1);
		this._kingdom1Rel.AssignWarEvent(this);
		this._kingdom2Rel.AssignWarEvent(this);

		EventManager.Instance.AddEventToDictionary(this);
	}

	#region overrides
	internal override void DoneEvent(){
		this.isActive = false;
	}
	#endregion

	internal void CreateInvasionPlan(Kingdom kingdomToDeclare, GameEvent gameEventTrigger){
		if (kingdomToDeclare.id == this._kingdom1.id) {
			this._kingdom1Rel.CreateInvasionPlan(gameEventTrigger);
		} else {
			this._kingdom2Rel.CreateInvasionPlan(gameEventTrigger);
		}
	}

	internal void CreateRequestPeaceEvent(Kingdom kingdomToRequest, Citizen citizenToSend, List<Citizen> saboteurs){
		if (kingdomToRequest.id == this._kingdom1.id) {
			this._kingdom1Rel.CreateRequestPeaceEvent(citizenToSend, saboteurs);
		} else {
			this._kingdom2Rel.CreateRequestPeaceEvent(citizenToSend, saboteurs);
		}
	}

	internal void DeclareWar(Kingdom sourceKingdom){
		if(!this._isAtWar){
			this._isAtWar = true;
			if(sourceKingdom.id == this._kingdom1.id){
				KingdomManager.Instance.DeclareWarBetweenKingdoms(this._kingdom1, this._kingdom2, this);
			}else{
				KingdomManager.Instance.DeclareWarBetweenKingdoms(this._kingdom2, this._kingdom1, this);
			}
		}
	}

	internal void DeclarePeace(){
		this._isAtWar = false;
		this._kingdom1Rel.DeclarePeace();
		this._kingdom2Rel.DeclarePeace();
		KingdomManager.Instance.DeclarePeaceBetweenKingdoms(this._kingdom1, this._kingdom2);
		this.DoneEvent();
	}

	internal Kingdom GetKingdomInvolvedInWar(Kingdom kingdom){
		if (kingdom1.id == kingdom.id) {
			return kingdom1;
		} else {
			return kingdom2;
		}
		return null;
	}
}
