using UnityEngine;
using System.Collections;

[System.Serializable]
public struct AssassinationTrigger {
	public ASSASSINATION_TRIGGER assassinationTrigger;
	public int rate;

	public AssassinationTrigger(ASSASSINATION_TRIGGER assassinationTrigger, int rate) {
		this.assassinationTrigger = assassinationTrigger;
		this.rate = rate;
	}

	internal void DefaultValues(){
		this.assassinationTrigger = ASSASSINATION_TRIGGER.NONE;
		this.rate = 0;
	}
}
