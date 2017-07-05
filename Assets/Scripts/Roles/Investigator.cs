using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Investigator : Role {
	public FirstAndKeystone firstAndKeystone;

	public Investigator(Citizen citizen): base(citizen){
		this.firstAndKeystone = null;
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is FirstAndKeystone){
            base.Initialize(gameEvent);
			this.firstAndKeystone = (FirstAndKeystone)gameEvent;
//			this.firstAndKeystone.raider = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Investigator"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<InvestigatorAvatar>().Init(this);
		}
	}
	internal override void Attack (){
//		base.Attack ();
		if(this.avatar != null){
			this.avatar.GetComponent<InvestigatorAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<InvestigatorAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<InvestigatorAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<InvestigatorAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<InvestigatorAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<InvestigatorAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<InvestigatorAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<InvestigatorAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}
}
