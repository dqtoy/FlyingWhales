using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Envoy : Role {
	public GameEvent gameEvent;


	public Envoy(Citizen citizen): base(citizen){

	}

	internal override void Initialize(GameEvent gameEvent){
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
		}
		this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Envoy"), this.citizen.city.hexTile.transform) as GameObject;
		this.avatar.transform.localPosition = Vector3.zero;
		this.avatar.GetComponent<EnvoyAvatar>().Init(this);
	}
}
