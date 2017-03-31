using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvasionPlan : GameEvent {

	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;

	public InvasionPlan(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom, Kingdom targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + ".";
		this.durationInWeeks = 8;
		this.remainingWeeks = this.durationInWeeks;
		this.sourceKingdom = sourceKingdom;
		this.targetKingdom = targetKingdom;
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}

	internal override void PerformAction(){
		
	}

	internal override void DoneCitizenAction(Citizen citizen){
		
	}

	internal override void DoneEvent(){
		
	}

	private void StartMilitarizationEvent(){
		Militarization currentMilitarization = null;
		if(IsThereMilitarizationActive(ref currentMilitarization)){
			currentMilitarization.durationInWeeks += 4;
		}else{
			//New Militarization
		}
	}

	private bool IsThereMilitarizationActive(ref Militarization currentMilitarization){
		List<GameEvent> militarizationEvents = EventManager.Instance.GetEventsOfType (EVENT_TYPES.MILITARIZATION);
		if(militarizationEvents != null){
			for (int i = 0; i < militarizationEvents.Count; i++) {
				if (((Militarization)militarizationEvents[i]).startedBy.id == sourceKingdom.king.id) {
					currentMilitarization = (Militarization) militarizationEvents [i];
					return true;
				}
			}
			return false;
		}else{
			return false;
		}
	}
}
