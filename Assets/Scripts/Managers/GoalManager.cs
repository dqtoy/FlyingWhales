using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoalManager : MonoBehaviour {

    public static GoalManager Instance = null;

    private void Awake() {
        Instance = this;
    }

    internal WEIGHTED_ACTION DetermineWeightedActionToPerform(Kingdom sourceKingdom) {
        Dictionary<WEIGHTED_ACTION, int> totalWeightedActions = new Dictionary<WEIGHTED_ACTION, int>();
        totalWeightedActions.Add(WEIGHTED_ACTION.DO_NOTHING, 50); //Add 500 Base Weight on Do Nothing Action
        for (int i = 0; i < sourceKingdom.king.allTraits.Count; i++) {
            Trait currTrait = sourceKingdom.king.allTraits[i];
            Dictionary<WEIGHTED_ACTION, int> weightsFromCurrTrait = currTrait.GetTotalActionWeights();
            totalWeightedActions = Utilities.MergeWeightedActionDictionaries(totalWeightedActions, weightsFromCurrTrait);
        }
        return Utilities.PickRandomElementWithWeights(totalWeightedActions);
    }

    internal Dictionary<Kingdom, int> GetKingdomWeightsForActionType(Kingdom sourceKingdom, WEIGHTED_ACTION weightedAction) {
        Dictionary<Kingdom, int> kingdomWeights = new Dictionary<Kingdom, int>();
        for (int i = 0; i < sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.discoveredKingdoms[i];
            int weightForOtherKingdom = GetDefaultWeightForAction(weightedAction, sourceKingdom, otherKingdom);
            //loop through all the traits of the current king
            for (int j = 0; j < sourceKingdom.king.allTraits.Count; j++) {
                Trait currTrait = sourceKingdom.king.allTraits[j];
                int modificationFromTrait = currTrait.GetWeightOfActionGivenTarget(weightedAction, otherKingdom, weightForOtherKingdom);
                weightForOtherKingdom += modificationFromTrait;
            }
            ApplyActionModificationForAll(weightedAction, sourceKingdom, otherKingdom, ref weightForOtherKingdom);
            kingdomWeights.Add(otherKingdom, weightForOtherKingdom);
        }
        return kingdomWeights;
    }
    internal Dictionary<Kingdom, Dictionary<Kingdom, int>> GetKingdomWeightsForSpecialActionType(Kingdom sourceKingdom, WEIGHTED_ACTION specialWeightedAction) {
        Dictionary<Kingdom, Dictionary<Kingdom, int>> kingdomWeights = new Dictionary<Kingdom, Dictionary<Kingdom, int>>();
        for (int i = 0; i < sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.discoveredKingdoms[i]; //the cause of the action
            Dictionary<Kingdom, int> possibleAllies = new Dictionary<Kingdom, int>();
            for (int j = 0; j < otherKingdom.adjacentKingdoms.Count; j++) {
                Kingdom adjKingdomOfOtherKingdom = otherKingdom.adjacentKingdoms[j]; //the target of the action
                if (adjKingdomOfOtherKingdom.id != sourceKingdom.id) {
                    int weightForOtherKingdom = GetDefaultWeightForAction(specialWeightedAction, sourceKingdom, otherKingdom);
                    //loop through all the traits of the current king
                    for (int k = 0; k < sourceKingdom.king.allTraits.Count; k++) {
                        Trait currTrait = sourceKingdom.king.allTraits[k];
                        int modificationFromTrait = currTrait.GetWeightOfActionGivenTargetAndCause(specialWeightedAction, adjKingdomOfOtherKingdom, otherKingdom, weightForOtherKingdom);
                        weightForOtherKingdom += modificationFromTrait;
                    }
                    ApplyActionModificationForAll(specialWeightedAction, sourceKingdom, otherKingdom, ref weightForOtherKingdom);
                    possibleAllies.Add(adjKingdomOfOtherKingdom, weightForOtherKingdom);
                }
            }
            kingdomWeights.Add(otherKingdom, possibleAllies);
        }
        return kingdomWeights;
    }
    private int GetDefaultWeightForAction(WEIGHTED_ACTION weightedAction, Kingdom sourceKingdom, Kingdom targetKingdom) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.WAR_OF_CONQUEST:
                return 0;
            case WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST:
                return 0;
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                return 0;
            case WEIGHTED_ACTION.TRADE_DEAL:
                return GetTradeDealDefaultWeight(sourceKingdom, targetKingdom);
            case WEIGHTED_ACTION.INCITE_UNREST:
                return GetInciteUnrestDefaultWeight(sourceKingdom, targetKingdom);
            case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
                return GetInternationalIncidentDefaultWeight(sourceKingdom, targetKingdom);
            case WEIGHTED_ACTION.FLATTER:
                return GetFlatterDefaultWeight(sourceKingdom, targetKingdom);
            case WEIGHTED_ACTION.SEND_AID:
                return 0;
            default:
                return 0;
        }
    }
    private void ApplyActionModificationForAll(WEIGHTED_ACTION weightedAction, Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.WAR_OF_CONQUEST:
                GetAllModificationForWarOfConquest(sourceKingdom, targetKingdom, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                GetAllModificationForAllianceOfProtection(sourceKingdom, targetKingdom, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.TRADE_DEAL:
                GetAllModificationForTradeDeal(sourceKingdom, targetKingdom, ref defaultWeight);
                break;
        }
    }

    #region Default Weights
    private int GetTradeDealDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
        int defaultWeight = 0;
        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);

        if (relWithOtherKingdom.sharedRelationship.isAdjacent) {
            defaultWeight = 40;
            if (relWithOtherKingdom.totalLike > 0) {
                defaultWeight += 2 * relWithOtherKingdom.totalLike;//add 2 to Default Weight per Positive Opinion I have towards target
            } else if (relWithOtherKingdom.totalLike < 0) {
                defaultWeight += 2 * relWithOtherKingdom.totalLike;//subtract 2 to Default Weight per Negative Opinion I have towards target
            }

            //add 1 to Default Weight per Positive Opinion target has towards me
            //subtract 1 to Default Weight per Negative Opinion target has towards me
            defaultWeight += relOfOtherWithSource.totalLike;
            defaultWeight = Mathf.Max(0, defaultWeight); //minimum 0

        }
        return defaultWeight;
    }
    private int GetInciteUnrestDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
        int defaultWeight = 0;
        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);

        if (!relWithOtherKingdom.AreAllies()) {
            defaultWeight = 40;
            if (relWithOtherKingdom.totalLike < 0) {
                defaultWeight += relWithOtherKingdom.totalLike;//subtract 2 to Default Weight per Negative Opinion I have towards target
            }
        }
        return defaultWeight;
    }
    private int GetInternationalIncidentDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
        int defaultWeight = 0;
        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        if (relWithOtherKingdom.totalLike < 0) {
            defaultWeight += Mathf.Abs(5 * relWithOtherKingdom.totalLike);
        }
        return defaultWeight;
    }
    private int GetFlatterDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
        int defaultWeight = 40;
        KingdomRelationship relOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);
        if (relOtherWithSource.totalLike < 0) {
            defaultWeight += Mathf.Abs(relOtherWithSource.totalLike);
        }
        return defaultWeight;
    }
    #endregion

    #region All Modifications
    private void GetAllModificationForWarOfConquest(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        KingdomRelationship relWithTargetKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        List<Kingdom> alliesAtWarWith = relWithTargetKingdom.GetAlliesTargetKingdomIsAtWarWith();
        //for each non-ally adjacent kingdoms that one of my allies declared war with recently
        if (relWithTargetKingdom.sharedRelationship.isAdjacent && !relWithTargetKingdom.AreAllies() && alliesAtWarWith.Count > 0) {
            //compare its theoretical power vs my theoretical power
            int sourceKingdomPower = relWithTargetKingdom._theoreticalPower;
            int otherKingdomPower = targetKingdom.GetRelationshipWithKingdom(sourceKingdom)._theoreticalPower;
            if (otherKingdomPower * 1.25f < sourceKingdomPower) {
                //If his theoretical power is not higher than 25% over mine
                defaultWeight = 20;
                for (int j = 0; j < alliesAtWarWith.Count; j++) {
                    Kingdom currAlly = alliesAtWarWith[j];
                    KingdomRelationship relationshipWithAlly = sourceKingdom.GetRelationshipWithKingdom(currAlly);
                    if (relationshipWithAlly.totalLike > 0) {
                        defaultWeight += 2 * relationshipWithAlly.totalLike; //add 2 weight per positive opinion i have over my ally
                    } else if (relationshipWithAlly.totalLike < 0) {
                        defaultWeight += relationshipWithAlly.totalLike; //subtract 1 weight per negative opinion i have over my ally (totalLike is negative)
                    }
                }
                //add 1 weight per negative opinion i have over the target
                //subtract 1 weight per positive opinion i have over the target
                defaultWeight += (relWithTargetKingdom.totalLike * -1); //If totalLike is negative it becomes positive(+), otherwise it becomes negative(-)
                defaultWeight = Mathf.Max(0, defaultWeight);
            }
        }
    }
    private void GetAllModificationForAllianceOfProtection(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        if (sourceKingdom.IsThreatened()) {
            //loop through known Kingdoms i am not at war with and whose Opinion of me is positive
            KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
            KingdomRelationship relOfOtherWithSource = targetKingdom.GetRelationshipWithKingdom(sourceKingdom);
            if (!relOfOtherWithSource.sharedRelationship.isAtWar && relOfOtherWithSource.totalLike > 0) {
                defaultWeight += 3 * relOfOtherWithSource.totalLike;//add 3 Weight for every positive Opinion it has towards me
                defaultWeight += relWithOtherKingdom.totalLike;//subtract 1 Weight for every negative Opinion I have towards it
                if (sourceKingdom.recentlyRejectedOffers.ContainsKey(targetKingdom)) {
                    defaultWeight -= 50;
                } else if (sourceKingdom.recentlyBrokenAlliancesWith.Contains(targetKingdom)) {
                    defaultWeight -= 50;
                }
                defaultWeight = Mathf.Max(0, defaultWeight); //minimum 0
            }
        }
    }
    private void GetAllModificationForTradeDeal(Kingdom sourceKingdom, Kingdom targetKingdom, ref int defaultWeight) {
        Dictionary<RESOURCE_TYPE, int> deficitOfTargetKingdom = targetKingdom.GetDeficitResourcesFor(sourceKingdom);
        Dictionary<RESOURCE_TYPE, int> surplusOfThisKingdom = sourceKingdom.GetSurplusResourcesFor(targetKingdom);
        foreach (KeyValuePair<RESOURCE_TYPE, int> kvp in surplusOfThisKingdom) {
            RESOURCE_TYPE currSurplus = kvp.Key;
            int surplusAmount = kvp.Value;
            if (deficitOfTargetKingdom.ContainsKey(currSurplus)) {
                //otherKingdom has a deficit for currSurplus
                //add Default Weight for every point of Surplus they have on our Deficit Resources 
                defaultWeight += surplusAmount;
            }
        }
    }
    #endregion
}
