using UnityEngine;
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

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "MutualDefenseTreaty", "event_title");

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "MutualDefenseTreaty", "start");
        newLog.AddToFillers(targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

        //if (UIManager.Instance.currentlyShowingKingdom == startedByKingdom || UIManager.Instance.currentlyShowingKingdom == targetKingdom) {
        //    UIManager.Instance.ShowNotification(newLog);
        //}
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
		this.treatyOfficer = this.startedByKingdom.capitalCity.CreateNewAgent (ROLE.TREATYOFFICER, EVENT_TYPES.MUTUAL_DEFENSE_TREATY, this.targetKingdom.capitalCity.hexTile);
		if(this.treatyOfficer != null){
			this.treatyOfficer.assignedRole.Initialize (this);
		}
	}
	private void EvaluateOffer(){
        Log resultLog;
		if(_targetRel.totalLike >= 0 && (this.targetKingdom.mainThreat == null || this.targetKingdom.mainThreat.id != this.startedByKingdom.id)){
			AcceptOffer ();
            resultLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "MilitaryAllianceOffer", "accept");
            resultLog.AddToFillers(targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
            resultLog.AddToFillers(startedByKingdom, startedByKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        } else{
			RejectOffer ();
            resultLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "MilitaryAllianceOffer", "reject");
            resultLog.AddToFillers(targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
            resultLog.AddToFillers(startedByKingdom, startedByKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        }
        //if (UIManager.Instance.currentlyShowingKingdom == startedByKingdom || UIManager.Instance.currentlyShowingKingdom == targetKingdom) {
        //    UIManager.Instance.ShowNotification(resultLog);
        //}
        DoneEvent ();
	}
	private void AcceptOffer(){
		_sourceRel.ChangeMutualDefenseTreaty (true);
	}
	private void RejectOffer(){
		this.targetKingdom.UpdateCurrentDefenseTreatyRejectionDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
	}
}
