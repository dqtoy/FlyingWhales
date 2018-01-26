using UnityEngine;
using System.Collections;

public class Benevolent : Trait {

    #region International Incidents
    internal override WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentReactionWeight(INTERNATIONAL_INCIDENT_TYPE incidentType,
        FactionRelationship rel, Faction aggressor) {
        WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
        actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 20); //Add 20 Weight to Do Nothing

        Relationship chieftainRel = CharacterManager.Instance.GetRelationshipBetween(_ownerOfTrait, aggressor.leader);
        if (chieftainRel != null) {
            if(chieftainRel.totalValue > 0) {
                //Add 2 Weight to Do Nothing per Positive Opinion the Chieftain has towards the other Chieftain (if they have a relationship)
                actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 2 * chieftainRel.totalValue);
            }
        }
        
        return actionWeights;
    }
    #endregion

    internal override int GetLeaveTradeDealWeightModification(Kingdom otherKingdom) {
        return -40; //subtract 40 from Default Weight
    }
	internal override int GetInternationalIncidentReactionWeight (InternationalIncident.INCIDENT_ACTIONS incidentAction, KingdomRelationship kr){
		int weight = 0;
		if(incidentAction == InternationalIncident.INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
			weight += 20;
			if(kr.totalLike > 0){
				weight += kr.totalLike * 2;
			}
		}
		return weight;
	}
	internal override int GetRefugeeGovernorDecisionWeight(Refuge.GOVERNOR_DECISION decision){
		if(decision == Refuge.GOVERNOR_DECISION.ACCEPT){
			return 200;
		}
		return 0;
	}
	internal override int GetKingdomThreatOpinionChange(int threat, out string summary){
		summary = string.Empty;
		if(threat >= -50 && threat < -25){
			summary = "Protects the Weak";
			return 15;
		}else if(threat > -100 && threat < -50){
			summary = "Protects the Weak";
			return 30;
		}else if(threat <= -100){
			summary = "Protects the Weak";
			return 50;
		}
		return 0;
	}
}
