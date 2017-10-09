using UnityEngine;
using System.Collections;

public struct SideWeariness {
	public WAR_SIDE side;
	public int weariness;

	public SideWeariness(WAR_SIDE side, int weariness){
		this.side = side;
		this.weariness = weariness;
	}

	internal void DefaultValues(){
		this.side = WAR_SIDE.NONE;
		this.weariness = 0;
	}
	internal void AdjustWeariness(int amount){
		this.weariness += amount;
		if(this.weariness < 0){
			this.weariness = 0;
		}
	}
}
