using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Imperialist : Trait {

    internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        int weight = 50;
        //loop through non-ally adjacent kingdoms i am not at war with
		if (currRel.sharedRelationship.isAdjacent && !currRel.sharedRelationship.isAtWar && !currRel.AreAllies()) {
            KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
            //compare its theoretical power vs my theoretical power, if my theoretical power is higher
            if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
                weight = 50; //add 50 base weight
                //5 weight per 1% of my theoretical power over his
                float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
                if (theoreticalPowerPercent > 0) {
                    weight += 5 * (int)theoreticalPowerPercent;
                }
                weight -= 30 * sourceKingdom.GetWarCount(); //subtract 30 weight per active wars I have

                if (currRel.totalLike != 0) {
                    //subtract 2 per 1 positive opinion
                    //add 2 per 1 negative opinion
                    weight += (currRel.totalLike * 2) * -1;
                }
            }
        }
        return weight;
    }
    internal override int GetAllianceOfConquestWeightModification(Kingdom otherKingdom) {
        //otherKingdom is possible ally
        int weight = 0;
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        KingdomRelationship relOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
        for (int i = 0; i < otherKingdom.adjacentKingdoms.Count; i++) {
            Kingdom adjKingdom = otherKingdom.adjacentKingdoms[i];
            if (sourceKingdom.adjacentKingdoms.Contains(adjKingdom)) {
                KingdomRelationship relSourceWithAdj = sourceKingdom.GetRelationshipWithKingdom(adjKingdom);
                KingdomRelationship relOtherWithAdj = otherKingdom.GetRelationshipWithKingdom(adjKingdom);
                //for each non-ally adjacent kingdom that have negative Relative Strength
                if (!relSourceWithAdj.AreAllies() && relSourceWithAdj.relativeStrength < 0) {
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
                    //TODO: subtract 50 Weight if an Alliance or Trade Deal between the two has recently been rejected by the target or 
                    //if either side has recently broken an Alliance or Trade Deal
                    weight = Mathf.Max(0, weight); //minimum 0
                }
            }
        }

        return weight;
    }

    //    for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
    //        Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
    //        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
    //        if (!relWithOtherKingdom.AreAllies() && relWithOtherKingdom._relativeStrength < 0) {
    //            //loop through other kingdoms or alliances adjacent to it
    //            Dictionary<Kingdom, int> offerAllianceTo = new Dictionary<Kingdom, int>();
    //            for (int j = 0; j < otherKingdom.adjacentKingdoms.Count; j++) {
    //                Kingdom adjacentKingdomOfOtherKingdom = otherKingdom.adjacentKingdoms[j];
    //                KingdomRelationship relOfOtherKingdomWithAdj = otherKingdom.GetRelationshipWithKingdom(adjacentKingdomOfOtherKingdom);
    //                if (!relOfOtherKingdomWithAdj.AreAllies()) {
    //                    int weight = 0;
    //                    KingdomRelationship relOfSourceKingdomWithAdj = sourceKingdom.GetRelationshipWithKingdom(adjacentKingdomOfOtherKingdom);
    //                    if (relOfSourceKingdomWithAdj.totalLike > 0) {
    //                        weight += 3 * relOfSourceKingdomWithAdj.totalLike; //add 3 Weight per positive opinion i have towards each alliance members
    //                    } else if (relOfSourceKingdomWithAdj.totalLike < 0) {
    //                        weight += 2 * relOfSourceKingdomWithAdj.totalLike; //subtract 2 Weight per negative opinion i have towards each alliance members
    //                    }
    //                    KingdomRelationship relOfAdjKingdomWithSource = adjacentKingdomOfOtherKingdom.GetRelationshipWithKingdom(sourceKingdom);
    //                    if (relOfAdjKingdomWithSource.totalLike > 0) {
    //                        weight += 3 * relOfAdjKingdomWithSource.totalLike; //add 3 Weight per positive opinion each alliance member has towards me
    //                    }
    //                    KingdomRelationship relOfAdjKingdomWithOther = adjacentKingdomOfOtherKingdom.GetRelationshipWithKingdom(otherKingdom);
    //                    if (relOfAdjKingdomWithOther.totalLike > 0) {
    //                        weight -= 2 * relOfAdjKingdomWithOther.totalLike; //subtract 2 Weight per positive opinion each alliance member has towards Conquest Target
    //                    } else if (relOfAdjKingdomWithOther.totalLike < 0) {
    //                        weight += Mathf.Abs(2 * relOfAdjKingdomWithOther.totalLike); //add 2 Weight per negative opinion each alliance member has towards Conquest Target
    //                    }

    //                    //TODO: subtract 50 Weight if an Alliance or Trade Deal between the two has recently been rejected by the target or 
    //                    //if either side has recently broken an Alliance or Trade Deal
    //                    weight = Mathf.Max(0, weight); //minimum 0
    //                    offerAllianceTo.Add(adjacentKingdomOfOtherKingdom, weight);
    //                }
    //            }
    //            kingdomWeights.Add(otherKingdom, offerAllianceTo);
    //        }
    //    }
    //    return kingdomWeights;

}
