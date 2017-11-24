using UnityEngine;
using System.Collections;

public class Diplomatic : Trait {

    internal override int GetFlatterWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;

        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        //add 2 to Default Weight for each positive point of Opinion I have towards the target
        if(relWithOtherKingdom.totalLike > 0) {
            weight += 2 * relWithOtherKingdom.totalLike;
        }
        
        return weight;
    }
}
