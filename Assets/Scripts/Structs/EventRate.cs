using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EventRate {
	public EVENT_TYPES eventType;
	public int rate;
	public RELATIONSHIP_STATUS[] relationshipTargets;
	public KINGDOM_TYPE[] kingdomTypes;
	public MILITARY_STRENGTH[] militaryStrength;

	public EventRate(EVENT_TYPES eventType, int rate, RELATIONSHIP_STATUS[] relationshipTargets, KINGDOM_TYPE[] kingdomTypes, MILITARY_STRENGTH[] militaryStrength) {
		this.eventType = eventType;
		this.rate = rate;
		this.relationshipTargets = relationshipTargets;
		this.kingdomTypes = kingdomTypes;
		this.militaryStrength = militaryStrength;
	}
}