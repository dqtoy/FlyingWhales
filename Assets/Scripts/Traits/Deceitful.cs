using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deceitful : Trait {

    internal override Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = new Dictionary<Kingdom, int>();
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
    internal override Dictionary<Kingdom, int> GetAllianceOfProtectionTargetWeights() {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        if (!sourceKingdom.IsThreatened()) {
            return null;
        }
        Dictionary<Kingdom, int> targetWeights = new Dictionary<Kingdom, int>();
        //loop through known Kingdoms i am not at war with and whose Opinion of me is positive
        for (int i = 0; i < sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.discoveredKingdoms[i];
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            KingdomRelationship relOfOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
            if (!relWithOtherKingdom.isAtWar && relOfOtherWithSource.totalLike > 0) {
                int weight = 0;
                if (relOfOtherWithSource.totalLike > 0) {
                    weight += 2 * relOfOtherWithSource.totalLike; //add 2 Weight for every positive Opinion it has towards me
                }
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
