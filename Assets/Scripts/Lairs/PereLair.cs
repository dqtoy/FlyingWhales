using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PereLair : Lair {

	public PereLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this.name = "Pere Lair";
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
			SummonMonster(MONSTER.PERE);

		}
	}
	#endregion

	private void SummonPere(){
//		AcquireTarget();
//		if(this._targetHextile != null){
//			MonsterManager.Instance.SummonNewMonster(MONSTER.PERE, this.hexTile);
//		}
	}

}
