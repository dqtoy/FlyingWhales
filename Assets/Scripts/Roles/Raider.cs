using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Raider : Role {
//	public Raid raid;

	public Raider(Citizen citizen): base(citizen){
//		this.raid = null;
	}

	internal override void Initialize(GameEvent gameEvent){
//		if(gameEvent is Raid){
//            base.Initialize(gameEvent);
//			this.raid = (Raid)gameEvent;
//			this.raid.raider = this;
//			this.avatar.GetComponent<RaiderAvatar>().Init(this);
//		}
	}
}
