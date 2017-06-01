using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trade : GameEvent {

    private Kingdom _sourceKingdom;
    private Kingdom _targetKingdom;
    private Citizen _trader;

    #region getters/setters
    public Kingdom targetKingdom {
        get { return this._targetKingdom; }
    }
    public Kingdom sourceKingdom {
        get { return this._sourceKingdom; }
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

        EventManager.Instance.AddEventToDictionary(this);
    }

    internal override void PerformAction() {

    }

    internal override void CancelEvent() {
        Debug.LogError("Trade Event was cancelled!");
        this.isActive = false;
    }

    internal override void DoneEvent() {
        this._trader.assignedRole.DestroyGO();
        this.isActive = false;
    }
    
    internal void KillTrader() {
        this._trader.Death(DEATH_REASONS.BATTLE);
    }

    internal void CreateTradeRouteBetweenKingdoms() {
        List<RESOURCE> resourcesThatTargetKingdomDoesNotHave = this._sourceKingdom.GetResourcesOtherKingdomDoesNotHave(this._targetKingdom);
        if (resourcesThatTargetKingdomDoesNotHave.Count > 0) {
            RESOURCE resourceToTrade = resourcesThatTargetKingdomDoesNotHave[Random.Range(0, resourcesThatTargetKingdomDoesNotHave.Count)];
            TradeRoute sourceKingdomTradeRoute = new TradeRoute(RESOURCE.GOLD, this._targetKingdom);
            TradeRoute targetKingdomTradeRoute = new TradeRoute(resourceToTrade, this._sourceKingdom);
            this._sourceKingdom.AddTradeRoute(sourceKingdomTradeRoute);
            this._targetKingdom.AddTradeRoute(targetKingdomTradeRoute);
            Debug.Log("Trade was successful " + this._sourceKingdom.name + " gained GOLD. " + this._targetKingdom.name + " gained " + resourceToTrade.ToString());
        } else {
            Debug.Log(this._sourceKingdom.name + " no longer has any resources that " + this._targetKingdom.name + " does not have. Trade event was unsuccessful!");
        }
        this.DoneEvent();
    }
}
