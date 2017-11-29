using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoalManager : MonoBehaviour {

    public static GoalManager Instance = null;

    [SerializeField] private List<WeightedActionRequirements> actionRequirements;

    public Dictionary<WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>> weightedActionRequirements;

    public static HashSet<WEIGHTED_ACTION> indirectActionTypes = new HashSet<WEIGHTED_ACTION>() {
        WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST
    };

    public static HashSet<WEIGHTED_ACTION> specialActionTypes = new HashSet<WEIGHTED_ACTION>() {
        WEIGHTED_ACTION.DECLARE_PEACE, WEIGHTED_ACTION.LEAVE_ALLIANCE, WEIGHTED_ACTION.LEAVE_TRADE_DEAL
    };

    private void Awake() {
        Instance = this;
        ConstructActionRequirementsDictionary();
    }

    private void ConstructActionRequirementsDictionary() {
        weightedActionRequirements = new Dictionary<WEIGHTED_ACTION, List<WEIGHTED_ACTION_REQS>>();
        for (int i = 0; i < actionRequirements.Count; i++) {
            WeightedActionRequirements currReq = actionRequirements[i];
            weightedActionRequirements.Add(currReq.weightedAction, currReq.requirements);
        }
    }

    internal WEIGHTED_ACTION DetermineWeightedActionToPerform(Kingdom sourceKingdom) {
        Debug.Log("========== " + sourceKingdom.name + " is trying to decide what to do... ==========");
        Dictionary<WEIGHTED_ACTION, int> totalWeightedActions = new Dictionary<WEIGHTED_ACTION, int>();
        totalWeightedActions.Add(WEIGHTED_ACTION.DO_NOTHING, 150); //Add 150 Base Weight on Do Nothing Action
        if (ActionMeetsRequirements(sourceKingdom, WEIGHTED_ACTION.DECLARE_PEACE)) {
            totalWeightedActions.Add(WEIGHTED_ACTION.DECLARE_PEACE, sourceKingdom.GetWarCount() * 5); //If at war, Add 5 weight to declare peace for each active war
        }
        if (ActionMeetsRequirements(sourceKingdom, WEIGHTED_ACTION.LEAVE_ALLIANCE)) {
            totalWeightedActions.Add(WEIGHTED_ACTION.LEAVE_ALLIANCE, 5); //If at war and in an alliance, Add 5 weight to leave alliance
        }
        if (ActionMeetsRequirements(sourceKingdom, WEIGHTED_ACTION.LEAVE_TRADE_DEAL)) {
            //If in a trade deal, add 5 weight to leave trade deal for each active trade deal
            totalWeightedActions.Add(WEIGHTED_ACTION.LEAVE_TRADE_DEAL, 5 * sourceKingdom.kingdomsInTradeDealWith.Count); 
        }
        for (int i = 0; i < sourceKingdom.king.allTraits.Count; i++) {
            Trait currTrait = sourceKingdom.king.allTraits[i];
            Dictionary<WEIGHTED_ACTION, int> weightsFromCurrTrait = currTrait.GetTotalActionWeights();
            totalWeightedActions = Utilities.MergeWeightedActionDictionaries(totalWeightedActions, weightsFromCurrTrait);
        }

        string actionWeightsSummary = "Action Weights of " + sourceKingdom.name;
        foreach (KeyValuePair<WEIGHTED_ACTION, int> kvp in totalWeightedActions) {
            actionWeightsSummary += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
        }
        Debug.Log(actionWeightsSummary);

        WEIGHTED_ACTION chosenAction = Utilities.PickRandomElementWithWeights(totalWeightedActions);
        Debug.Log("Chosen action of " + sourceKingdom.name + " is " + chosenAction.ToString());
        return chosenAction;
    }

    #region Weight Dictionaries
    internal Dictionary<T, int> GetWeightsForSpecialActionType<T>(Kingdom source, List<T> choices, WEIGHTED_ACTION actionType, ref int weightToNotPerformAction) {
        Dictionary<T, int> weights = new Dictionary<T, int>();
        for (int i = 0; i < choices.Count; i++) {
            T currChoice = choices[i];
            int weightForCurrChoice = 0;
            if (actionType == WEIGHTED_ACTION.TRADE_DEAL) {
                //add Default Weight if Kingdom no longer benefits from any Surplus of the trade partner, otherwise, add its Default Weight to Not Leave Any Trade Deal
                Kingdom otherKingdom = (Kingdom)((object)currChoice);
                if (source.IsTradeDealStillNeeded(otherKingdom)) {
                    weightForCurrChoice = 0;
                    weightToNotPerformAction = GetDefaultWeightForAction(actionType, source, currChoice);
                } else {
                    weightForCurrChoice = GetDefaultWeightForAction(actionType, source, currChoice);
                    weightToNotPerformAction = 0;
                }
            } else {
                weightForCurrChoice = GetDefaultWeightForAction(actionType, source, currChoice);
            }
            //loop through all the traits of the current king
            for (int j = 0; j < source.king.allTraits.Count; j++) {
                Trait currTrait = source.king.allTraits[j];
                int modificationFromTrait = currTrait.GetWeightOfActionGivenTarget(actionType, currChoice, weightForCurrChoice);
                weightToNotPerformAction += currTrait.GetDontDoActionWeight(actionType, currChoice);
                weightForCurrChoice += modificationFromTrait;
            }
            ApplySpecialActionModificationForAll(actionType, source, currChoice, ref weightForCurrChoice, ref weightToNotPerformAction);
            weights.Add(currChoice, weightForCurrChoice);
        }
        return weights;
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
    internal Dictionary<Kingdom, Dictionary<Kingdom, int>> GetKingdomWeightsForIndirectActionType(Kingdom sourceKingdom, WEIGHTED_ACTION specialWeightedAction) {
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
    #endregion

    #region Default Weights
    private int GetDefaultWeightForAction(WEIGHTED_ACTION weightedAction, object source, object target) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.WAR_OF_CONQUEST:
                return 0;
            case WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST:
                return 0;
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                return 0;
            case WEIGHTED_ACTION.TRADE_DEAL:
                return GetTradeDealDefaultWeight((Kingdom)source, (Kingdom)target);
            case WEIGHTED_ACTION.INCITE_UNREST:
                return GetInciteUnrestDefaultWeight((Kingdom)source, (Kingdom)target);
            case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
                return GetInternationalIncidentDefaultWeight((Kingdom)source, (Kingdom)target);
            case WEIGHTED_ACTION.FLATTER:
                return GetFlatterDefaultWeight((Kingdom)source, (Kingdom)target);
            case WEIGHTED_ACTION.SEND_AID:
                return 0;
            case WEIGHTED_ACTION.LEAVE_TRADE_DEAL:
                return GetLeaveTradeDealDefaultWeight((Kingdom)source, (Kingdom)target);
            default:
                return 0;
        }
    }
    private int GetTradeDealDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
        if (sourceKingdom.kingdomsInTradeDealWith.Contains(targetKingdom)) {
            return 0;
        }
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
    private int GetLeaveTradeDealDefaultWeight(Kingdom sourceKingdom, Kingdom targetKingdom) {
        int defaultWeight = 0;
        if (sourceKingdom.kingdomsInTradeDealWith.Contains(targetKingdom)) {
            defaultWeight = 100; //Default Weight to Leave Trade Deal is 100
            KingdomRelationship relSourceWithOther = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
            if (relSourceWithOther.targetKingdomThreatLevel > 0) {
                defaultWeight += relSourceWithOther.targetKingdomThreatLevel; //add 1 to Default Weight for every Threat of the kingdom
            }
            if (relSourceWithOther.totalLike < 0) {
                defaultWeight += Mathf.Abs(2 * relSourceWithOther.totalLike); //add 2 to Default Weight for every negative Opinion I have towards the king
            } else if (relSourceWithOther.totalLike > 0) {
                defaultWeight -= 2 * relSourceWithOther.totalLike; //subtract 2 to Default Weight for every positive Opinion I have towards the king
            }

            //add Default Weight if Kingdom no longer benefits from any Surplus of the trade partner, otherwise, add its Default Weight to Not Leave Any Trade Deal
        }
        return defaultWeight;
    }
    #endregion

    #region All Modifications
    private void ApplyActionModificationForAll(WEIGHTED_ACTION weightedAction, object source, object target, ref int defaultWeight) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.WAR_OF_CONQUEST:
                GetAllModificationForWarOfConquest((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                GetAllModificationForAllianceOfProtection((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
            case WEIGHTED_ACTION.TRADE_DEAL:
                GetAllModificationForTradeDeal((Kingdom)source, (Kingdom)target, ref defaultWeight);
                break;
        }
    }
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
        if (sourceKingdom.kingdomsInTradeDealWith.Contains(targetKingdom)) {
            return;
        }
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
    

    private void ApplySpecialActionModificationForAll(WEIGHTED_ACTION weightedAction, object source, object target, ref int defaultWeight, ref int weightNotToDoAction) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.DECLARE_PEACE:
                GetAllModificationForDeclarePeace((Kingdom)source, (Warfare)target, ref defaultWeight, ref weightNotToDoAction);
                break;
            case WEIGHTED_ACTION.LEAVE_ALLIANCE:
                GetAllModificationForLeaveAlliance((Kingdom)source, (AlliancePool)target, ref defaultWeight, ref weightNotToDoAction);
                break;
        }
    }
    private void GetAllModificationForDeclarePeace(Kingdom sourceKingdom, Warfare targetWar, ref int defaultWeight, ref int weightNotToDoAction) {
        WAR_SIDE sourceSide = targetWar.GetSideOfKingdom(sourceKingdom);
        WAR_SIDE otherSide = WAR_SIDE.A;
        if(sourceSide == WAR_SIDE.A) {
            otherSide = WAR_SIDE.B;
        }
        List<Kingdom> enemyKingdoms = targetWar.GetListFromSide(otherSide);
        for (int i = 0; i < enemyKingdoms.Count; i++) {
            Kingdom enemyKingdom = enemyKingdoms[i];
            KingdomRelationship sourceRelWithEnemy = sourceKingdom.GetRelationshipWithKingdom(enemyKingdom);
            KingdomRelationship enemyRelWithSource = enemyKingdom.GetRelationshipWithKingdom(sourceKingdom);
            //add 2 to Weight to Declare Peace for every Relative Strength the enemy kingdoms have over me
            if(enemyRelWithSource.relativeStrength > 0) {
                defaultWeight += 2 * enemyRelWithSource.relativeStrength;
            }
            //add 2 to Weight to Don't Declare Peace for every Relative Strength I have over each enemy kingdom
            if (sourceRelWithEnemy.relativeStrength > 0) {
                weightNotToDoAction += 2 * sourceRelWithEnemy.relativeStrength;
            }
            //add 3 Weight to Declare Peace for each War Weariness I have
            weightNotToDoAction += 3 * targetWar.kingdomSideWeariness[sourceKingdom.id].weariness;
        }
    }
    private void GetAllModificationForLeaveAlliance(Kingdom sourceKingdom, AlliancePool alliance, ref int defaultWeight, ref int weightNotToDoAction) {
        //loop through the other kingdoms within the alliance
        for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
            Kingdom ally = alliance.kingdomsInvolved[i];
            if (ally.id != sourceKingdom.id) {
                KingdomRelationship relWithAlly = sourceKingdom.GetRelationshipWithKingdom(ally);
                if(relWithAlly.targetKingdomThreatLevel > 0) {
                    defaultWeight += relWithAlly.targetKingdomThreatLevel; //add 1 weight to leave alliance for every threat of ally kingdom
                }
                if (relWithAlly.totalLike < 0) {
                    defaultWeight +=  Mathf.Abs(3 * relWithAlly.totalLike); //add 3 weight to leave alliance for every negative opinion I have towards the king
                } else if (relWithAlly.totalLike > 0) {
                    weightNotToDoAction += 2 * relWithAlly.totalLike; //add 2 weight to keep alliance for every positive opinion I have towards the king
                }
            }
        }

        for (int i = 0; i < sourceKingdom.adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = sourceKingdom.adjacentKingdoms[i];
            if (!alliance.kingdomsInvolved.Contains(otherKingdom)) {
                //loop through non-ally adjacent kingdoms
                KingdomRelationship relWithOther = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
                if(relWithOther.targetKingdomThreatLevel > 0) {
                    weightNotToDoAction += relWithOther.targetKingdomThreatLevel; //add 1 weight to keep alliance for every threat of the kingdom
                }
            }
        }
    }
    #endregion

    #region Action Requirements
    internal bool ActionMeetsRequirements(Kingdom sourceKingdom, WEIGHTED_ACTION actionType) {
        if (weightedActionRequirements.ContainsKey(actionType)) {
            List<WEIGHTED_ACTION_REQS> requirements = weightedActionRequirements[actionType];
            for (int i = 0; i < requirements.Count; i++) {
                WEIGHTED_ACTION_REQS currRequirement = requirements[i];
                switch (currRequirement) {
                    case WEIGHTED_ACTION_REQS.NO_ALLIANCE:
                        if(sourceKingdom.alliancePool != null) {
                            return false;
                        }
                        break;
                    case WEIGHTED_ACTION_REQS.HAS_ALLIANCE:
                        if (sourceKingdom.alliancePool == null) {
                            return false;
                        }
                        break;
                    case WEIGHTED_ACTION_REQS.HAS_WAR:
                        if(sourceKingdom.GetWarCount() <= 0) {
                            return false;
                        }
                        break;
                    case WEIGHTED_ACTION_REQS.HAS_ACTIVE_TRADE_DEAL:
                        if(sourceKingdom.kingdomsInTradeDealWith.Count <= 0) {
                            return false;
                        }
                        break;
                    default:
                        return true;
                }
            }
        }
        return true;
    }
    #endregion

}
