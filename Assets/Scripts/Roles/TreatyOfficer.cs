using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TreatyOfficer : Role {

	public TreatyOfficer(Citizen citizen): base(citizen){
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize (gameEvent);
		this.avatar.GetComponent<TreatyOfficerAvatar>().Init(this);
	}
//	internal override void Attack (){
////		base.Attack ();
//		if(this.avatar != null){
//			this.avatar.GetComponent<RaiderAvatar> ().HasAttacked();
//			if(this.avatar.GetComponent<RaiderAvatar> ().direction == DIRECTION.LEFT){
//				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Left");
//			}else if(this.avatar.GetComponent<RaiderAvatar> ().direction == DIRECTION.RIGHT){
//				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Right");
//			}else if(this.avatar.GetComponent<RaiderAvatar> ().direction == DIRECTION.UP){
//				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Up");
//			}else{
//				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Down");
//			}
//		}
//	}
}
