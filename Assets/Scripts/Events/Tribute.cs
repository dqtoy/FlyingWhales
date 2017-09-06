﻿using UnityEngine;
using System.Collections;

public class Tribute : GameEvent {
	public Kingdom targetKingdom;
	public Citizen tributer;

	public Tribute(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.TRIBUTE;
		this.name = "Tribute";
		this.targetKingdom = targetKingdom;
		CreateTributer ();
	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		base.DoneCitizenAction(citizen);
		GiveTribute ();
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

	private void CreateTributer(){
		this.tributer = this.startedByKingdom.capitalCity.CreateAgent (ROLE.TRIBUTER, EVENT_TYPES.TRIBUTE, this.targetKingdom.capitalCity.hexTile, this.durationInDays);
		if(this.tributer != null){
			this.tributer.assignedRole.Initialize (this);
		}
	}
	private void GiveTribute(){
		this.startedByKingdom.AdjustHappiness (-5);
		this.targetKingdom.AdjustHappiness (5);
		this.targetKingdom.AdjustPrestige (15);

		KingdomRelationship relationship = this.targetKingdom.GetRelationshipWithKingdom (this.startedByKingdom);
		if(!relationship.eventBuffs[this.eventType]){
			relationship.AddEventModifier (15, "Tribute", this);
		}else{
			relationship.AddEventModifier (5, "Tribute", this);
		}
		DoneEvent ();
	}
}
