using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deceitful : Trait {

    internal override Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = base.GetWarOfConquestTargetWeights();
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            //loop through non-ally adjacent kingdoms i am not at war with
            if (currRel.AreAllies()) {
                if (otherKingdom.GetWarCount() > 0) {//if any of them is at war with another kingdom
                    //compare its theoretical power vs my theoretical power, if my theoretical power is higher
                    KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                    if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
                        int weight = 50; //add 50 base weight
                        //5 weight per 1% of my theoretical power over his
                        float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
                        if (theoreticalPowerPercent > 0) {
                            weight += 5 * (int)theoreticalPowerPercent;
                        }
                        weight -= 30 * sourceKingdom.GetWarCount();
                        if (targetWeights.ContainsKey(otherKingdom)) {
                            int existingWeight = targetWeights[otherKingdom];
                            weight += existingWeight;
                            weight = Mathf.Max(0, weight); //minimum 0
                            targetWeights[otherKingdom] = weight;
                        } else {
                            targetWeights.Add(otherKingdom, weight);
                        }
                    }
                }
            }
        }
        return targetWeights;
    }
    internal virtual Dictionary<Kingdom, Dictionary<Kingdom, int>> GetAllianceOfConquestTargetWeights() {
        Dictionary<Kingdom, Dictionary<Kingdom, int>> kingdomWeights = new Dictionary<Kingdom, Dictionary<Kingdom, int>>();
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        //for each non-ally adjacent kingdom that have negative Relative Strength
        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            if(!relWithOtherKingdom.AreAllies() && relWithOtherKingdom._relativeStrength < 0) {
                //loop through other kingdoms or alliances adjacent to it
                Dictionary<Kingdom, int> offerAllianceTo = new Dictionary<Kingdom, int>();
                for (int j = 0; j < otherKingdom.adjacentKingdoms.Count; j++) {
                    Kingdom adjacentKingdomOfOtherKingdom = otherKingdom.adjacentKingdoms[j];
                    KingdomRelationship relOfOtherKingdomWithAdj = otherKingdom.GetRelationshipWithKingdom(adjacentKingdomOfOtherKingdom);
                    if (!relOfOtherKingdomWithAdj.AreAllies()) {
                        int weight = 0;
                        KingdomRelationship relOfSourceKingdomWithAdj = sourceKingdom.GetRelationshipWithKingdom(adjacentKingdomOfOtherKingdom);
                        if(relOfSourceKingdomWithAdj.totalLike > 0) {
                            weight += 3 * relOfSourceKingdomWithAdj.totalLike; //add 3 Weight per positive opinion i have towards each alliance members
                        } else if(relOfSourceKingdomWithAdj.totalLike < 0) {
                            weight += 2 * relOfSourceKingdomWithAdj.totalLike; //subtract 2 Weight per negative opinion i have towards each alliance members
                        }
                        KingdomRelationship relOfAdjKingdomWithSource = adjacentKingdomOfOtherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                        if (relOfAdjKingdomWithSource.totalLike > 0) {
                            weight += 3 * relOfAdjKingdomWithSource.totalLike; //add 3 Weight per positive opinion each alliance member has towards me
                        }
                        KingdomRelationship relOfAdjKingdomWithOther = adjacentKingdomOfOtherKingdom.GetRelationshipWithKingdom(otherKingdom);
                        if (relOfAdjKingdomWithOther.totalLike > 0) {
                            weight -= 2 * relOfAdjKingdomWithOther.totalLike; //subtract 2 Weight per positive opinion each alliance member has towards Conquest Target
                        } else if (relOfAdjKingdomWithOther.totalLike < 0) {
                            weight += Mathf.Abs(2 * relOfAdjKingdomWithOther.totalLike); //add 2 Weight per negative opinion each alliance member has towards Conquest Target
                        }

                        //subtract 50 Weight if an Alliance or Trade Deal between the two has recently been rejected by the target or 
                        //if either side has recently broken an Alliance or Trade Deal
                        weight = Mathf.Max(0, weight); //minimum 0
                        offerAllianceTo.Add(adjacentKingdomOfOtherKingdom, weight);
                    }
                }
                kingdomWeights.Add(otherKingdom, offerAllianceTo);
            }
        }
        return kingdomWeights;
    }
}
