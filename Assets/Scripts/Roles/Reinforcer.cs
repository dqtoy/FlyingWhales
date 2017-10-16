using UnityEngine;
using System.Collections;

public class Reinforcer : Role {
//	public Reinforcement reinforcement;
	public int reinforcementValue;
	internal bool isRebel;

	public Reinforcer(Citizen citizen): base(citizen){
//		this.reinforcement = null;
		this.reinforcementValue = 0;
		this.isRebel = false;
	}
	internal override void Initialize(GameEvent gameEvent){
//		if(gameEvent is Reinforcement){
//            base.Initialize(gameEvent);
//			this.reinforcement = (Reinforcement)gameEvent;
//			this.reinforcement.reinforcer = this;
//			this.avatar.GetComponent<ReinforcerAvatar>().Init(this);
//		}
	}
}
