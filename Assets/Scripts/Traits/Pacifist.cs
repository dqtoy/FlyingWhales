using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pacifist : Trait {

    #region International Incidents
    internal override WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentReactionWeight(INTERNATIONAL_INCIDENT_TYPE incidentType,
        FactionRelationship rel, Faction aggressor) {
        WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
        actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 50); //Add 50 Weight to Do Nothing
        return actionWeights;
    }
    #endregion

    //  internal override int GetWarOfConquestWeightModification(Kingdom otherKingdom) {
    //      Kingdom sourceKingdom = ownerOfTrait.city.kingdom;
    //      int weight = 0;
    //      //loop through adjacent kingdoms i am not at war with:
    //      KingdomRelationship currRel = sourceKingdom.GetRelationshipWithKingdom(otherKingdom);
    //if (currRel.sharedRelationship.isAdjacent && !currRel.sharedRelationship.isAtWar) {
    //          KingdomRelationship otherKingdomRelTowardsSource = otherKingdom.GetRelationshipWithKingdom(sourceKingdom);
    //          if (currRel._theoreticalPower > otherKingdomRelTowardsSource._theoreticalPower) {//compare its theoretical power vs my theoretical power
    //              weight = -10; //subtract 10 base weight and 
    //              //subtract 5 weight per 1% of his theoretical power over mine
    //              float theoreticalPowerPercent = otherKingdomRelTowardsSource.GetTheoreticalPowerAdvantageOverTarget();
    //              if (theoreticalPowerPercent > 0) {
    //                  weight -= 5 * (int)theoreticalPowerPercent;
    //              }
    //              if (currRel.totalLike > 0) {
    //                  //subtract 2 per positive opinion
    //                  weight -= Mathf.Abs(currRel.totalLike * 2);
    //              }
    //          }
    //      }
    //      return weight;
    //  }

    #region Leave Alliance
    internal override int GetLeaveAllianceWeightModification(AlliancePool alliance) {
        //otherKingdom is Current Ally Member
        int weight = 0;
        for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
            Kingdom ally = alliance.kingdomsInvolved[i];
            if(ally.id != ownerOfTrait.city.kingdom.id) {
                weight += 20 * ally.GetWarCount(); //add 20 weight to leave alliance for every active war of active kingdoms in alliance
            }
        }
        return weight;
    }
    internal override int GetKeepAllianceWeightModification(AlliancePool alliance) {
        //add 50 weight of keep alliance if no other member is at war
        for (int i = 0; i < alliance.kingdomsInvolved.Count; i++) {
            Kingdom ally = alliance.kingdomsInvolved[i];
            if (ally.id != ownerOfTrait.city.kingdom.id) {
                if (ally.GetWarCount() > 0) {
                    return 0;
                }
            }
        }
        return 50;
    }
	internal override int GetInternationalIncidentReactionWeight (InternationalIncident.INCIDENT_ACTIONS incidentAction, KingdomRelationship kr){
		if(incidentAction == InternationalIncident.INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
			return 50;
		}
		return 0;
	}

	internal override int GetRandomInternationalIncidentWeight(){
		return -20;
	}

	internal override int GetMaxGeneralsModifier(){
		return -1;
	}
    #endregion

}
