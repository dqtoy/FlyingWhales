using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Thief : Role {
	public FirstAndKeystone firstAndKeystone;

	public Thief(Citizen citizen): base(citizen){
		this.firstAndKeystone = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is FirstAndKeystone){
            base.Initialize(gameEvent);
			this.firstAndKeystone = (FirstAndKeystone)gameEvent;
//			this.firstAndKeystone.raider = this;
			this.avatar.GetComponent<ThiefAvatar>().Init(this);
		}
	}
//	internal override void Attack (){
////		base.Attack ();
//		if(this.avatar != null){
//			this.avatar.GetComponent<ThiefAvatar> ().HasAttacked();
//			if(this.avatar.GetComponent<ThiefAvatar> ().direction == DIRECTION.LEFT){
//				this.avatar.GetComponent<ThiefAvatar> ().animator.Play ("Attack_Left");
//			}else if(this.avatar.GetComponent<ThiefAvatar> ().direction == DIRECTION.RIGHT){
//				this.avatar.GetComponent<ThiefAvatar> ().animator.Play ("Attack_Right");
//			}else if(this.avatar.GetComponent<ThiefAvatar> ().direction == DIRECTION.UP){
//				this.avatar.GetComponent<ThiefAvatar> ().animator.Play ("Attack_Up");
//			}else{
//				this.avatar.GetComponent<ThiefAvatar> ().animator.Play ("Attack_Down");
//			}
//		}
//	}
}
