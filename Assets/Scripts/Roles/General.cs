﻿using UnityEngine;
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
	public int damage;
	public AttackCity attackCity;
	public bool markAsDead;
	public General(Citizen citizen): base(citizen){
//		this.location = citizen.city.hexTile;
//		this.daysBeforeMoving = citizen.city.hexTile.movementDays;
//		this.targetLocation = null;
		this.damage = UnityEngine.Random.Range(60,101);
		this.markAsDead = false;
		this.attackCity = null;
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
		if(gameEvent is AttackCity){
			this.attackCity = (AttackCity)gameEvent;
			this.attackCity.general = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/General"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<GeneralAvatar>().Init(this);
		}
	}

	internal override void Attack (){
		//		base.Attack ();
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
}
