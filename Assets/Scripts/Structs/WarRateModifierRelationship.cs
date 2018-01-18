using UnityEngine;
using System.Collections;

[System.Serializable]
public struct WarRateModifierRelationship {

	public KINGDOM_RELATIONSHIP_STATUS relationshipStatus;
	public int rate;

	public WarRateModifierRelationship(KINGDOM_RELATIONSHIP_STATUS relationshipStatus, int rate){
		this.relationshipStatus = relationshipStatus;
		this.rate = rate;
	}

	internal void DefaultValues(){
		this.relationshipStatus = KINGDOM_RELATIONSHIP_STATUS.NA;
		this.rate = -100;
	}
}
