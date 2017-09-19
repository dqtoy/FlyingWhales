using UnityEngine;
using System.Collections;

public class Instigation : GameEvent {
	public Kingdom targetKingdom;
	public Kingdom instigatedKingdom;
	public Citizen instigator;

	public Instigation(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom instigatedKingdom, Kingdom targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.INSTIGATION;
		this.name = "Instigation";
		this.instigatedKingdom = instigatedKingdom;
		this.targetKingdom = targetKingdom;
		CreateInstigator ();

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Instigation", "event_title");
        newLogTitle.AddToFillers(null, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Instigation", "start");
        newLog.AddToFillers(startedByKingdom, startedByKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(instigatedKingdom, instigatedKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        newLog.AddToFillers(targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_3);

        //if (UIManager.Instance.currentlyShowingKingdom == startedByKingdom || UIManager.Instance.currentlyShowingKingdom == targetKingdom
        //    || UIManager.Instance.currentlyShowingKingdom == instigatedKingdom) {
        //    UIManager.Instance.ShowNotification(newLog);
        //}
    }

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		base.DoneCitizenAction(citizen);
		Instigate ();
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
	}

	internal override void CancelEvent (){
		base.CancelEvent ();
	}
	#endregion

	private void CreateInstigator(){
		this.instigator = this.startedByKingdom.capitalCity.CreateNewAgent (ROLE.INSTIGATOR, EVENT_TYPES.INSTIGATION, this.instigatedKingdom.capitalCity.hexTile);
		if(this.instigator != null){
			this.instigator.assignedRole.Initialize (this);
		}
	}
	private void Instigate(){
		KingdomRelationship relationship = this.instigatedKingdom.GetRelationshipWithKingdom (this.targetKingdom);
		if(!relationship.eventBuffs[this.eventType]){
			relationship.AddEventModifier (-15, "Instigation", this);
		}else{
			relationship.AddEventModifier (-5, "Instigation", this);
		}
		DoneEvent ();
	}
}
