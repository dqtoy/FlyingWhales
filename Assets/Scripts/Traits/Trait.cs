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
     * WEIGHTED_ACTION type.
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
    /*
     * This will return a dictionary of
     * kingdoms and their respective weights for WAR. The base class
     * uses the logic for all traits, override this method for
     * specific logic on other taits
     * */
    internal virtual Dictionary<Kingdom, int> GetWarOfConquestTargetWeights() {
        return null;
    }
    internal virtual Dictionary<Kingdom, Dictionary<Kingdom, int>> GetAllianceOfConquestTargetWeights() {
        return null;
    }
    internal virtual Dictionary<Kingdom, int> GetAllianceOfProtectionTargetWeights() {
        return null;
    }
    internal virtual Dictionary<Kingdom, int> GetTradeDealTargetWeights() {
        return null;
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
