using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LycanLair : Lair {

	private int _lycanHP;
	private HexTile _targetHextile;
	private List<HexTile> availableTargets;

	public LycanLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this._lycanHP = 100;
		this._targetHextile = null;
		this.availableTargets = new List<HexTile>();
		Initialize();
	}

	#region Overrides
	public override void Initialize(){
		base.Initialize();
        //Create structure
        this.goStructure = this.hexTile.CreateSpecialStructureOnTile(LAIR.LYCAN);
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
	private void AcquireTarget(){
		this.availableTargets.Clear();
		this._targetHextile = null;
		for (int i = 0; i < this.tilesInRadius.Count; i++) {
			if(this.tilesInRadius[i].isOccupied && this.tilesInRadius[i].isHabitable && this.tilesInRadius[i].city.id != 0){
				this.availableTargets.Add(this.tilesInRadius[i]);
			}
		}

		if(this.availableTargets.Count > 0){
			this._targetHextile = this.availableTargets[UnityEngine.Random.Range(0, this.availableTargets.Count)];
		}
	}
}
