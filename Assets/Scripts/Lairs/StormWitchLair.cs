using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StormWitchLair : Lair {

	public StormWitchLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this.name = "Storm Witch Lair";
		Initialize();
	}

	#region Overrides
	public override void Initialize(){
		base.Initialize();
        //Create structure
		this.goStructure = this.hexTile.CreateSpecialStructureOnTile(LAIR.STORM_WITCH);
    }
	public override void EverydayAction (){
		base.EverydayAction ();
		if(this.daysCounter >= this.spawnRate){
			this.daysCounter = 0;
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 10){
				SummonWitch();
			}
		}
	}
	#endregion

	private void SummonWitch(){
		AcquireTarget();
		if(this._targetHextile != null){
			MonsterManager.Instance.SummonNewMonster(MONSTER.STORM_WITCH, this.hexTile, this._targetHextile);
		}
	}
}
