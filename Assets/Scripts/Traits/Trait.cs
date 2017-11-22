using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class Trait{
    public string traitName;
    public TRAIT trait;
    public ActionWeight[] actionWeights;

    protected Citizen ownerOfTrait;


    public Trait(Citizen ownerOfTrait) {
        this.ownerOfTrait = ownerOfTrait;
    }

    /*
     * This will return a Dictionary, containing the weights of each
     * WEIGHTED_ACTION type.
     * */
    internal Dictionary<WEIGHTED_ACTION, int> GetTotalActionWeights() {
        WEIGHTED_ACTION[] allWeightedActions = Utilities.GetEnumValues<WEIGHTED_ACTION>();
        Dictionary<WEIGHTED_ACTION, int> totalWeights = new Dictionary<WEIGHTED_ACTION, int>();
        
        for (int i = 0; i < allWeightedActions.Length; i++) {
            WEIGHTED_ACTION currAction = allWeightedActions[i];
            int totalWeightOfAction = Mathf.Max(0, GetBaseWeightOfAction(currAction)); //So that the returned number can never be negative
            if(totalWeightOfAction > 0) {
                totalWeights.Add(currAction, totalWeightOfAction);
            }
        }
        return totalWeights;
    }

    //protected virtual int GetTotalWeightOfActionTowards(WEIGHTED_ACTION actionType, Kingdom targetKingdom) {
    //    switch (actionType) {
    //        case WEIGHTED_ACTION.DO_NOTHING:
    //            break;
    //        case WEIGHTED_ACTION.WAR_OF_CONQUEST:
    //            return GetWarOfConquestWeightTowards(targetKingdom);
    //        case WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST:
    //            break;
    //        case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
    //            break;
    //        case WEIGHTED_ACTION.TRADE_DEAL:
    //            break;
    //        case WEIGHTED_ACTION.INCITE_UNREST:
    //            break;
    //        case WEIGHTED_ACTION.PIT_OTHER_KINGDOMS:
    //            break;
    //        case WEIGHTED_ACTION.FLATTER:
    //            break;
    //        case WEIGHTED_ACTION.SEND_AID:
    //            break;
    //        default:
    //            break;
    //    }
    //    return 0;
    //}

    internal virtual Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        Dictionary<Kingdom, int> targetWeights = new Dictionary<Kingdom, int>();
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
            List<Kingdom> alliesAtWarWith = currRel.GetAlliesTargetKingdomIsAtWarWith();
            //for each non-ally adjacent kingdoms that one of my allies declared war with recently
            if (currRel.isAdjacent && !currRel.AreAllies() && alliesAtWarWith.Count > 0) {
                //compare its theoretical power vs my theoretical power
                int sourceKingdomPower = currRel._theoreticalPower;
                int otherKingdomPower = otherKingdom.GetRelationshipWithKingdom(sourceKingdom)._theoreticalPower;
                if (otherKingdomPower * 1.25f < sourceKingdomPower) {
                    //If his theoretical power is not higher than 25% over mine
                    int weightOfOtherKingdom = 20;
                    for (int j = 0; j < alliesAtWarWith.Count; j++) {
                        Kingdom currAlly = alliesAtWarWith[j];
                        KingdomRelationship relationshipWithAlly = sourceKingdom.GetRelationshipWithKingdom(currAlly);
                        if (relationshipWithAlly.totalLike > 0) {
                            weightOfOtherKingdom += 2 * relationshipWithAlly.totalLike; //add 2 weight per positive opinion i have over my ally
                        } else if (relationshipWithAlly.totalLike < 0) {
                            weightOfOtherKingdom += relationshipWithAlly.totalLike; //subtract 1 weight per negative opinion i have over my ally (totalLike is negative)
                        }
                    }
                    //add 1 weight per negative opinion i have over the target
                    //subtract 1 weight per positive opinion i have over the target
                    weightOfOtherKingdom += (currRel.totalLike * -1); //If totalLike is negative it becomes positive(+), otherwise it becomes negative(-)
                    weightOfOtherKingdom = Mathf.Max(0, weightOfOtherKingdom);
                    targetWeights.Add(otherKingdom, weightOfOtherKingdom);
                }
            }
        }
        return targetWeights;
    }

    protected int GetBaseWeightOfAction(WEIGHTED_ACTION actionType) {
        int baseWeight = 0;
        for (int i = 0; i < actionWeights.Length; i++) {
            ActionWeight currWeight = actionWeights[i];
            if(currWeight.actionType == actionType) {
                baseWeight += currWeight.weight;
            }
        }
        return baseWeight;
    }
}
