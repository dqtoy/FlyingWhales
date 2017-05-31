using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Raider : Role {
	public Raid raid;

	public Raider(Citizen citizen): base(citizen){

	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Raid){
			this.raid = (Raid)gameEvent;
			this.raid.raider = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Raider"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<RaiderAvatar>().Init(this);
		}
	}

}
