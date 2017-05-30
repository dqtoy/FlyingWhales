using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expander : Role {
	public Expansion expansion;
	public HexTile location;
	public HexTile targetLocation;
	public List<HexTile> path;

	internal GameObject expansionAvatar;
	internal int daysBeforeMoving;

	public Expander(Citizen citizen): base(citizen){
		this.location = citizen.city.hexTile;
		this.expansion = null;
		this.targetLocation = null;
		this.path = new List<HexTile> ();
		this.expansionAvatar = null;
		this.daysBeforeMoving = 0;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Expansion){
			this.expansion = (Expansion)gameEvent;
			this.expansion.expander = this;
			this.targetLocation = this.expansion.hexTileToExpandTo;
			this.path = PathGenerator.Instance.GetPath (this.citizen.city.hexTile, this.expansion.hexTileToExpandTo, PATHFINDING_MODE.COMBAT).ToList();
			this.daysBeforeMoving = this.path [0].movementDays;
			this.expansionAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/ExpansionAvatar"), this.citizen.city.hexTile.transform) as GameObject;
			this.expansionAvatar.transform.localPosition = Vector3.zero;
			this.expansionAvatar.GetComponent<ExpansionAvatar>().Init(this);
		}
	}

	internal override void DestroyGO(){
		GameObject.Destroy (this.expansionAvatar);
	}
}
