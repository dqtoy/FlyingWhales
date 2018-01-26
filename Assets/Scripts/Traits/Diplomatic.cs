using UnityEngine;
using System.Collections;

public class Diplomatic : Trait {

    #region International Incidents
    internal override WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentReactionWeight(INTERNATIONAL_INCIDENT_TYPE incidentType,
        FactionRelationship rel, Faction aggressor) {
        WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
        actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 50); //Add 50 Weight to Do Nothing
        return actionWeights;
    }
    #endregion

    internal override int GetFlatterWeightModification(Kingdom otherKingdom) {
        Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
        int weight = 0;

        KingdomRelationship relWithOtherKingdom = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
        //add 2 to Default Weight for each positive point of Opinion I have towards the target
        if(relWithOtherKingdom.totalLike > 0) {
            weight += 2 * relWithOtherKingdom.totalLike;
        }
        
        return weight;
    }

    internal override int GetLeaveTradeDealWeightModification(Kingdom otherKingdom) {
        return -40; //subtract 40 from Default Weight
    }

	internal override int GetInternationalIncidentReactionWeight (InternationalIncident.INCIDENT_ACTIONS incidentAction, KingdomRelationship kr){
		if(incidentAction == InternationalIncident.INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
			return 100;
		}
		return 0;
	}
	internal override int GetRandomInternationalIncidentWeight(){
		return -20;
	}
}
