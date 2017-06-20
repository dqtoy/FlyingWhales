using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Militarization : GameEvent {

	private InvasionPlan _invasionPlanThatTriggeredEvent;
	private List<Citizen> _uncovered;

	public InvasionPlan invasionPlanThatTriggeredEvent{
		get { return this._invasionPlanThatTriggeredEvent; }
	}
	public List<Citizen> uncovered{
		get { return this._uncovered; }
	}

	public Militarization(int startWeek, int startMonth, int startYear, Citizen startedBy, InvasionPlan _invasionPlanThatTriggeredEvent) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.MILITARIZATION;
		this.name = "Militarization";
		this.description = startedBy.name + " prioritizing the training of his generals and army, in preparation for war.";
		this.durationInDays = 120;
		this.remainingDays = this.durationInDays;
		this._invasionPlanThatTriggeredEvent = _invasionPlanThatTriggeredEvent;
		this._uncovered = new List<Citizen>();

		Log startLog = _invasionPlanThatTriggeredEvent.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
			               "Events", "War", "militarization_start");
		startLog.AddToFillers (this.startedBy, this.startedBy.name);
		startLog.AddToFillers (this.startedByKingdom, this.startedByKingdom.name);
		startLog.AddToFillers (_invasionPlanThatTriggeredEvent.targetKingdom, _invasionPlanThatTriggeredEvent.targetKingdom.name);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
	}

	#region overrides
	internal override void PerformAction(){
		if (this.startedBy.isDead) {
			this.resolution = this.startedBy.name + " died before the event could finish.";
			this.DoneEvent();
			return;
		}
		this.remainingDays -= 1;
		if (this.remainingDays <= 0) {
			this.DoneEvent();
		}
	}

	internal override void DoneEvent(){
        base.DoneEvent();
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
//		if (_invasionPlanThatTriggeredEvent.isActive) {
//			_invasionPlanThatTriggeredEvent.MilitarizationDone ();
//		}
	}

    internal override void CancelEvent() {
        base.CancelEvent();
        this.DoneEvent();
    }
    #endregion

    internal void AddCitizenThatUncoveredEvent(Citizen citizen){
		this._uncovered.Add(citizen);
	}
}
