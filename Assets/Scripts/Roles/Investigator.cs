using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Investigator : Role {
//	public FirstAndKeystone firstAndKeystone;

	public Investigator(Citizen citizen): base(citizen){
//		this.firstAndKeystone = null;
	}

	internal override void Initialize(GameEvent gameEvent){
//		if(gameEvent is FirstAndKeystone){
//            base.Initialize(gameEvent);
//			this.firstAndKeystone = (FirstAndKeystone)gameEvent;
////			this.firstAndKeystone.raider = this;
//			this.avatar.GetComponent<InvestigatorAvatar>().Init(this);
//		}
	}
}
