using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Opportunist : Trait {

    internal override Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = base.GetWarOfConquestTargetWeights();
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
}
