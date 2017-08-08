using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LycanLair : Lair {

	public LycanLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this.name = "Lycan Lair";
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
			SummonLycan();
		}
	}
	#endregion

	private void SummonLycan(){
		AcquireTarget();
		if(this._targetHextile != null){
			MonsterManager.Instance.SummonNewMonster(MONSTER.LYCAN, this.hexTile, this._targetHextile);
		}
	}

}
