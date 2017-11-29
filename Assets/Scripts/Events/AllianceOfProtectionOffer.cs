using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AllianceOfProtectionOffer : GameEvent {

    private Kingdom offeredToKingdom;
    private Kingdom offeringKingdom;

    private List<Kingdom> offerAllianceTo;

    public AllianceOfProtectionOffer(int startDay, int startMonth, int startYear, Citizen startedBy, Kingdom offeringKingdom, Kingdom offeredToKingdom) 
        : base(startDay, startMonth, startYear, startedBy) {

        this.offeredToKingdom = offeredToKingdom;
        this.offeringKingdom = offeringKingdom;

        offerAllianceTo = new List<Kingdom>();
        if (offeredToKingdom.alliancePool != null) {
            offerAllianceTo.AddRange(offeredToKingdom.alliancePool.kingdomsInvolved);
        } else {
            offerAllianceTo.Add(offeredToKingdom);
        }
        CheckIfKingdomsWillAcceptOffer(offerAllianceTo);
    }

    private void CheckIfKingdomsWillAcceptOffer(List<Kingdom> kingdomsOfferedTo) {
        for (int i = 0; i < kingdomsOfferedTo.Count; i++) {
            Kingdom currKingdomOfferedTo = kingdomsOfferedTo[i];
            Dictionary<RESPONSE, int> responseWeight = GetResponseWeights(currKingdomOfferedTo);
            RESPONSE chosenResponse = Utilities.PickRandomElementWithWeights(responseWeight);
            if (chosenResponse == RESPONSE.REJECT) {
                OfferRejected();
                return;
            }
        }
        OfferAccepted();
    }

    private void OfferRejected() {
        Debug.Log(offeringKingdom.name + "'s alliance of protection offer was rejected");
        for (int i = 0; i < offerAllianceTo.Count; i++) {
            Kingdom rejectedBy = offerAllianceTo[i];
            offeringKingdom.AddRejectedOffer(rejectedBy, WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION);
            rejectedBy.AddRejectedOffer(offeringKingdom, WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION);
        }
        DoneEvent();
    }

    private void OfferAccepted() {
        Debug.Log(offeringKingdom.name + "'s alliance of protection offer was accepted by all alliance members");
        if (offeredToKingdom.alliancePool == null) {
            KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(offeringKingdom, offeredToKingdom);
        } else {
            offeredToKingdom.alliancePool.AddKingdomInAlliance(offeringKingdom);
        }
        DoneEvent();
    }

    private Dictionary<RESPONSE, int> GetResponseWeights(Kingdom otherKingdom) {
        Dictionary<RESPONSE, int> responseWeights = new Dictionary<RESPONSE, int>();
        KingdomRelationship relWithOfferingKingdom = otherKingdom.GetRelationshipWithKingdom(offeringKingdom);
        KingdomRelationship offeringRelWithOfferedKingdom = offeringKingdom.GetRelationshipWithKingdom(otherKingdom);
        int totalAcceptWeight = GetAcceptanceWeightFor(otherKingdom, relWithOfferingKingdom, offeringRelWithOfferedKingdom);
        int totalRejectWeight = GetRejectionWeightFor(otherKingdom, relWithOfferingKingdom, offeringRelWithOfferedKingdom);
        responseWeights.Add(RESPONSE.ACCEPT, totalAcceptWeight);
        responseWeights.Add(RESPONSE.REJECT, totalRejectWeight);
        return responseWeights;
    }
    private int GetAcceptanceWeightFor(Kingdom otherKingdom, KingdomRelationship relWithOfferingKingdom, KingdomRelationship offeringRelWithOfferedKingdom) {
        int acceptanceWeight = 0;
        //base
        if (relWithOfferingKingdom.totalLike > 0) {
            //+3 Weight on Accept for each positive Opinion towards Deal Source.
            acceptanceWeight += 3 * relWithOfferingKingdom.totalLike;
        }

        if (otherKingdom.king.HasTrait(TRAIT.BENEVOLENT)) {
            //+1 Weight on Accept for each negative point of Relative Strength of the Deal Source
            if(offeringRelWithOfferedKingdom.relativeStrength < 0) {
                acceptanceWeight += Mathf.Abs(offeringRelWithOfferedKingdom.relativeStrength);
            }
        }
        if (otherKingdom.king.HasTrait(TRAIT.DIPLOMATIC)) {
            //+20 Weight on Accept
            acceptanceWeight += 20;
        }
        if (otherKingdom.king.HasTrait(TRAIT.PACIFIST)) {
            //+50 Weight on Accept
            acceptanceWeight += 50;
        }
        return acceptanceWeight;
    }
    private int GetRejectionWeightFor(Kingdom otherKingdom, KingdomRelationship relWithOfferingKingdom, KingdomRelationship offeringRelWithOfferedKingdom) {
        int rejectionWeight = 0;
        //base
        if (relWithOfferingKingdom.totalLike < 0) {
            //+3 Weight on Reject for each negative Opinion towards Deal Source.
            rejectionWeight += Mathf.Abs(3 * relWithOfferingKingdom.totalLike);
        }
        if (otherKingdom.king.HasTrait(TRAIT.HOSTILE)) {
            //+50 Weight on Reject
            rejectionWeight += 50;
        }
        return rejectionWeight;
    }
}
