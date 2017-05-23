using UnityEngine;
using System.Collections;

[System.Serializable]
public struct WarRateModifierRelationship {

	public RELATIONSHIP_STATUS relationshipStatus;
	public int rate;

	public WarRateModifierRelationship(RELATIONSHIP_STATUS relationshipStatus, int rate){
		this.relationshipStatus = relationshipStatus;
		this.rate = rate;
	}

	internal void DefaultValues(){
		this.relationshipStatus = RELATIONSHIP_STATUS.NA;
		this.rate = -100;
	}
}
