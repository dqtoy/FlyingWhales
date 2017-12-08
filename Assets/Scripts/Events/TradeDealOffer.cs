using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TradeDealOffer : GameEvent {

    private Kingdom offeredToKingdom;
    private Kingdom offeringKingdom;

    public TradeDealOffer(int startDay, int startMonth, int startYear, Citizen startedBy, Kingdom offeringKingdom, Kingdom offeredToKingdom) 
        : base(startDay, startMonth, startYear, startedBy) {
        this.eventType = EVENT_TYPES.TRADE_DEAL_OFFER;
        this.offeredToKingdom = offeredToKingdom;
        this.offeringKingdom = offeringKingdom;
        CheckIfKingdomWillAcceptOffer(offeredToKingdom);
    }

    private void CheckIfKingdomWillAcceptOffer(Kingdom kingdomOfferedTo) {
        Dictionary<RESPONSE, int> responseWeight = GetResponseWeights(kingdomOfferedTo);
        RESPONSE chosenResponse = Utilities.PickRandomElementWithWeights(responseWeight);
        if (chosenResponse == RESPONSE.REJECT) {
            OfferRejected();
            return;
        }
        OfferAccepted();
    }

    private void OfferRejected() {
        Debug.Log(offeringKingdom.name + "'s trade deal offer was rejected by " + offeredToKingdom.name);
        offeringKingdom.AddRejectedOffer(offeredToKingdom, WEIGHTED_ACTION.TRADE_DEAL);
        offeredToKingdom.AddRejectedOffer(offeringKingdom, WEIGHTED_ACTION.TRADE_DEAL);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "TradeDeal", "trade_deal_reject");
        newLog.AddToFillers(this.offeredToKingdom, this.offeredToKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(this.offeringKingdom, this.offeringKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom>() { this.offeredToKingdom, this.offeringKingdom });
        DoneEvent();
    }

    private void OfferAccepted() {
        Debug.Log(offeringKingdom.name + "'s trade deal offer was accepted by " + offeredToKingdom.name);
        KingdomManager.Instance.CreateTradeDeal(offeringKingdom, offeredToKingdom);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "TradeDeal", "trade_deal_accept");
        newLog.AddToFillers(this.offeredToKingdom, this.offeredToKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(this.offeringKingdom, this.offeringKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom>() { this.offeredToKingdom, this.offeringKingdom });
        DoneEvent();
    }

    private Dictionary<RESPONSE, int> GetResponseWeights(Kingdom otherKingdom) {
        Dictionary<RESPONSE, int> responseWeights = new Dictionary<RESPONSE, int>();
        int totalAcceptWeight = GetAcceptanceWeightFor(otherKingdom);
        int totalRejectWeight = GetRejectionWeightFor(otherKingdom);
        responseWeights.Add(RESPONSE.ACCEPT, totalAcceptWeight);
        responseWeights.Add(RESPONSE.REJECT, totalRejectWeight);
        return responseWeights;
    }

    private int GetAcceptanceWeightFor(Kingdom otherKingdom) {
        int acceptanceWeight = 0;
        //base
        //+10 Weight on Accept for every point of Surplus of the Deal Source that is a Deficit for me
        Dictionary<RESOURCE_TYPE, int> deficitOfOtherKingdom = otherKingdom.GetDeficitResourcesFor(offeringKingdom);
        Dictionary<RESOURCE_TYPE, int> surplusOfOfferingKingdom = offeringKingdom.GetSurplusResourcesFor(otherKingdom);
        foreach (KeyValuePair<RESOURCE_TYPE, int> kvp in surplusOfOfferingKingdom) {
            RESOURCE_TYPE currSurplus = kvp.Key;
            int surplusAmount = kvp.Value;
            if (deficitOfOtherKingdom.ContainsKey(currSurplus)) {
                //otherKingdom has a deficit for currSurplus
                int deficitAmount = deficitOfOtherKingdom[currSurplus];
                int modifier = 0;
                if (surplusAmount >= deficitAmount) {
                    modifier = deficitAmount;
                } else {
                    modifier = surplusAmount;
                }
                acceptanceWeight += 10 * modifier;

            }
        }
        if (otherKingdom.king.HasTrait(TRAIT.DIPLOMATIC)) {
            acceptanceWeight *= 2;
        }
        return acceptanceWeight;
    }
    private int GetRejectionWeightFor(Kingdom otherKingdom) {
        int rejectionWeight = 1;
        //base
        //+1 Weight on Reject for each negative Opinion towards Deal Source
        KingdomRelationship relWithOfferingKingdom = otherKingdom.GetRelationshipWithKingdom(offeringKingdom);
        if(relWithOfferingKingdom.totalLike < 0) {
            rejectionWeight += Mathf.Abs(relWithOfferingKingdom.totalLike);

            if (otherKingdom.king.HasTrait(TRAIT.HOSTILE)) {
                //+5 Weight on Reject for each negative Opinion towards Deal Source
                rejectionWeight += Mathf.Abs(5 * relWithOfferingKingdom.totalLike);
            }
        }

        return rejectionWeight;
    }
}
