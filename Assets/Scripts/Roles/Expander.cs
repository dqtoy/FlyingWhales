using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expander : Role {
	public Expansion expansion;

	public Expander(Citizen citizen): base(citizen){
		this.expansion = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Expansion){
			this.expansion = (Expansion)gameEvent;
			this.expansion.expander = this;
			this.targetLocation = this.expansion.hexTileToExpandTo;
			this.path = path;
			this.daysBeforeMoving = this.path [0].movementDays;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/ExpansionAvatar"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<ExpansionAvatar>().Init(this);
		}
	}
}
