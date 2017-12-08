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
            if (GoalManager.Instance.ActionMeetsRequirements(ownerOfTrait.city.kingdom, currAction)) {
                int totalWeightOfAction = Mathf.Max(0, GetBaseWeightOfAction(currAction)); //So that the returned number can never be negative
                if (totalWeightOfAction > 0) {
                    totalWeights.Add(currAction, totalWeightOfAction);
                }
            }
            
        }
        return totalWeights;
    }

    #region Weighted Actions
    internal int GetWeightOfActionGivenTarget(WEIGHTED_ACTION weightedAction, object target, int currentWeight) {
        switch (weightedAction) {
            //case WEIGHTED_ACTION.WAR_OF_CONQUEST:
            //    return GetWarOfConquestWeightModification((Kingdom)target);
            case WEIGHTED_ACTION.ALLIANCE_OF_PROTECTION:
                return GetAllianceOfProtectionWeightModification((Kingdom)target);
            case WEIGHTED_ACTION.TRADE_DEAL:
                return GetTradeDealWeightModification((Kingdom)target);
            case WEIGHTED_ACTION.INCITE_UNREST:
                return GetInciteUnrestWeightModification((Kingdom)target);
            case WEIGHTED_ACTION.START_INTERNATIONAL_INCIDENT:
                return GetInternationalIncidentWeightModification((Kingdom)target);
            case WEIGHTED_ACTION.FLATTER:
                return GetFlatterWeightModification((Kingdom)target);
            case WEIGHTED_ACTION.LEAVE_ALLIANCE:
                return GetLeaveAllianceWeightModification((AlliancePool)target);
            case WEIGHTED_ACTION.LEAVE_TRADE_DEAL:
                return GetLeaveTradeDealWeightModification((Kingdom)target);
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
    internal int GetDontDoActionWeight(WEIGHTED_ACTION weightedAction, object target) {
        switch (weightedAction) {
            case WEIGHTED_ACTION.LEAVE_ALLIANCE:
                return GetKeepAllianceWeightModification((AlliancePool)target);
            default:
                return 0;
        }
    }

    //internal virtual int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
    //    return 0;
    //}
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
    internal virtual int GetLeaveAllianceWeightModification(AlliancePool alliance) {
        return 0;
    }
    internal virtual int GetKeepAllianceWeightModification(AlliancePool alliance) {
        return 0;
    }
    internal virtual int GetLeaveTradeDealWeightModification(Kingdom otherKingdom) {
        return 0;
    }
	internal virtual int GetInternationalIncidentReactionWeight(InternationalIncident.INCIDENT_ACTIONS incidentAction, KingdomRelationship kr){
		return 0;
	}
	internal virtual int GetRefugeeGovernorDecisionWeight(Refuge.GOVERNOR_DECISION decision){
		return 0;
	}
	internal virtual int GetKingdomThreatOpinionChange(int threat, out string summary){
		summary = string.Empty;
		return 0;
	}
	internal virtual int GetRandomInternationalIncidentWeight(){
		return 0;
	}
	internal virtual int GetMaxGeneralsModifier(){
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
