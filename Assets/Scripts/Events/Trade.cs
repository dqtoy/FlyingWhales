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

	#region Overrides
    internal override void PerformAction() {

    }

    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        CreateTradeRouteBetweenKingdoms();
    }

//    internal override void CancelEvent() {
//        Debug.LogError("Trade Event was cancelled!");
//        this.isActive = false;
//    }

    internal override void DoneEvent() {
        base.DoneEvent();
//        this._trader.assignedRole.DestroyGO();
    }
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
    #endregion
    internal void KillTrader() {
        this._trader.Death(DEATH_REASONS.BATTLE);
    }

    internal void CreateTradeRouteBetweenKingdoms() {
        List<RESOURCE> resourcesThatTargetKingdomDoesNotHave = this._sourceKingdom.GetResourcesOtherKingdomDoesNotHave(this._targetKingdom);
        RelationshipKingdom rel1 = this._sourceKingdom.GetRelationshipWithOtherKingdom(this._targetKingdom);
        RelationshipKingdom rel2 = this._targetKingdom.GetRelationshipWithOtherKingdom(this._sourceKingdom);
        if (resourcesThatTargetKingdomDoesNotHave.Count > 0 && !rel1.isAtWar && !rel2.isAtWar && 
            !this._sourceKingdom.embargoList.ContainsKey(this._targetKingdom) && !this._targetKingdom.embargoList.ContainsKey(this._sourceKingdom))  {
            RESOURCE resourceToTrade = resourcesThatTargetKingdomDoesNotHave[Random.Range(0, resourcesThatTargetKingdomDoesNotHave.Count)];
            TradeRoute tradeRoute = new TradeRoute(resourceToTrade, this._sourceKingdom, this._targetKingdom);
            this._sourceKingdom.AddTradeRoute(tradeRoute);
            this._targetKingdom.AddTradeRoute(tradeRoute);
            this._sourceKingdom.UpdateAllCitiesDailyGrowth();
            this._targetKingdom.UpdateAllCitiesDailyGrowth();
            this._sourceKingdom.UpdateBasicResourceCount();
            this._targetKingdom.UpdateBasicResourceCount();
            Debug.Log("Trade was successful " + this._sourceKingdom.name + " gained GOLD. " + this._targetKingdom.name + " gained " + resourceToTrade.ToString());
        } else {
            Debug.Log(this._sourceKingdom.name + " and " + this._targetKingdom.name + " are no longer elligible for trade!");
        }
        this.DoneEvent();
    }
}
