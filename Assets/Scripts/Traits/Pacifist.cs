using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pacifist : Trait {

    public Pacifist(Citizen ownerOfTrait) : base(ownerOfTrait) {}

    internal override Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = base.GetWarOfConquestTargetWeights();
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        //loop through adjacent kingdoms i am not at war with:
        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            if (!currRel.isAtWar) {
                KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {//compare its theoretical power vs my theoretical power
                    int weight = -10; //subtract 10 base weight and 
                    //subtract 5 weight per 1% of his theoretical power over mine
                    float theoreticalPowerPercent = otherKingdomRelTowardsSource.GetTheoreticalPowerAdvantageOverTarget();
                    if (theoreticalPowerPercent > 0) {
                        weight -= 5 * (int)theoreticalPowerPercent;
                    }
                    if (currRel.totalLike > 0) {
                        //subtract 2 per positive opinion
                        weight -= Mathf.Abs(currRel.totalLike * 2);
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
