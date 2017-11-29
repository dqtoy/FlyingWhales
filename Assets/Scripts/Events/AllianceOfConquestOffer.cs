using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AllianceOfConquestOffer : GameEvent {

    private Kingdom offeredToKingdom;
    private Kingdom conquestTarget;
    private Kingdom offeringKingdom;

    private List<Kingdom> offerAllianceTo;

    public AllianceOfConquestOffer(int startDay, int startMonth, int startYear, Citizen startedBy, Kingdom offeringKingdom, Kingdom offeredToKingdom, Kingdom conquestTarget) 
        : base(startDay, startMonth, startYear, startedBy) {
        this.offeredToKingdom = offeredToKingdom;
        this.conquestTarget = conquestTarget;
        this.offeringKingdom = offeringKingdom;

        offerAllianceTo = new List<Kingdom>();
        if(offeredToKingdom.alliancePool != null) {
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
            if(chosenResponse == RESPONSE.REJECT) {
                OfferRejected();
                return;
            }
        }
        OfferAccepted();
    }

    private void OfferRejected() {
        Debug.Log(offeringKingdom.name + "'s alliance of conquest offer was rejected");
        for (int i = 0; i < offerAllianceTo.Count; i++) {
            Kingdom rejectedBy = offerAllianceTo[i];
            offeringKingdom.AddRejectedOffer(rejectedBy, WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST);
            rejectedBy.AddRejectedOffer(offeringKingdom, WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST);
        }
        DoneEvent();
    }

    private void OfferAccepted() {
        Debug.Log(offeringKingdom.name + "'s alliance of conquest offer was accepted by all alliance members");
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
        KingdomRelationship relWithConquestTarget = otherKingdom.GetRelationshipWithKingdom(conquestTarget);
        KingdomRelationship offeringRelWithOfferedKingdom = offeringKingdom.GetRelationshipWithKingdom(otherKingdom);
        int totalAcceptWeight = GetAcceptanceWeightFor(otherKingdom, relWithOfferingKingdom, relWithConquestTarget, offeringRelWithOfferedKingdom);
        int totalRejectWeight = GetRejectionWeightFor(otherKingdom, relWithOfferingKingdom, relWithConquestTarget, offeringRelWithOfferedKingdom);
        responseWeights.Add(RESPONSE.ACCEPT, totalAcceptWeight);
        responseWeights.Add(RESPONSE.REJECT, totalRejectWeight);
        return responseWeights;
    }
    private int GetAcceptanceWeightFor(Kingdom otherKingdom, KingdomRelationship relWithOfferingKingdom, KingdomRelationship relWithConquestTarget, KingdomRelationship offeringRelWithOfferedKingdom) {
        int acceptanceWeight = 0;
        //base
        if (relWithOfferingKingdom.totalLike > 0) {
            //+2 Weight on Accept for each positive Opinion towards Deal Source.
            acceptanceWeight += 2 * relWithOfferingKingdom.totalLike;
        }
        if (relWithConquestTarget.totalLike < 0) {
            //+3 Weight on Accept for each negative Opinion towards Conquest Target.
            acceptanceWeight += Mathf.Abs(3 * relWithConquestTarget.totalLike);
        }

        if (otherKingdom.king.HasTrait(TRAIT.IMPERIALIST)) {
            //+10 Weight on Accept
            acceptanceWeight += 10;
        }
        if (otherKingdom.king.HasTrait(TRAIT.HOSTILE)) {
            //+10 Weight on Accept
            acceptanceWeight += 10;
        }
        if (otherKingdom.king.HasTrait(TRAIT.MILITANT)) {
            //+5 Weight on Accept
            acceptanceWeight += 5;
        }
        if (otherKingdom.king.HasTrait(TRAIT.OPPORTUNIST)) {
            //+2 Weight on Accept for each positive point of Relative Strength of the Deal Source
            if(offeringRelWithOfferedKingdom._relativeStrength > 0) {
                acceptanceWeight += 2 * offeringRelWithOfferedKingdom._relativeStrength;
            }
        }
        return acceptanceWeight;
    }
    private int GetRejectionWeightFor(Kingdom otherKingdom, KingdomRelationship relWithOfferingKingdom, KingdomRelationship relWithConquestTarget, KingdomRelationship offeringRelWithOfferedKingdom) {
        int rejectionWeight = 0;
        //base
        if (relWithOfferingKingdom.totalLike < 0) {
            //+2 Weight on Reject for each negative Opinion towards Deal Source.
            rejectionWeight += Mathf.Abs(2 * relWithOfferingKingdom.totalLike);
        }
        if (relWithConquestTarget.totalLike > 0) {
            //+3 Weight on Reject for each positive Opinion towards Conquest Target.
            rejectionWeight += 3 * relWithConquestTarget.totalLike;
        }

        if (otherKingdom.king.HasTrait(TRAIT.BENEVOLENT)) {
            //+20 Weight on Reject
            rejectionWeight += 20;
        }
        if (otherKingdom.king.HasTrait(TRAIT.PACIFIST)) {
            //+30 Weight on Reject
            rejectionWeight += 30;
        }
        if (otherKingdom.king.HasTrait(TRAIT.OPPORTUNIST)) {
            //+2 Weight on Reject for each negative point of Relative Strength of the Deal Source
            if (offeringRelWithOfferedKingdom._relativeStrength < 0) {
                rejectionWeight += Mathf.Abs(2 * offeringRelWithOfferedKingdom._relativeStrength);
            }
        }
        return rejectionWeight;
    }
}
