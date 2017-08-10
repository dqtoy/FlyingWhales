using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Reliever : Role {
	public SendReliefGoods sendReliefGoods;

	public Reliever(Citizen citizen): base(citizen){
		this.sendReliefGoods = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is SendReliefGoods){
            base.Initialize(gameEvent);
			this.sendReliefGoods = (SendReliefGoods)gameEvent;
			this.sendReliefGoods.reliever = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Reliever"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<RelieverAvatar>().Init(this);
		}
	}
//	internal override void Attack (){
////		base.Attack ();
//		if(this.avatar != null){
//			this.avatar.GetComponent<RelieverAvatar> ().HasAttacked();
//			if(this.avatar.GetComponent<RelieverAvatar> ().direction == DIRECTION.LEFT){
//				this.avatar.GetComponent<RelieverAvatar> ().animator.Play ("Attack_Left");
//			}else if(this.avatar.GetComponent<RelieverAvatar> ().direction == DIRECTION.RIGHT){
//				this.avatar.GetComponent<RelieverAvatar> ().animator.Play ("Attack_Right");
//			}else if(this.avatar.GetComponent<RelieverAvatar> ().direction == DIRECTION.UP){
//				this.avatar.GetComponent<RelieverAvatar> ().animator.Play ("Attack_Up");
//			}else{
//				this.avatar.GetComponent<RelieverAvatar> ().animator.Play ("Attack_Down");
//			}
//		}
//	}
}
