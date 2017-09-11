﻿using UnityEngine;
using System.Collections;

public class MutualDefenseTreaty : GameEvent {
	public Kingdom targetKingdom;
	public Citizen treatyOfficer;

    private KingdomRelationship _sourceRel;
    private KingdomRelationship _targetRel;

    public MutualDefenseTreaty(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.MUTUAL_DEFENSE_TREATY;
		this.name = "Mutual Defense Treaty";
		this.targetKingdom = targetKingdom;

        _sourceRel = startedByKingdom.GetRelationshipWithKingdom(targetKingdom);
        _targetRel = targetKingdom.GetRelationshipWithKingdom(startedByKingdom);

        _sourceRel.currentActiveDefenseTreatyOffer = this;
        _targetRel.currentActiveDefenseTreatyOffer = this;

        CreateTreatyOfficer ();
	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		base.DoneCitizenAction(citizen);
		EvaluateOffer ();
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent ();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent ();
	}

	internal override void DoneEvent(){
		base.DoneEvent();
        _sourceRel.currentActiveDefenseTreatyOffer = null;
        _targetRel.currentActiveDefenseTreatyOffer = null;
    }

	internal override void CancelEvent (){
		base.CancelEvent ();
	}
	#endregion

	private void CreateTreatyOfficer(){
		this.treatyOfficer = this.startedByKingdom.capitalCity.CreateAgent (ROLE.TREATYOFFICER, EVENT_TYPES.MUTUAL_DEFENSE_TREATY, this.targetKingdom.capitalCity.hexTile, this.durationInDays);
		if(this.treatyOfficer != null){
			this.treatyOfficer.assignedRole.Initialize (this);
		}
	}
	private void EvaluateOffer(){
		if(_targetRel.totalLike >= 0 && (this.targetKingdom.mainThreat == null || this.targetKingdom.mainThreat.id != this.startedByKingdom.id)){
			AcceptOffer ();
		}else{
			RejectOffer ();
		}
		DoneEvent ();
	}
	private void AcceptOffer(){
		_sourceRel.ChangeMutualDefenseTreaty (true);
	}
	private void RejectOffer(){
		this.targetKingdom.UpdateCurrentDefenseTreatyRejectionDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
	}
}
