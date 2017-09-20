using UnityEngine;
using System.Collections;

public struct WarfareInfo {
	public WAR_SIDE side;
	public Warfare warfare;

	public WarfareInfo(WAR_SIDE side, Warfare warfare){
		this.side = side;
		this.warfare = warfare;
	}

	internal void DefaultValues(){
		this.side = WAR_SIDE.NONE;
		this.warfare = null;
	}
}
