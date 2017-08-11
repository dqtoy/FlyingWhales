using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhoulLair : Lair {

	public GhoulLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this.name = "Ghoul Lair";
		Initialize();
	}

	#region Overrides
	public override void Initialize(){
		base.Initialize();
        //Create structure
		this.goStructure = this.hexTile.CreateSpecialStructureOnTile(this.type);
    }
	public override void EverydayAction (){
		base.EverydayAction ();
		if(this.daysCounter >= this.spawnRate){
			this.daysCounter = 0;
			SummonMonster(MONSTER.GHOUL);
		}
	}
	#endregion

	private void SummonGhoul(){
//		AcquireTarget();
//		if(this._targetHextile != null){
//			MonsterManager.Instance.SummonNewMonster(MONSTER.GHOUL, this.hexTile);
//		}
	}

}
