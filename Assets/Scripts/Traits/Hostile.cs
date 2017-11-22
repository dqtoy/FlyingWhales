using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hostile : Trait {

    public Hostile(Citizen ownerOfTrait) : base(ownerOfTrait) { }

    internal override Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = base.GetWarOfConquestTargetWeights();
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        //if i am not at war, loop through non-ally adjacent kingdoms i am not at war with
        int warCount = sourceKingdom.GetWarCount();
        if(warCount <= 0) {
            for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
                Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
                KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
                if (!currRel.AreAllies()) {
                    KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                    if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
                        int weight = 0; 
                        //5 weight per 1% of my theoretical power over his
                        float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
                        if (theoreticalPowerPercent > 0) {
                            weight += 5 * (int)theoreticalPowerPercent;
                        }
                        if(currRel.totalLike < 0) {
                            //add 2 weight per negative opinion
                            weight += Mathf.Abs(currRel.totalLike * 2);
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
        }
        return targetWeights;
    }
}
