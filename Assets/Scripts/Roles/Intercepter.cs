using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Intercepter : Role {
//	public SendReliefGoods sendReliefGoods;

	public Intercepter(Citizen citizen): base(citizen){
//		this.sendReliefGoods = null;
	}

	internal override void Initialize(GameEvent gameEvent){
//		if(gameEvent is SendReliefGoods){
//            base.Initialize(gameEvent);
//			this.sendReliefGoods = (SendReliefGoods)gameEvent;
//			this.sendReliefGoods.intercepter = this;
//			this.avatar.GetComponent<IntercepterAvatar>().Init(this);
//		}
	}
}
