using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Missionary : Role {
	public Evangelism evangelism;

	public Missionary(Citizen citizen): base(citizen){
		this.evangelism = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Evangelism){
			this.evangelism = (Evangelism)gameEvent;
			this.evangelism.missionary = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Missionary"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<MissionaryAvatar>().Init(this);
		}
	}
	internal override void Attack (){
//		base.Attack ();
		if(this.avatar != null){
			this.avatar.GetComponent<MissionaryAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<MissionaryAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<MissionaryAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<MissionaryAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<MissionaryAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<MissionaryAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<MissionaryAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<MissionaryAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}
}
