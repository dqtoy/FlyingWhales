using UnityEngine;
using System.Collections;

public struct FirstAndKeystoneOwnership {
	public bool knowEffects;
	public bool hasRequested;
	public bool hasDecided;
	public EVENT_APPROACH approach;

	public FirstAndKeystoneOwnership(bool knowEffects, bool hasRequested, bool hasDecided, EVENT_APPROACH approach){
		this.knowEffects = knowEffects;
		this.hasRequested = hasRequested;
		this.hasDecided = hasDecided;
		this.approach = approach;
	}

	public void DefaultValues(){
		this.knowEffects = false;
		this.hasRequested = false;
		this.hasDecided = false;
		this.approach = EVENT_APPROACH.NONE;
	}
}
