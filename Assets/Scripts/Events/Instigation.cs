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
		this.instigator = this.startedByKingdom.capitalCity.CreateAgent (ROLE.INSTIGATOR, EVENT_TYPES.INSTIGATION, this.instigatedKingdom.capitalCity.hexTile, this.durationInDays);
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
