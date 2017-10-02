using UnityEngine;
using System.Collections;

public struct JoinWarfareInfo {
	public WarfareInfo info;
	public Kingdom allyKingdom;
	public bool isAdjacentToEnemy;

	public JoinWarfareInfo(WarfareInfo info, Kingdom allyKingdom, bool isAdjacentToEnemy){
		this.info = info;
		this.allyKingdom = allyKingdom;
		this.isAdjacentToEnemy = isAdjacentToEnemy;
	}

	internal void DefaultValues(){
		this.info.DefaultValues();
		this.allyKingdom = null;
		this.isAdjacentToEnemy = false;	
	}
}
