using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Raider : Role {
	public Raid raid;
	public HexTile location;
	public HexTile targetLocation;
	public List<HexTile> path;

	internal GameObject raidAvatar;
	internal int daysBeforeMoving;

	public Raider(Citizen citizen): base(citizen){
		this.location = citizen.city.hexTile;
		this.raid = null;
		this.targetLocation = null;
		this.path = new List<HexTile> ();
		this.raidAvatar = null;
		this.daysBeforeMoving = 0;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Raid){
			this.raid = (Raid)gameEvent;
			this.raid.raider = this;
			this.targetLocation = this.raid.raidedCity.hexTile;
			this.path = PathGenerator.Instance.GetPath (this.citizen.city.hexTile, this.raid.raidedCity.hexTile, PATHFINDING_MODE.COMBAT).ToList();
			this.daysBeforeMoving = this.path [0].movementDays;
			this.raidAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/Raider"), this.citizen.city.hexTile.transform) as GameObject;
			this.raidAvatar.transform.localPosition = Vector3.zero;
			this.raidAvatar.GetComponent<RaiderAvatar>().Init(this);
		}
	}

	internal override void DestroyGO(){
		GameObject.Destroy (this.raidAvatar);
	}
}
