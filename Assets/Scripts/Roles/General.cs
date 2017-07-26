using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class General : Role {
//	public HexTile location;
//	public HexTile targetLocation;
//	public List<HexTile> roads;
//	public int daysBeforeArrival;
//	public int daysBeforeReleaseTask;
//	public GameObject generalAvatar;
//	public int battlesWon;
//	public int citiesInvaded;
//	public int successfulRaids;
//	public int unsuccessfulRaids;
//	public bool inAction;
//	public bool isGoingHome;
//
//	internal Citizen target;
//	public int daysCounter = 0;
//	public int daysBeforeMoving;
	public GameEvent gameEvent;
	internal bool isRebel;
	internal int spawnRate;
    private int _weaponCount;

	private bool _hasSerumOfAlacrity;


	#region Getters and Setters
	public bool hasSerumOfAlacrity{
		get{return this._hasSerumOfAlacrity;}
	}

	#endregion
    public General(Citizen citizen): base(citizen){
//		this.location = citizen.city.hexTile;
//		this.daysBeforeMoving = citizen.city.hexTile.movementDays;
//		this.targetLocation = null;
		this.spawnRate = 0;
		this.gameEvent = null;
		this.isRebel = false;
		this._hasSerumOfAlacrity = false;
//		this.daysBeforeArrival = 0;
//		this.daysBeforeReleaseTask = 0;
//		this.roads = new List<HexTile> ();
//		this.battlesWon = 0;
//		this.citiesInvaded = 0;
//		this.successfulRaids = 0;
//		this.unsuccessfulRaids = 0;
//		this.inAction = false;
//		this.isGoingHome = false;
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize(gameEvent);
		this.gameEvent = gameEvent;
		if(gameEvent is AttackCity){
			((AttackCity)this.gameEvent).general = this;
		}else if(gameEvent is AttackLair){
			((AttackLair)this.gameEvent).general = this;
		}
		this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/General"), this.citizen.city.hexTile.transform) as GameObject;
		this.avatar.transform.localPosition = Vector3.zero;
		this.avatar.GetComponent<GeneralAvatar>().Init(this);
	}

	internal override void Attack (){
		//		base.Attack ();

//		if(this.attackCity != null){
//			if(this.damage >= this.attackCity.targetCity.hp){
//				if(this.attackCity.gameEvent is Rebellion){
//					((Rebellion)this.attackCity.gameEvent).warPair.isDone = true;
//				}else if(this.attackCity.gameEvent is War){
//					((War)this.attackCity.gameEvent).warPair.isDone = true;
//				}
//			}
//		}
		if(this.avatar != null){
			this.avatar.GetComponent<GeneralAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<GeneralAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<GeneralAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<GeneralAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<GeneralAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<GeneralAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<GeneralAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<GeneralAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}

	internal int GetDamage(){
		int baseDamage = UnityEngine.Random.Range (50, 81) + (_weaponCount * 100);
		int cityDamage = (6 + this.citizen.city.kingdom.techLevel) * (UnityEngine.Random.Range (0, this.citizen.city.ownedTiles.Count));
		int otherCityTileCount = 0;
		for (int i = 0; i < this.citizen.city.kingdom.cities.Count; i++) {
			if(this.citizen.city.kingdom.cities[i].id != this.citizen.city.id){
				otherCityTileCount += this.citizen.city.kingdom.cities [i].ownedTiles.Count;
			}
		}
		int otherCityDamage = 1 * otherCityTileCount;
		int spawnRateDamage = Mathf.CeilToInt((float)this.spawnRate / 6f);
		return (baseDamage + cityDamage + otherCityDamage) * spawnRateDamage;
	}

    #region Weapons
    internal void AdjustWeaponCount(int adjustment) {
        _weaponCount += adjustment;
    }
    #endregion

	#region Serum of Alacrity
	internal void InjectSerumOfAlacrity(){
		this._hasSerumOfAlacrity = true;
	}
	internal void CheckSerumOfAlacrity(){
		if(this._hasSerumOfAlacrity){
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 5){
				this.citizen.Death (DEATH_REASONS.SERUM_OF_ALACRITY);
			}
		}
	}
	#endregion
}
