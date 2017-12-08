using UnityEngine;
using System.Collections;

public class Benevolent : Trait {

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
