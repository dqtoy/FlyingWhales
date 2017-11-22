using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Imperialist : Trait {

    public Imperialist(Citizen ownerOfTrait) : base(ownerOfTrait) {}

    internal override Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = base.GetWarOfConquestTargetWeights();
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        //loop through non-ally adjacent kingdoms i am not at war with
        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            if (!currRel.AreAllies() && !currRel.isAtWar) {
                KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                //compare its theoretical power vs my theoretical power, if my theoretical power is higher
                if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
                    int weight = 50; //add 50 base weight
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
        return targetWeights;
    }
}
