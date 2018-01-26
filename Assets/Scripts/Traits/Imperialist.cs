using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Imperialist : Trait {

    #region International Incidents
    internal override WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentReactionWeight(INTERNATIONAL_INCIDENT_TYPE incidentType,
        FactionRelationship rel, Faction aggressor) {
        if (rel.relationshipStatus == RELATIONSHIP_STATUS.NEUTRAL) {
            WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
            //TODO:- Check Relative Strength, add 2 Weight to Declare War for each Positive Point of Relative Strength
            //TODO:- Check Relative Strength, add 5 Weight to Do Nothing for each Negative Point of Relative Strength
            return actionWeights;
        }
        return null;
    }
    #endregion
    //  internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
    //      Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
    //      KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
    //      int weight = 0;
    //      //loop through non-ally adjacent kingdoms i am not at war with
    //if (currRel.sharedRelationship.isAdjacent && !currRel.sharedRelationship.isAtWar && !currRel.AreAllies()) {
    //          KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
    //          //compare its theoretical power vs my theoretical power, if my theoretical power is higher
    //          if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
    //              weight = 50; //add 50 base weight
    //              //5 weight per 1% of my theoretical power over his
    //              float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
    //              if (theoreticalPowerPercent > 0) {
    //                  weight += 5 * (int)theoreticalPowerPercent;
    //              }
    //              weight -= 30 * sourceKingdom.GetWarCount(); //subtract 30 weight per active wars I have

    //              if (currRel.totalLike != 0) {
    //                  //subtract 2 per 1 positive opinion
    //                  //add 2 per 1 negative opinion
    //                  weight += (currRel.totalLike * 2) * -1;
    //              }
    //          }
    //      }
    //      return weight;
    //  }
    internal override int GetAllianceOfConquestWeightModification(Kingdom otherKingdom, Kingdom causingKindom) {
        //otherKingdom is possible ally
        int weight = 0;
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        KingdomRelationship relOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);

        KingdomRelationship relSourceWithAdj = sourceKingdom.GetRelationshipWithKingdom(causingKindom);
        KingdomRelationship relOtherWithAdj = otherKingdom.GetRelationshipWithKingdom(causingKindom);
        //for each non-ally adjacent kingdom that have negative Relative Strength
        if (relSourceWithAdj.sharedRelationship.isAdjacent && relOtherWithAdj.sharedRelationship.isAdjacent && !relSourceWithAdj.AreAllies() 
            && relSourceWithAdj.relativeStrength < 0) {
            if (relSourceWithOther.totalLike > 0) {
                weight += 3 * relSourceWithOther.totalLike; //add 3 Weight per positive opinion i have towards each alliance members
            } else if (relSourceWithOther.totalLike < 0) {
                weight += 2 * relSourceWithOther.totalLike; //subtract 2 Weight per negative opinion i have towards each alliance members
            }
            if (relOtherWithSource.totalLike > 0) {
                weight += 3 * relOtherWithSource.totalLike; //add 3 Weight per positive opinion each alliance member has towards me
            }
            if (relOtherWithAdj.totalLike > 0) {
                weight -= 2 * relOtherWithAdj.totalLike; //subtract 2 Weight per positive opinion each alliance member has towards Conquest Target
            } else if (relOtherWithAdj.totalLike < 0) {
                weight += Mathf.Abs(2 * relOtherWithAdj.totalLike); //add 2 Weight per negative opinion each alliance member has towards Conquest Target
            }
            if (sourceKingdom.recentlyRejectedOffers.ContainsKey(otherKingdom)) {
                weight -= 50;
            } else if (sourceKingdom.recentlyBrokenAlliancesWith.Contains(otherKingdom)) {
                weight -= 50;
            }
            weight = Mathf.Max(0, weight); //minimum 0
        }

        return weight;
    }
	internal override int GetInternationalIncidentReactionWeight (InternationalIncident.INCIDENT_ACTIONS incidentAction, KingdomRelationship kr){
		if(!kr.AreAllies()){
			KingdomRelationship rk = kr.targetKingdom.GetRelationshipWithKingdom (kr.sourceKingdom);
			if(incidentAction == InternationalIncident.INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
				if(kr._theoreticalPower < rk._theoreticalPower){
					return 5 * kr.relativeStrength;
				}
			}else if(incidentAction == InternationalIncident.INCIDENT_ACTIONS.INCREASE_TENSION){
				if(kr._theoreticalPower > rk._theoreticalPower){
					return 10 * rk.relativeStrength;
				}
			}
		}
		return 0;
	}
	internal override int GetKingdomThreatOpinionChange(int threat, out string summary){
		summary = string.Empty;
		if(threat >= -50 && threat < -25){
			summary = "Preys on the Weak";
			return -15;
		}else if(threat > -100 && threat < -50){
			summary = "Preys on the Weak";
			return -30;
		}else if(threat <= -100){
			summary = "Preys on the Weak";
			return -50;
		}
		return 0;
	}
}
