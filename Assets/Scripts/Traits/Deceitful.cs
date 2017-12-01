using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deceitful : Trait {

    internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        int weight = 0;
        //loop through adjacent allies
        if (!currRel.sharedRelationship.isAtWar && currRel.AreAllies()) {
            if (otherKingdom.GetWarCount() > 0) {//if any of them is at war with another kingdom
                //compare its theoretical power vs my theoretical power, if my theoretical power is higher
                KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
                if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
                    weight = 50; //add 50 base weight
                    //5 weight per 1% of my theoretical power over his
                    float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
                    if (theoreticalPowerPercent > 0) {
                        weight += 5 * (int)theoreticalPowerPercent;
                    }
                    weight -= 30 * sourceKingdom.GetWarCount();
                    weight = Mathf.Max(0, weight);
                }
            }
        }
        return weight;
    }
    internal override int GetAllianceOfProtectionWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;
        if (sourceKingdom.IsThreatened()) {
            //loop through known Kingdoms i am not at war with and whose Opinion of me is positive
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            KingdomRelationship relOfOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
			if (!relWithOtherKingdom.sharedRelationship.isAtWar && relOfOtherWithSource.totalLike > 0) {
                if (relOfOtherWithSource.totalLike > 0) {
                    weight += 2 * relOfOtherWithSource.totalLike; //add 2 Weight for every positive Opinion it has towards me
                }
                weight = Mathf.Max(0, weight); //minimum 0
            }
        }
        return weight;
    }
    //internal override int GetInciteUnrestWeightModification(Kingdom otherKingdom) {
    //    Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
    //    int weight = 0;

    //    KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
    //    //if ally
    //    if (relWithOtherKingdom.AreAllies()) {
    //        //add Default Weight per Negative Opinion I have towards target
    //        if(relWithOtherKingdom.totalLike < 0) {
    //            weight += Mathf.Abs(relWithOtherKingdom.totalLike);
    //        }
    //    }
    //    return weight;
    //}
    internal override int GetFlatterWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;

        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        if(relWithOtherKingdom.totalLike < 0) {
            weight += Mathf.Abs(2 * relWithOtherKingdom.totalLike); //add 2 to Default Weight for each negative point of Opinion I have towards the target
        }
        return weight;
    }
}
