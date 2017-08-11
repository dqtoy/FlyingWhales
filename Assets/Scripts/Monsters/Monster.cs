using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Monster {
	public delegate void onAcquireTarget ();
	public static event onAcquireTarget onAcquireTargetAction;

	public MONSTER type;
	public int hp;
	public GameObject avatar;
	public HexTile originHextile;
	public HexTile targetLocation;
	public HexTile location;
	public HexTile prevLocation;
	public bool markAsDead;
	public bool isDead;
	public List<HexTile> path;

	public Lair lair;

	internal object _targetObject;
	internal List<HexTile> occupiedTiles;


	public Monster(MONSTER type, HexTile originHextile){
		this.type = type;
		this.lair = originHextile.lair;
		this.hp = this.lair.lairSpawn.monsterHP;
		this.avatar = null;
		this.originHextile = originHextile;
		this.location = originHextile;
		this.targetLocation = null;
		this.prevLocation = null;
		this._targetObject = null;
		this.markAsDead = false;
		this.isDead = false;
		this.path = new List<HexTile>();
		this.occupiedTiles = new List<HexTile> ();
		RegisterBehavior ();
	}
	internal void UpdateUI(){
		if(this.avatar != null){
			this.avatar.GetComponent<MonsterAvatar>().UpdateUI();
		}
	}
	internal void RegisterBehavior(){
		switch (this.lair.lairSpawn.behavior){
		case BEHAVIOR.HOMING:
			onAcquireTargetAction += HomingBehavior;
			break;
		case BEHAVIOR.ROAMING:
			onAcquireTargetAction += RoamingBehavior;
			break;
		}
		AcquireTarget ();
	}
	internal void AcquireTarget (){
		if(onAcquireTargetAction != null){
			onAcquireTargetAction ();
		}
	}
	internal void ClearAcquireTargetSubscriptions(){
		onAcquireTargetAction = null;
	}
	private void HomingBehavior(){
		this.lair.availableTargets.Clear();
		for (int i = 0; i < this.lair.tilesInRadius.Count; i++) {
			if(this.lair.tilesInRadius[i] != null){
				if(this.lair.tilesInRadius[i].isOccupied && this.lair.tilesInRadius[i].isHabitable && (this.lair.tilesInRadius[i].city != null && this.lair.tilesInRadius[i].city.id != 0)){
					if(this.lair.hexTile.tag == this.lair.tilesInRadius[i].tag){
						this.lair.availableTargets.Add(this.lair.tilesInRadius[i]);
					}
				}
			}
		}
		if(this.lair.availableTargets.Count > 0){
			this.targetLocation = this.lair.availableTargets[UnityEngine.Random.Range(0, this.lair.availableTargets.Count)];
		}
		this.path = PathGenerator.Instance.GetPath (this.location, this.targetLocation, PATHFINDING_MODE.AVATAR);
		onAcquireTargetAction -= HomingBehavior;
	}
	private void RoamingBehavior(){
		if(this._targetObject != null){
			if(this._targetObject is City){
				City city = (City)this._targetObject;
				if(!city.isDead){
					return;
				}
			}else if(this._targetObject is Citizen){
				Citizen citizen = (Citizen)this._targetObject;
				if(!citizen.isDead && citizen.assignedRole.avatar != null){
					this.targetLocation = citizen.assignedRole.location;
					this.path = PathGenerator.Instance.GetPath (this.location, this.targetLocation, PATHFINDING_MODE.AVATAR);
					return;
				}
			}
		}
		this._targetObject = null;
		this.occupiedTiles.Clear ();
		List<HexTile> tileRadius = this.location.GetTilesInRange(2);
		for (int i = 0; i < tileRadius.Count; i++) {
			if(tileRadius[i].isHabitable && tileRadius[i].isOccupied && (tileRadius[i].city != null && tileRadius[i].city.id != 0) || tileRadius[i].citizensOnTile.Count > 0) {
                this.occupiedTiles.Add (tileRadius [i]);
			}
		}
		if(this.occupiedTiles.Count > 0){
			this.targetLocation = this.occupiedTiles [UnityEngine.Random.Range (0, this.occupiedTiles.Count)];
            //this._targetObject = this.targetLocation.city;

            int chance = UnityEngine.Random.Range(0, 2);
            if (chance == 0) {
                if (this.targetLocation.isHabitable && this.targetLocation.isOccupied && (this.targetLocation.city != null && this.targetLocation.city.id != 0)) {
                } else {
                    this._targetObject = this.targetLocation.citizensOnTile[UnityEngine.Random.Range(0, this.targetLocation.citizensOnTile.Count)];
                }
            } else {
                if (this.targetLocation.citizensOnTile.Count > 0) {
                    this._targetObject = this.targetLocation.citizensOnTile[UnityEngine.Random.Range(0, this.targetLocation.citizensOnTile.Count)];
                } else {
                    this._targetObject = this.targetLocation.city;
                }
            }
        } else{
			HexTile[] neighbors = this.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER).ToArray ();
			int numOfNeighbors = neighbors.Length;
			if(this.prevLocation != null){

				int indexOfOppositeTile = GetIndexOfOppositeTile(this.location, this.prevLocation, neighbors);
				if(indexOfOppositeTile == -1){
					indexOfOppositeTile = UnityEngine.Random.Range (0, numOfNeighbors);
				}
				int chance = UnityEngine.Random.Range (0, 100);
				if(chance < 50){
					this.targetLocation = neighbors [indexOfOppositeTile];
				}else{
					int adder = 0;
					if(chance >= 50 && chance < 65){
						adder = 1;
					}else if(chance >= 65 && chance < 73){
						adder = 2;
					}else if(chance >= 73 && chance < 77){
						adder = 3;
					}else if(chance >= 77 && chance < 85){
						adder = 4;
					}else{
						adder = 5;
					}
					if(adder >= numOfNeighbors){
						adder = numOfNeighbors - 1;
					}
					int index = indexOfOppositeTile + adder;
					if(index >= numOfNeighbors){
						index -= numOfNeighbors;
					}
					this.targetLocation = neighbors [index];
				}

			}else{
				this.targetLocation = neighbors [UnityEngine.Random.Range (0, numOfNeighbors)];
			}
		}

		this.path = PathGenerator.Instance.GetPath (this.location, this.targetLocation, PATHFINDING_MODE.AVATAR);
	}
	private int GetIndexOfOppositeTile(HexTile fromHextile, HexTile prevHextile, HexTile[] neighbors){
//		HexTile[] neighbors = fromHextile.AllNeighbours.ToArray ();
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
//				return GetOppositeIndex (i);
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
	internal virtual void Initialize(){}
	internal virtual void Attack(){
		if(this.avatar != null){
			this.avatar.GetComponent<MonsterAvatar> ().HasAttacked();
			if (this.avatar.GetComponent<MonsterAvatar> ().animator.gameObject.activeSelf) {
				if (this.avatar.GetComponent<MonsterAvatar> ().direction == DIRECTION.LEFT) {
					this.avatar.GetComponent<MonsterAvatar> ().animator.Play ("Attack_Left");
				} else if (this.avatar.GetComponent<MonsterAvatar> ().direction == DIRECTION.RIGHT) {
					this.avatar.GetComponent<MonsterAvatar> ().animator.Play ("Attack_Right");
				} else if (this.avatar.GetComponent<MonsterAvatar> ().direction == DIRECTION.UP) {
					this.avatar.GetComponent<MonsterAvatar> ().animator.Play ("Attack_Up");
				} else {
					this.avatar.GetComponent<MonsterAvatar> ().animator.Play ("Attack_Down");
				}
			}else{
				this.avatar.GetComponent<MonsterAvatar> ().OnEndAttack ();
			}
		}
	}
	internal virtual void Death(){
		this.isDead = true;
		this.lair.AdjustActiveMonsterCount (-1);
	}

	internal virtual void DoneAction(){}
	#endregion
}