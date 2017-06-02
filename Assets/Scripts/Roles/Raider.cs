using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Raider : Role {
	public Raid raid;

	public Raider(Citizen citizen): base(citizen){
		this.raid = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Raid){
			this.raid = (Raid)gameEvent;
			this.raid.raider = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Raider"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<RaiderAvatar>().Init(this);
		}
	}
	internal override void Attack (){
//		base.Attack ();
		if(this.avatar != null){
			this.avatar.GetComponent<RaiderAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<RaiderAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<RaiderAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<RaiderAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}
}
