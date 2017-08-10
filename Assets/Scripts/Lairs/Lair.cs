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

	#region getters/setters
	public LairSpawn lairSpawn{
		get{return this._lairSpawn;}
	}

	#endregion
	public Lair(LAIR type, HexTile hexTile){
		this.type = type;
		this.name = "Lair";
		this.hexTile = hexTile;
		this._lairSpawn = MonsterManager.Instance.GetLairSpawnData (this.type);
		this.hp = this._lairSpawn.lairHP;
		this.maxHP = this.hp;
		this.spawnRate = this._lairSpawn.spawnRate;
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

	internal void SummonMonster(MONSTER monster){
		MonsterManager.Instance.SummonNewMonster(monster, this.hexTile);

//		HexTile target = AcquireTarget(null, this.hexTile);
//		if(target != null){
//		}
	}

	private HexTile GetHexTileByCoordinatesFromPool(int x, int y, HexTile[] pool){
		for (int i = 0; i < pool.Length; i++) {
			if(pool[i].xCoordinate == x && pool[i].yCoordinate == y){
				return pool [i];
			}
		}
		return null;
	}

	private int GetIndexOfOppositeTile(HexTile fromHextile, HexTile prevHextile){
		HexTile[] neighbors = fromHextile.AllNeighbours.ToArray ();
		List<Point> points = new List<Point>();

		int xOppositeIndex = -1;
		int yOppositeIndex = -1;

		if(fromHextile.yCoordinate % 2 == 0){
			points = Utilities.EvenNeighbours;
		}else{
			points = Utilities.OddNeighbours;
		}
		int xDiff = prevHextile.xCoordinate - fromHextile.xCoordinate;
		int yDiff = prevHextile.yCoordinate - fromHextile.yCoordinate;
		for (int i = 0; i < points.Count; i++) {
			if(points[i].X == xDiff && points[i].Y == yDiff){
				int oppositeIndex = GetOppositeIndex (i);
				xOppositeIndex = fromHextile.xCoordinate + points [oppositeIndex].X;
				yOppositeIndex = fromHextile.yCoordinate + points [oppositeIndex].Y;
				break;
			}
		}
		for (int i = 0; i < neighbors.Length; i++) {
			if(neighbors[i].xCoordinate == xOppositeIndex && neighbors[i].yCoordinate == yOppositeIndex){
				return i;
			}
		}
		return -1;
	}
	private int GetOppositeIndex(int index){
		if(index == 0){
			return 3;
		}else if(index == 1){
			return 4;
		}else if(index == 2){
			return 5;
		}else if(index == 3){
			return 0;
		}else if(index == 4){
			return 1;
		}else if(index == 5){
			return 2;
		}
		return 0;
	}
	#region Virtual
	public virtual void Initialize(){}
	public virtual void EverydayAction(){
		this.daysCounter += 1;
	}
	#endregion
}
