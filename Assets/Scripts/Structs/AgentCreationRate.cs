using UnityEngine;
using System.Collections;

[System.Serializable]
public struct AgentCreationRate {
	public EVENT_TYPES eventType;
	public int rate; // number of days it needs to fill its growth meter, once full, the Kingdom will produce a new Agent of this type once the gold is available

	public AgentCreationRate(EVENT_TYPES eventType, int rate) {
		this.eventType = eventType;
		this.rate = rate;
	}
}