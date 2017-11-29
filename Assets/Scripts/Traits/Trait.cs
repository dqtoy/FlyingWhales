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


    public void AssignCitizen(Citizen ownerOfTrait) {
        this.ownerOfTrait = ownerOfTrait;
    }

    /*
     * This will return a Dictionary, containing the weights of each
     * WEIGHTED_ACTION type. This is for determining what action the citizen
     * should do.
     * */
    internal Dictionary<WEIGHTED_ACTION, int> GetTotalActionWeights() {
        WEIGHTED_ACTION[] allWeightedActions = Utilities.GetEnumValues<WEIGHTED_ACTION>();
        Dictionary<WEIGHTED_ACTION, int> totalWeights = new Dictionary<WEIGHTED_ACTION, int>();
        
        for (int i = 0; i < allWeightedActions.Length; i++) {
            WEIGHTED_ACTION currAction = allWeightedActions[i];
            bool shouldIncludeActionToWeights = true;
            if (Utilities.weightedActionRequirements.ContainsKey(currAction)) {
                if (Utilities.weightedActionRequirements[currAction].Contains(WEIGHTED_ACTION_REQS.NO_ALLIANCE) && ownerOfTrait.city.kingdom.alliancePool != null) {
                    shouldIncludeActionToWeights = false;
                }
            }
            if (shouldIncludeActionToWeights) {
                int totalWeightOfAction = Mathf.Max(0, GetBaseWeightOfAction(currAction)); //So that the returned number can never be negative
                if (totalWeightOfAction > 0) {
                    totalWeights.Add(currAction, totalWeightOfAction);
                }
            }
            
        }
        return totalWeights;
    }

    #region Weighted Actions
    internal int GetWeightOfActionGivenTarget(WEIGHTED_ACTION weightedAction, Kingdom targetKingdom, int currentWeight) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.WAR_OF_CONQUEST:
                return GetWarOfConquestWeightModification(targetKingdom);
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                return GetAllianceOfProtectionWeightModification(targetKingdom);
            case WEIGHTED_ACTION.TRADE_DEAL:
                return GetTradeDealWeightModification(targetKingdom);
            case WEIGHTED_ACTION.INCITE_UNREST:
                return GetInciteUnrestWeightModification(targetKingdom);
            case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
                return GetInternationalIncidentWeightModification(targetKingdom);
            case WEIGHTED_ACTION.FLATTER:
                return GetFlatterWeightModification(targetKingdom);
            default:
                return 0;
        }
    }
    internal int GetWeightOfActionGivenTargetAndCause(WEIGHTED_ACTION weightedAction, Kingdom targetKingdom, Kingdom causingKindom, int currentWeight) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.ALLIANCE_OF_CONQUEST:
                return GetAllianceOfConquestWeightModification(targetKingdom, causingKindom);
            default:
                return 0;
        }
    }

    internal virtual int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
        return 0;
    }
    internal virtual int GetAllianceOfConquestWeightModification(Kingdom otherKingdom, Kingdom causingKindom) {
        return 0;
    }
    internal virtual int GetAllianceOfProtectionWeightModification(Kingdom otherKingdom) {
        return 0;
    }
    internal virtual int GetTradeDealWeightModification(Kingdom otherKingdom) {
        return 0;
    }
    internal virtual int GetInciteUnrestWeightModification(Kingdom otherKingdom) {
        return 0;
    }
    internal virtual int GetInternationalIncidentWeightModification(Kingdom otherKingdom) {
        return 0;
    }
    internal virtual int GetFlatterWeightModification(Kingdom otherKingdom) {
        return 0;
    }
    #endregion


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
