using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lair {

	public int hp;
	public LAIR type;
	public int spawnRate;
	public HexTile hexTile;
	public int daysCounter;
	public GameObject goStructure;
	public List<HexTile> tilesInRadius;

	public bool isDead;

	public Lair(LAIR type, HexTile hexTile){
		this.type = type;
		this.hexTile = hexTile;
		this.hp = GetLairHP();
		this.spawnRate = GetSpawnRate();
		this.goStructure = null;
		this.tilesInRadius = this.hexTile.GetTilesInRange(MonsterManager.Instance.tileRadiusDetection);
		this.isDead = false;
		AttachLairToHextile();
		if(MonsterManager.Instance.activateLairImmediately){
			ActivateLair();
		}
	}

	private int GetLairHP(){
		switch (this.type){
		case LAIR.LYCAN:
			return 200;
		}
		return 0;
	}
	private int GetSpawnRate(){
		switch (this.type){
		case LAIR.LYCAN:
			return 15;
		}
		return 0;
	}
	private void ActivateLair(){
		EventManager.Instance.onWeekEnd.AddListener(EverydayAction);
	}
	private void AttachLairToHextile(){
		this.hexTile.isLair = true;
		this.hexTile.isOccupied = true;
		this.hexTile.lair = this;
	}
	private void DetachLairFromHextile(){
		this.hexTile.isLair = false;
		this.hexTile.isOccupied = false;
		this.hexTile.lair = null;
	}
	private void DestroyLair(){
		if(this.goStructure != null){
			GameObject.Destroy(this.goStructure);
			this.goStructure = null;
		}
		this.isDead = true;
		DetachLairFromHextile();

		//Reset Hextile

		EventManager.Instance.onWeekEnd.RemoveListener(EverydayAction);
		MonsterManager.Instance.RemoveFromLairList(this);
	}

	public void AdjustHP(int amount){
		this.hp += amount;
		if(this.hp < 0){
			this.hp = 0;
			DestroyLair();
		}
	}

	#region Virtual
	public virtual void Initialize(){}
	public virtual void EverydayAction(){
		this.daysCounter += 1;
	}
	#endregion
}
