using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pacifist : Trait {

    internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;
        //loop through adjacent kingdoms i am not at war with:
        KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
		if (currRel.sharedRelationship.isAdjacent && !currRel.sharedRelationship.isAtWar) {
            KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
            if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {//compare its theoretical power vs my theoretical power
                weight = -10; //subtract 10 base weight and 
                //subtract 5 weight per 1% of his theoretical power over mine
                float theoreticalPowerPercent = otherKingdomRelTowardsSource.GetTheoreticalPowerAdvantageOverTarget();
                if (theoreticalPowerPercent > 0) {
                    weight -= 5 * (int)theoreticalPowerPercent;
                }
                if (currRel.totalLike > 0) {
                    //subtract 2 per positive opinion
                    weight -= Mathf.Abs(currRel.totalLike * 2);
                }
            }
        }
        return weight;
    }
}
