using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Trade : GameEvent {

    private City _sourceCity;
    private City _targetCity;
    private Citizen _trader;

    #region getters/setters
    public City targetCity {
        get { return this._targetCity; }
    }
    public City sourceCity {
        get { return this._sourceCity; }
    }
    #endregion

    public Trade(int startWeek, int startMonth, int startYear, Citizen startedBy, City _sourceCity, City _targetCity, Citizen _trader) 
        : base(startWeek, startMonth, startYear, startedBy) {
        this.eventType = EVENT_TYPES.TRADE;
        this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
        this.remainingDays = this.durationInDays;

        //Event Specific
        this._sourceCity = _sourceCity;
        this._targetCity = _targetCity;
        this._trader = _trader;

        EventManager.Instance.AddEventToDictionary(this);
    }

	#region Overrides
    internal override void PerformAction() {

    }

    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        citizen.assignedRole.DestroyGO();
        TradeResources();
        //CreateTradeRouteBetweenKingdoms();
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
        this._trader.assignedRole.DestroyGO();
        this.DoneEvent ();
	}
    #endregion

    private void TradeResources() {
        if(this._sourceCity.kingdom.id != this._targetCity.kingdom.id) {
            Kingdom sourceKingdom = this._sourceCity.kingdom;
            Kingdom targetKingdom = this._targetCity.kingdom;

            RelationshipKingdom rel1 = sourceKingdom.GetRelationshipWithOtherKingdom(targetKingdom);
            RelationshipKingdom rel2 = targetKingdom.GetRelationshipWithOtherKingdom(sourceKingdom);

            List<RESOURCE> resourcesThatTargetKingdomDoesNotHave = sourceKingdom.GetResourcesOtherKingdomDoesNotHave(targetKingdom);
            List<RESOURCE> resourcesThatThisKingdomDoesNotHave = targetKingdom.GetResourcesOtherKingdomDoesNotHave(sourceKingdom);

            if ((resourcesThatTargetKingdomDoesNotHave.Count > 0 || resourcesThatThisKingdomDoesNotHave.Count > 0)
                && !rel1.isAtWar && !rel2.isAtWar &&
                !sourceKingdom.embargoList.ContainsKey(targetKingdom) &&
                !targetKingdom.embargoList.ContainsKey(sourceKingdom)) {

                int dailyGrowthGained = 0;
                int techGrowthGained = 0;

                List<RESOURCE> resourcesToTrade = resourcesThatTargetKingdomDoesNotHave.Union(resourcesThatThisKingdomDoesNotHave).ToList();

                for (int i = 0; i < resourcesToTrade.Count; i++) {
                    RESOURCE currResource = resourcesToTrade[i];
                    RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.First();
                    if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
                        techGrowthGained += (int)Utilities.resourceBenefits[currResource][resourceBenefit];
                    } else if (resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
                        dailyGrowthGained += (int)Utilities.resourceBenefits[currResource][resourceBenefit];
                    }
                }

                dailyGrowthGained = (dailyGrowthGained * 15) * sourceCity.structures.Count;
                techGrowthGained = (techGrowthGained * 15) * sourceCity.structures.Count;

                this._sourceCity.AdjustDailyGrowth(dailyGrowthGained);
                sourceKingdom.AdjustTechCounter(techGrowthGained);

                this._targetCity.AdjustDailyGrowth(dailyGrowthGained / 2);
                targetKingdom.AdjustTechCounter(techGrowthGained / 2);

            } else {
                Debug.Log(this.sourceCity.name + " and " + this.targetCity.name + " are no longer elligible for trade!");
            }
        }
        this.DoneEvent();



    }

    internal void KillTrader() {
        this._trader.Death(DEATH_REASONS.BATTLE);
    }

    //internal void CreateTradeRouteBetweenKingdoms() {
    //    List<RESOURCE> resourcesThatTargetKingdomDoesNotHave = this._sourceKingdom.GetResourcesOtherKingdomDoesNotHave(this._targetKingdom);
    //    RelationshipKingdom rel1 = this._sourceKingdom.GetRelationshipWithOtherKingdom(this._targetKingdom);
    //    RelationshipKingdom rel2 = this._targetKingdom.GetRelationshipWithOtherKingdom(this._sourceKingdom);
    //    if (resourcesThatTargetKingdomDoesNotHave.Count > 0 && !rel1.isAtWar && !rel2.isAtWar && 
    //        !this._sourceKingdom.embargoList.ContainsKey(this._targetKingdom) && !this._targetKingdom.embargoList.ContainsKey(this._sourceKingdom))  {
    //        RESOURCE resourceToTrade = resourcesThatTargetKingdomDoesNotHave[Random.Range(0, resourcesThatTargetKingdomDoesNotHave.Count)];
    //        TradeRoute tradeRoute = new TradeRoute(resourceToTrade, this._sourceKingdom, this._targetKingdom);
    //        this._sourceKingdom.AddTradeRoute(tradeRoute);
    //        this._targetKingdom.AddTradeRoute(tradeRoute);
    //        this._sourceKingdom.UpdateAllCitiesDailyGrowth();
    //        this._targetKingdom.UpdateAllCitiesDailyGrowth();
    //        Debug.Log("Trade was successful " + this._sourceKingdom.name + " gained GOLD. " + this._targetKingdom.name + " gained " + resourceToTrade.ToString());
    //    } else {
    //        Debug.Log(this._sourceKingdom.name + " and " + this._targetKingdom.name + " are no longer elligible for trade!");
    //    }
    //    this.DoneEvent();
    //}
}
