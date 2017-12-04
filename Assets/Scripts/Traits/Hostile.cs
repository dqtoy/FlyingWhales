using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hostile : Trait {

   // internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
   //     Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
   //     int weight = 0;
   //     //if i am not at war, loop through non-ally adjacent kingdoms i am not at war with
   //     int warCount = sourceKingdom.GetWarCount();
   //     if(warCount <= 0) {
   //         KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
			//if (currRel.sharedRelationship.isAdjacent && !currRel.sharedRelationship.isAtWar && !currRel.AreAllies()) {
   //             KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
   //             if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {
   //                 //5 weight per 1% of my theoretical power over his
   //                 float theoreticalPowerPercent = currRel.GetTheoreticalPowerAdvantageOverTarget();
   //                 if (theoreticalPowerPercent > 0) {
   //                     weight += 5 * (int)theoreticalPowerPercent;
   //                 }
   //                 if(currRel.totalLike < 0) {
   //                     //add 2 weight per negative opinion
   //                     weight += Mathf.Abs(currRel.totalLike * 2);
   //                 }
   //             }
   //         }
   //     }
   //     return weight;
   // }

    #region Leave Alliance
    internal override int GetLeaveAllianceWeightModification(AlliancePool alliance) {
        int weight = 0;
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
            Kingdom ally = alliance.kingdomsInvolved[i];
            if (ally.id != sourceKingdom.id) {
                KingdomRelationship sourceRelWithAlly = sourceKingdom.GetRelationshipWithKingdom(ally);
                if(sourceRelWithAlly.totalLike < 0) {
                    weight += Mathf.Abs(2 * sourceRelWithAlly.totalLike); //add 2 weight to leave alliance for every negative opinion I have towards the king
                }
            }
        }
        return weight;
    }
    internal override int GetKeepAllianceWeightModification(AlliancePool alliance) {
        int weight = 0;
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        List<Warfare> activeWars = sourceKingdom.GetAllActiveWars();
        for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
            Kingdom ally = alliance.kingdomsInvolved[i];
            if (ally.id != sourceKingdom.id) {
                List<Warfare> activeWarsOfAlly = ally.GetAllActiveWars();
                for (int j = 0; j < activeWarsOfAlly.Count; j++) {
                    Warfare currWar = activeWarsOfAlly[j];
                    if (activeWars.Contains(currWar)) {
                        weight += 20; //add 20 weight to keep alliance for every active war of other kingdoms withinin alliance
                    }
                }
                
            }
        }
        return weight;
    }
    #endregion

    internal override int GetLeaveTradeDealWeightModification(Kingdom otherKingdom) {
        return -30; //add 30 to Default Weight
    }
}
