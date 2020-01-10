using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InhabitedFeature : TileFeature {

	public InhabitedFeature() {
		name = "Inhabited";
		description = "This tile has inhabitants.";
		type = REGION_FEATURE_TYPE.PASSIVE;
		isRemovedOnInvade = true;
	}  
}
