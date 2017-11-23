using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Opportunist : Trait {

    internal override Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = new Dictionary<Kingdom, int>();
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            //loop through non-ally adjacent kingdoms i am not at war with
            if(!currRel.AreAllies() && !currRel.isAtWar) {
                if (otherKingdom.GetWarCount() > 0) {//if any of them is at war with another kingdom
                    //compare its theoretical power vs my theoretical power, if my theoretical power is higher
                    KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                    if(currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
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
    internal override Dictionary<Kingdom, int> GetAllianceOfProtectionTargetWeights() {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        if (!sourceKingdom.IsThreatened()) {
            return null;
        }
        Dictionary<Kingdom, int> targetWeights = new Dictionary<Kingdom, int>();
        //loop through known Kingdoms with the highest Relative Strength and only select one with positive Relative Strength and not at war with
        for (int i = 0; i < sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.discoveredKingdoms[i];
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            KingdomRelationship relOfOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
            if (!relWithOtherKingdom.isAtWar && relOfOtherWithSource._relativeStrength > 0) {
                int weight = 0;
                weight += 5 * relOfOtherWithSource._relativeStrength;//add 5 Weight for every positive Relative Strength point of the kingdom
                //add 2 Weight for every positive Opinion it has towards me
                //subtract 1 Weight for every negative Opinion it has towards me
                if(relOfOtherWithSource.totalLike > 0) {
                    weight += 2 * relOfOtherWithSource.totalLike;
                } else if(relOfOtherWithSource.totalLike < 0) {
                    weight += relOfOtherWithSource.totalLike;
                }

                //TODO: subtract 50 Weight if an Alliance or Trade Deal between the two has recently been rejected by the 
                //target or if either side has recently broken an Alliance or Trade Deal
                weight = Mathf.Max(0, weight); //minimum 0
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
        return targetWeights;
    }
}
