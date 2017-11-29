using UnityEngine;
using System.Collections;

public class Benevolent : Trait {

    internal override int GetLeaveTradeDealWeightModification(Kingdom otherKingdom) {
        return -40; //subtract 40 from Default Weight
    }
}
