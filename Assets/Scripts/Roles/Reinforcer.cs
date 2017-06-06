using UnityEngine;
using System.Collections;

public class Reinforcer : Role {
	public Reinforcement reinforcement;
	public int reinforcementValue;

	public Reinforcer(Citizen citizen): base(citizen){
		this.reinforcement = null;
		this.reinforcementValue = 0;
	}
	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Reinforcement){
			this.reinforcement = (Reinforcement)gameEvent;
			this.reinforcement.reinforcer = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Reinforcer"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<ReinforcerAvatar>().Init(this);
		}
	}

	internal override void Attack (){
		//		base.Attack ();
		if(this.avatar != null){
			this.avatar.GetComponent<ReinforcerAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<ReinforcerAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<ReinforcerAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<ReinforcerAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}
}
