using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Imperialist : Trait {

    internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        int weight = 50;
        //loop through non-ally adjacent kingdoms i am not at war with
        if (currRel.isAdjacent && !currRel.isAtWar && !currRel.AreAllies()) {
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
    internal override int GetAllianceOfConquestWeightModification(Kingdom otherKingdom, Kingdom causingKindom) {
        //otherKingdom is possible ally
        int weight = 0;
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        KingdomRelationship relOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);

        KingdomRelationship relSourceWithAdj = sourceKingdom.GetRelationshipWithKingdom(causingKindom);
        KingdomRelationship relOtherWithAdj = otherKingdom.GetRelationshipWithKingdom(causingKindom);
        //for each non-ally adjacent kingdom that have negative Relative Strength
        if (!relSourceWithAdj.AreAllies() && relSourceWithAdj._relativeStrength < 0) {
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
}
