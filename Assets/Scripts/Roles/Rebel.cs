using UnityEngine;
using System.Collections;

public class Rebel : Role {
	public GameEvent gameEvent;
	internal bool isLeader;
	public Rebel(Citizen citizen): base(citizen){
		this.gameEvent = null;
		this.isLeader = false;
	}
	internal override void Initialize(GameEvent gameEvent){
		this.gameEvent = gameEvent;
		if(gameEvent is Riot){
            base.Initialize(gameEvent);
			Riot riot = (Riot)this.gameEvent;
			riot.rebel = this;
//			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Reinforcer"), this.citizen.city.hexTile.transform) as GameObject;
//			this.avatar.transform.localPosition = Vector3.zero;
//			this.avatar.GetComponent<ReinforcerAvatar>().Init(this);
		}else if(gameEvent is Rebellion){
            base.Initialize(gameEvent);
            Rebellion rebellion = (Rebellion)this.gameEvent;
			rebellion.rebelLeader = this;
		}
	}

//	internal override void Attack (){
//		//		base.Attack ();
//		if(this.avatar != null){
//			this.avatar.GetComponent<ReinforcerAvatar> ().HasAttacked();
//			if(this.avatar.GetComponent<ReinforcerAvatar> ().direction == DIRECTION.LEFT){
//				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Left");
//			}else if(this.avatar.GetComponent<ReinforcerAvatar> ().direction == DIRECTION.RIGHT){
//				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Right");
//			}else if(this.avatar.GetComponent<ReinforcerAvatar> ().direction == DIRECTION.UP){
//				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Up");
//			}else{
//				this.avatar.GetComponent<ReinforcerAvatar> ().animator.Play ("Attack_Down");
//			}
//		}
//	}
}
