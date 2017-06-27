using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Envoy : Role {
	public GameEvent gameEvent;


	public Envoy(Citizen citizen): base(citizen){
		this.gameEvent = null;
	}

	internal override void Initialize(GameEvent gameEvent){
        base.Initialize(gameEvent);
		this.gameEvent = gameEvent;
		if(this.gameEvent is BorderConflict){
			BorderConflict bc = (BorderConflict)this.gameEvent;
			bc.activeEnvoyResolve = this;
		}else if(this.gameEvent is DiplomaticCrisis){
			DiplomaticCrisis crisis = (DiplomaticCrisis)this.gameEvent;
			crisis.activeEnvoyResolve = this;
		}else if(this.gameEvent is JoinWar){
			JoinWar joinWar = (JoinWar)this.gameEvent;
			joinWar.envoyToSend = this;
		}else if(this.gameEvent is StateVisit){
			StateVisit stateVisit = (StateVisit)this.gameEvent;
			stateVisit.visitor = this;
		}else if(this.gameEvent is RequestPeace) {
            //RequestPeace requestPeace = (RequestPeace)this.gameEvent;
            //requestPeace.SetEnvoySent(this);
		}else if(this.gameEvent is Secession){
			Secession secession = (Secession)this.gameEvent;
			secession.convincer = this;
		}
		this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Envoy"), this.citizen.city.hexTile.transform) as GameObject;
		this.avatar.transform.localPosition = Vector3.zero;
		this.avatar.GetComponent<EnvoyAvatar>().Init(this);
	}

	internal override void Attack (){
//		base.Attack ();
		if(this.avatar != null){
			this.avatar.GetComponent<EnvoyAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<EnvoyAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<EnvoyAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<EnvoyAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<EnvoyAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<EnvoyAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<EnvoyAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<EnvoyAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}
}
