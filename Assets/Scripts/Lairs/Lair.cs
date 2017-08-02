using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Lair {
	private delegate void OnPerformAction();
	private OnPerformAction onPerformAction;

	public int hp;
	public LAIR type;
	public int spawnRate;
	public HexTile hexTile;
	public int daysCounter;
	public GameObject goStructure;
	public List<HexTile> tilesInRadius;

	public bool isDead;
	public bool isActivated;
	public Lair(LAIR type, HexTile hexTile){
		this.type = type;
		this.hexTile = hexTile;
		this.hp = GetLairHP();
		this.spawnRate = GetSpawnRate();
		this.goStructure = null;
		this.tilesInRadius = this.hexTile.GetTilesInRange(MonsterManager.Instance.tileRadiusDetection);
		this.isDead = false;
		this.isActivated = false;
		AttachLairToHextile();
		EventManager.Instance.onWeekEnd.AddListener (PerformAction);
		onPerformAction += CheckForActivation;

		if(MonsterManager.Instance.activateLairImmediately){
			ActivateLair();
		}
	}

	private int GetLairHP(){
		switch (this.type){
		case LAIR.LYCAN:
			return 200;
		case LAIR.STORM_WITCH:
			return 200;
		}
		return 0;
	}
	private int GetSpawnRate(){
		switch (this.type){
		case LAIR.LYCAN:
			return 30;
		case LAIR.STORM_WITCH:
			return 365;
		}
		return 0;
	}
	internal void ActivateLair(){
		onPerformAction -= CheckForActivation;
		if(!this.isActivated){
			this.isActivated = true;
			onPerformAction += EverydayAction;
		}
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
        this.hexTile.ResetTile();

		onPerformAction -= EverydayAction;
		MonsterManager.Instance.RemoveFromLairList(this);
	}

	public void AdjustHP(int amount){
		this.hp += amount;
		if(this.hp < 0){
			this.hp = 0;
			DestroyLair();
		}
	}

	private void PerformAction(){
		if(onPerformAction != null){
			onPerformAction ();
		}
	}
	private void CheckForActivation(){
		if(!this.isActivated){
			if(this.hexTile.isBorder){
				ActivateLair ();
			}else{
				List<HexTile> neighbors = this.hexTile.AllNeighbours.ToList ();
				for (int i = 0; i < neighbors.Count; i++) {
					if(neighbors[i].isBorder){
						ActivateLair ();
						break;
					}
				}
			}
		}
	}
	#region Virtual
	public virtual void Initialize(){}
	public virtual void EverydayAction(){
		this.daysCounter += 1;
	}
	#endregion
}
