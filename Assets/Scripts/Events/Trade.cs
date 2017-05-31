using UnityEngine;
using System.Collections;

public class Trade : GameEvent {

    private Kingdom _sourceKingdom;
    private Kingdom _targetKingdom;
    private Citizen _trader;

    #region getters/setters
    public Kingdom targetKingdom {
        get { return this._targetKingdom; }
    }
    #endregion

    public Trade(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _sourceKingdom, Kingdom _targetKingdom, Citizen _trader) 
        : base(startWeek, startMonth, startYear, startedBy) {
        this.eventType = EVENT_TYPES.TRADE;
        this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
        this.remainingDays = this.durationInDays;

        //Event Specific
        this._sourceKingdom = _sourceKingdom;
        this._targetKingdom = _targetKingdom;
        this._trader = _trader;
    }

    internal override void PerformAction() {

    }

    internal override void CancelEvent() {

    }

    internal override void DoneEvent() {
        
    }

    internal void KillTrader() {

    }
}
