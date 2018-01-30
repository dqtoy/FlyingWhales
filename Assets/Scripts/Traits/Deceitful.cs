using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deceitful : Trait {

    #region International Incidents
    internal override WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentReactionWeight(INTERNATIONAL_INCIDENT_TYPE incidentType,
        FactionRelationship rel, Faction aggressor) {
        if (rel.relationshipStatus == RELATIONSHIP_STATUS.FRIENDLY) {
            WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
            actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, -50); //Subtract 50 Weight to Do Nothing
            int relativeStr = rel.factionLookup[_ownerOfTrait.faction.id].relativeStrength;
            if (relativeStr > 0) {
                actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 3 * relativeStr); //Check Relative Strength, add 3 Weight to Declare War for each Positive Point of Relative Strength
            }

            return actionWeights;
        }

        return null;
    }
    #endregion

    #region War
    internal override WeightedDictionary<ALLY_WAR_REACTION> GetAllyReactionWeight(Faction friend, Faction enemy) {
        WeightedDictionary<ALLY_WAR_REACTION> actionWeights = new WeightedDictionary<ALLY_WAR_REACTION>();
        FactionRelationship relWithFriend = _ownerOfTrait.faction.GetRelationshipWith(friend);
        
        int relativeStr = relWithFriend.factionLookup[_ownerOfTrait.faction.id].relativeStrength;
        if (relativeStr > 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.BETRAY, 2 * relativeStr); //+2 Weight to Betray for every positive point of Relative Strength I have over the ally
        }
        if (relWithFriend.sharedOpinion < 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.BETRAY, Mathf.Abs(relWithFriend.sharedOpinion)); //+1 Weight to Betray for every Negative Opinion I have towards the ally
        }
        return actionWeights;
    }
    #endregion

    //internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
    //    Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
    //    KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
    //    int weight = 0;
    //    //loop through adjacent allies
    //    if (!currRel.sharedRelationship.isAtWar && currRel.AreAllies()) {
    //        if (otherKingdom.GetWarCount() > 0) {//if any of them is at war with another kingdom
    //            //compare its theoretical power vs my theoretical power, if my theoretical power is higher
    //            KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
    //            if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
    //                weight = 50; //add 50 base weight
    //                //5 weight per 1% of my theoretical power over his
    //                float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
    //                if (theoreticalPowerPercent > 0) {
    //                    weight += 5 * (int)theoreticalPowerPercent;
    //                }
    //                weight -= 30 * sourceKingdom.GetWarCount();
    //                weight = Mathf.Max(0, weight);
    //            }
    //        }
    //    }
    //    return weight;
    //}
    internal override int GetAllianceOfProtectionWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;
        if (sourceKingdom.IsThreatened()) {
            //loop through known Kingdoms i am not at war with and whose Opinion of me is positive
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            KingdomRelationship relOfOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
			if (!relWithOtherKingdom.sharedRelationship.isAtWar && relOfOtherWithSource.totalLike > 0) {
                weight += 2 * relOfOtherWithSource.totalLike; //add 2 Weight for every positive Opinion it has towards me
                weight = Mathf.Max(0, weight); //minimum 0
            }
        }
        return weight;
    }
    //internal override int GetInciteUnrestWeightModification(Kingdom otherKingdom) {
    //    Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
    //    int weight = 0;

    //    KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
    //    //if ally
    //    if (relWithOtherKingdom.AreAllies()) {
    //        //add Default Weight per Negative Opinion I have towards target
    //        if(relWithOtherKingdom.totalLike < 0) {
    //            weight += Mathf.Abs(relWithOtherKingdom.totalLike);
    //        }
    //    }
    //    return weight;
    //}
    internal override int GetFlatterWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;

        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        if(relWithOtherKingdom.totalLike < 0) {
            weight += Mathf.Abs(2 * relWithOtherKingdom.totalLike); //add 2 to Default Weight for each negative point of Opinion I have towards the target
        }
        return weight;
    }
	internal override int GetInternationalIncidentReactionWeight (InternationalIncident.INCIDENT_ACTIONS incidentAction, KingdomRelationship kr){
		if (kr.AreAllies ()) {
			if (incidentAction == InternationalIncident.INCIDENT_ACTIONS.RESOLVE_PEACEFULLY) {
				return 50;
			}else if (incidentAction == InternationalIncident.INCIDENT_ACTIONS.INCREASE_TENSION) {
				KingdomRelationship rk = kr.targetKingdom.GetRelationshipWithKingdom (kr.sourceKingdom);
				if(kr._theoreticalPower > rk._theoreticalPower && kr.targetKingdom.HasWar(kr.sourceKingdom)){
					return (20 * rk.relativeStrength);
				}
			}
		}
		return 0;
	}
}
