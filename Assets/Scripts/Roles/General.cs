using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class General : Role {
	public HexTile location;
	public HexTile targetLocation;
	public List<HexTile> roads;
	public int daysBeforeArrival;
	public int daysBeforeReleaseTask;
	public GameObject generalAvatar;
	public int battlesWon;
	public int citiesInvaded;
	public int successfulRaids;
	public int unsuccessfulRaids;
	public bool inAction;
	public bool isGoingHome;

	internal Citizen target;
	public int daysCounter = 0;
	public int daysBeforeMoving;
	public int damage;

	public General(Citizen citizen): base(citizen){
		this.location = citizen.city.hexTile;
		this.daysBeforeMoving = citizen.city.hexTile.movementDays;
		this.targetLocation = null;
		this.damage = UnityEngine.Random.Range(30,51);
		this.daysBeforeArrival = 0;
		this.daysBeforeReleaseTask = 0;
		this.roads = new List<HexTile> ();
		this.battlesWon = 0;
		this.citiesInvaded = 0;
		this.successfulRaids = 0;
		this.unsuccessfulRaids = 0;
		this.inAction = false;
		this.isGoingHome = false;
	}
}
