using UnityEngine;
using System.Collections;

public class Scheming : Trait {

    internal override int GetInciteUnrestWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;
        //add 2 to Default Weight for each positive point of Relative Strength the kingdom has over me
        KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        if(relSourceWithOther.relativeStrength > 0) {
            weight += 2 * relSourceWithOther.relativeStrength;
        }
        return weight;
    }
}
