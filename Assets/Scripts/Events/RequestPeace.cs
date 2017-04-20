using UnityEngine;
using System.Collections;

public class RequestPeace : GameEvent {

	public RequestPeace(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen citizenSent, Kingdom targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REQUEST_PEACE;
		if (citizenSent.role == ROLE.KING) {
			this.description = startedBy.name + " has decided to go to " + targetKingdom.name + " to request peace.";
		} else {
			this.description = startedBy.name + " has sent " + citizenSent.name + " to " + targetKingdom.name + " to request peace.";
		}
		this.durationInWeeks = 4;
		this.remainingWeeks = this.durationInWeeks;

		this.startedBy.city.hexTile.AddEventOnTile(this);
		this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, 
			this.startedBy.name + " started a request peace event.", HISTORY_IDENTIFIER.NONE));

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	internal override void PerformAction(){
		if (this.remainingWeeks > 0) {
			this.remainingWeeks -= 1;
		}
		int targetWarExhaustion = 0;
		if (this.remainingWeeks <= 0) {
			int chanceForSuccess = 0;

			if (targetWarExhaustion < 50) {
				chanceForSuccess += 10;
			}
		}
	}

	internal override void DoneEvent(){
		Debug.Log (this.startedBy.name + "'s request peace event has ended. " + this.resolution);
		this.isActive = false;
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
}
