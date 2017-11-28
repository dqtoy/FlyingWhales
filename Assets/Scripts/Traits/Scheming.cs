using UnityEngine;
using System.Collections;

public class Scheming : Trait {

    internal override int GetInciteUnrestWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;
        //add 2 to Default Weight for each positive point of Relative Strength the kingdom has over me
        KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        KingdomRelationship relOtherWithSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
        int relativeStrSource = relSourceWithOther.relativeStrength;
        int relativeStrOther = relOtherWithSource.relativeStrength;
        int difference = relativeStrOther - relativeStrSource;
        if(difference > 0) {
            weight += difference;
        }
        return weight;
    }
}
