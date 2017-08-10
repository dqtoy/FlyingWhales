using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Lair {
	private delegate void OnPerformAction();
	private OnPerformAction onPerformAction;

	public int hp;
	public int maxHP;
	public LAIR type;
	public string name;
	public int spawnRate;
	public HexTile hexTile;
	public int daysCounter;
	public GameObject goStructure;
	public List<HexTile> tilesInRadius;

	public bool isDead;
	public bool isActivated;

	private LairItem _lairItem;
	private LairSpawn _lairSpawn;

	internal HexTile _targetHextile;
	internal List<HexTile> availableTargets;

	public Lair(LAIR type, HexTile hexTile){
		this.type = type;
		this.name = "Lair";
		this.hexTile = hexTile;
		this.hp = GetLairHP();
		this.maxHP = this.hp;
		this.spawnRate = GetSpawnRate();
		this._lairSpawn = MonsterManager.Instance.GetLairSpawnData (this.type);
		this.goStructure = null;
		this.tilesInRadius = this.hexTile.GetTilesInRange(this._lairSpawn.tileRadiusDetection);
		this.isDead = false;
		this.isActivated = false;
		this._targetHextile = null;
		this.availableTargets = new List<HexTile>();
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
			return 400;
		case LAIR.STORM_WITCH:
			return 200;
		case LAIR.PERE:
			return 1200;
		case LAIR.GHOUL:
			return 600;
		}
		return 0;
	}
	private int GetSpawnRate(){
		switch (this.type){
		case LAIR.LYCAN:
			return 30;
		case LAIR.STORM_WITCH:
//			return 30;
			return 365;
		case LAIR.PERE:
			return 20;
		case LAIR.GHOUL:
			return 30;
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
		this.hexTile.ShowLairNamePlate ();
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

        for (int i = 0; i < this.hexTile.isVisibleByCities.Count; i++) {
            City currCity = this.hexTile.isVisibleByCities[i];
            currCity.UpdateBorderTiles();
            if (this.hexTile.isBorder) {
                break;
            }
        }

        onPerformAction -= EverydayAction;
		MonsterManager.Instance.RemoveFromLairList(this);
	}

	public void AdjustHP(int amount){
		this.hexTile.UpdateLairNamePlate();

		this.hp += amount;
		if(this.hp < 0){
			this.hp = 0;
			DestroyLair();
		}else if(this.hp > this.maxHP){
			this.hp = this.maxHP;
		}
	}

	private void PerformAction(){
		if(onPerformAction != null){
			onPerformAction ();
		}
	}
	private void CheckForActivation(){
		if(!this.isActivated){
			for (int i = 0; i < this.tilesInRadius.Count; i++) {
				if (this.tilesInRadius [i].isHabitable && this.tilesInRadius [i].isOccupied && this.tilesInRadius [i].city.id != 0) {
					ActivateLair ();
					break;
				}
			}
//			if(this.hexTile.isBorder){
//				ActivateLair ();
//			}else{
//				List<HexTile> neighbors = this.hexTile.AllNeighbours.ToList ();
//				for (int i = 0; i < neighbors.Count; i++) {
//					if(neighbors[i].isBorder){
//						ActivateLair ();
//						break;
//					}
//				}
//			}
		}
	}

	public void AcquireTarget(){
		this.availableTargets.Clear();
		this._targetHextile = null;
		for (int i = 0; i < this.tilesInRadius.Count; i++) {
			if(this.tilesInRadius[i] != null){
				if(this.tilesInRadius[i].isOccupied && this.tilesInRadius[i].isHabitable && this.tilesInRadius[i].city.id != 0){
					if(this.hexTile.tag == this.tilesInRadius[i].tag){
						this.availableTargets.Add(this.tilesInRadius[i]);
					}
				}
			}
		}
		if(this.availableTargets.Count > 0){
			this._targetHextile = this.availableTargets[UnityEngine.Random.Range(0, this.availableTargets.Count)];
		}
	}

	#region Virtual
	public virtual void Initialize(){}
	public virtual void EverydayAction(){
		this.daysCounter += 1;
	}
	#endregion
}
