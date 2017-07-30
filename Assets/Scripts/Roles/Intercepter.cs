using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Intercepter : Role {
	public SendReliefGoods sendReliefGoods;

	public Intercepter(Citizen citizen): base(citizen){
		this.sendReliefGoods = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is SendReliefGoods){
            base.Initialize(gameEvent);
			this.sendReliefGoods = (SendReliefGoods)gameEvent;
			this.sendReliefGoods.intercepter = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Intercepter"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<IntercepterAvatar>().Init(this);
		}
	}
	internal override void Attack (){
//		base.Attack ();
		if(this.avatar != null){
			this.avatar.GetComponent<IntercepterAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<IntercepterAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<IntercepterAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<IntercepterAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<IntercepterAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<IntercepterAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<IntercepterAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<IntercepterAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}
}
