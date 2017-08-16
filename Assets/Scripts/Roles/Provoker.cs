using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Provoker : Role {
	public Provocation provocation;

	public Provoker(Citizen citizen): base(citizen){
		this.provocation = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Provocation){
            base.Initialize(gameEvent);
			this.provocation = (Provocation)gameEvent;
			this.provocation.provoker = this;
			this.avatar.GetComponent<ProvokerAvatar>().Init(this);
		}
	}
//	internal override void Attack (){
////		base.Attack ();
//		if(this.avatar != null){
//			this.avatar.GetComponent<ProvokerAvatar> ().HasAttacked();
//			if(this.avatar.GetComponent<ProvokerAvatar> ().direction == DIRECTION.LEFT){
//				this.avatar.GetComponent<ProvokerAvatar> ().animator.Play ("Attack_Left");
//			}else if(this.avatar.GetComponent<ProvokerAvatar> ().direction == DIRECTION.RIGHT){
//				this.avatar.GetComponent<ProvokerAvatar> ().animator.Play ("Attack_Right");
//			}else if(this.avatar.GetComponent<ProvokerAvatar> ().direction == DIRECTION.UP){
//				this.avatar.GetComponent<ProvokerAvatar> ().animator.Play ("Attack_Up");
//			}else{
//				this.avatar.GetComponent<ProvokerAvatar> ().animator.Play ("Attack_Down");
//			}
//		}
//	}
}
