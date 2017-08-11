using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Ranger : Role {
	private object _targetObject;
	private List<HexTile> occupiedTiles;
	internal HexTile prevLocation;

	public object targetObject{
		get{return this._targetObject;}
	}

	public Ranger(Citizen citizen) : base(citizen) {
		this.damage = 50 + (10 * (this.citizen.city.kingdom.techLevel - 1));
		this._targetObject = null;
		this.prevLocation = null;
		this.occupiedTiles = new List<HexTile> ();
    }

	#region Overrides
    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is HuntLair) {
            base.Initialize(gameEvent);
			this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Ranger"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<RangerAvatar>().Init(this);
			if(this.targetLocation == null){
				AcquireTarget ();
			}else{
				this._targetObject = this.targetLocation.lair;
			}
        }
    }
	#endregion

	internal void AcquireTarget(){
		RoamingBehavior ();
	}
	private void RoamingBehavior(){
		if(this._targetObject != null){
			if(this._targetObject is Lair){
				Lair lair = (Lair)this._targetObject;
				if(!lair.isDead){
					return;
				}
			}else if(this._targetObject is Monster){
				Monster monster = (Monster)this._targetObject;
				if(!monster.isDead && monster.avatar != null){
					this.targetLocation = monster.location;
					this.path = PathGenerator.Instance.GetPath (this.location, this.targetLocation, PATHFINDING_MODE.AVATAR);
					return;
				}
			}
		}
		this._targetObject = null;
		this.occupiedTiles.Clear ();
		List<HexTile> tileRadius = this.location.GetTilesInRange(2);
		for (int i = 0; i < tileRadius.Count; i++) {
			if(tileRadius[i].lair != null){ //tileRadius[i].citizensOnTile.Count > 0
				this.occupiedTiles.Add (tileRadius [i]);
			}
		}
		if(this.occupiedTiles.Count > 0){
			this.targetLocation = this.occupiedTiles [UnityEngine.Random.Range (0, this.occupiedTiles.Count)];
			this._targetObject = this.targetLocation.lair;
//			int chance = UnityEngine.Random.Range (0, 2);
//			if(chance == 0){
//				if(this.targetLocation.isOccupied && this.targetLocation.city.id != 0){
//					this._targetObject = this.targetLocation.city;
//				}else{
//					this._targetObject = this.targetLocation.citizensOnTile[UnityEngine.Random.Range(0, this.targetLocation.citizensOnTile.Count)];
//				}
//			}else{
//				if(this.targetLocation.citizensOnTile.Count > 0){
//					this._targetObject = this.targetLocation.citizensOnTile[UnityEngine.Random.Range(0, this.targetLocation.citizensOnTile.Count)];
//				}else{
//					this._targetObject = this.targetLocation.city;
//				}
//			}
		}else{
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

}
