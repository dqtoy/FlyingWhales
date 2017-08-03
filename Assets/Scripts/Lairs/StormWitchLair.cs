using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StormWitchLair : Lair {

	private HexTile _targetHextile;
	private List<HexTile> availableTargets;

	public StormWitchLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this.name = "Storm Witch Lair";
		this._targetHextile = null;
		this.availableTargets = new List<HexTile>();
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
	private void AcquireTarget(){
		this.availableTargets.Clear();
		this._targetHextile = null;
		for (int i = 0; i < this.tilesInRadius.Count; i++) {
			if(this.tilesInRadius[i] != null){
				if(this.tilesInRadius[i].isOccupied && this.tilesInRadius[i].isHabitable && this.tilesInRadius[i].city.id != 0){
					this.availableTargets.Add(this.tilesInRadius[i]);
				}
			}
		}

		if(this.availableTargets.Count > 0){
			this._targetHextile = this.availableTargets[UnityEngine.Random.Range(0, this.availableTargets.Count)];
		}
	}
}
