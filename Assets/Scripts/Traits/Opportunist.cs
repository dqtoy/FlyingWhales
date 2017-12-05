using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Opportunist : Trait {

  //  internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
  //      Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
  //      KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
  //      int weight = 0;
  //      //loop through non-ally adjacent kingdoms i am not at war with
		//if (currRel.sharedRelationship.isAdjacent && !currRel.sharedRelationship.isAtWar && !currRel.AreAllies()) {
  //          if (otherKingdom.GetWarCount() > 0) {//if any of them is at war with another kingdom
  //              //compare its theoretical power vs my theoretical power, if my theoretical power is higher
  //              KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
  //              if(currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
  //                  weight = 50; //add 50 base weight
  //                  //5 weight per 1% of my theoretical power over his
  //                  float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
  //                  if (theoreticalPowerPercent > 0) {
  //                      weight += 5 * (int)theoreticalPowerPercent;
  //                  }
  //                  weight -= 30 * sourceKingdom.GetWarCount();
  //                  weight = Mathf.Max(0, weight);
  //              }
  //          }
  //      }
  //      return weight;
  //  }
    internal override int GetAllianceOfProtectionWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;
        if (sourceKingdom.IsThreatened()) {
            //loop through known Kingdoms with the highest Relative Strength and only select one with positive Relative Strength and not at war with
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            KingdomRelationship relOfOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
			if (!relWithOtherKingdom.sharedRelationship.isAtWar && relWithOtherKingdom.relativeStrength > 0) {
                weight = 0;
                weight += 5 * relWithOtherKingdom.relativeStrength;//add 5 Weight for every positive Relative Strength point of the kingdom
                //add 2 Weight for every positive Opinion it has towards me
                //subtract 1 Weight for every negative Opinion it has towards me
                if (relOfOtherWithSource.totalLike > 0) {
                    weight += 2 * relOfOtherWithSource.totalLike;
                } else if (relOfOtherWithSource.totalLike < 0) {
                    weight += relOfOtherWithSource.totalLike;
                }
                if (sourceKingdom.recentlyRejectedOffers.ContainsKey(otherKingdom)) {
                    weight -= 50;
                } else if (sourceKingdom.recentlyBrokenAlliancesWith.Contains(otherKingdom)) {
                    weight -= 50;
                }
                weight = Mathf.Max(0, weight); //minimum 0
            }
        }
        return weight;
    }
    internal override int GetFlatterWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;

        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        //add or subtract 3 to Default Weight for each positive or negative point of Relative Strength the kingdom has over me
        weight += 3 * relWithOtherKingdom.relativeStrength;
        return weight;
    }
}
