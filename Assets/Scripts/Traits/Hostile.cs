using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hostile : Trait {

    internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;
        //if i am not at war, loop through non-ally adjacent kingdoms i am not at war with
        int warCount = sourceKingdom.GetWarCount();
        if(warCount <= 0) {
            KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            if (currRel.isAdjacent && !currRel.isAtWar && !currRel.AreAllies()) {
                KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
                    //5 weight per 1% of my theoretical power over his
                    float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
                    if (theoreticalPowerPercent > 0) {
                        weight += 5 * (int)theoreticalPowerPercent;
                    }
                    if(currRel.totalLike < 0) {
                        //add 2 weight per negative opinion
                        weight += Mathf.Abs(currRel.totalLike * 2);
                    }
                }
            }
        }
        return weight;
    }
}
