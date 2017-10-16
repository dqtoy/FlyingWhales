using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Provoker : Role {
//	public Provocation provocation;

	public Provoker(Citizen citizen): base(citizen){
//		this.provocation = null;
	}

	internal override void Initialize(GameEvent gameEvent){
//		if(gameEvent is Provocation){
//            base.Initialize(gameEvent);
//			this.provocation = (Provocation)gameEvent;
//			//this.provocation.provoker = this;
//			this.avatar.GetComponent<ProvokerAvatar>().Init(this);
//		}
	}
}
