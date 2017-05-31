using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Envoy : Role {
	public GameEvent gameEvent;
	public HexTile location;
	public HexTile targetLocation;
	public List<HexTile> path;

	internal GameObject envoyAvatar;
	internal int daysBeforeMoving;

	public Envoy(Citizen citizen): base(citizen){
		this.location = citizen.city.hexTile;
		this.gameEvent = null;
		this.targetLocation = null;
		this.path = new List<HexTile> ();
		this.envoyAvatar = null;
		this.daysBeforeMoving = 0;
	}

	internal override void Initialize(GameEvent gameEvent, List<HexTile> path){
		this.gameEvent = gameEvent;
		if(this.gameEvent is BorderConflict){
			BorderConflict bc = (BorderConflict)this.gameEvent;
			bc.activeEnvoyResolve = this;
			this.targetLocation = bc.targetCity.hexTile;
		}else if(this.gameEvent is DiplomaticCrisis){
			DiplomaticCrisis crisis = (DiplomaticCrisis)this.gameEvent;
			crisis.activeEnvoyResolve = this;
			this.targetLocation = crisis.targetCity.hexTile;
		}else if(this.gameEvent is JoinWar){
			JoinWar joinWar = (JoinWar)this.gameEvent;
			joinWar.envoyToSend = this;
			this.targetLocation = joinWar.candidateForAlliance.city.hexTile;
		}else if(this.gameEvent is StateVisit){
			StateVisit stateVisit = (StateVisit)this.gameEvent;
			stateVisit.visitor = this;
			this.targetLocation = stateVisit.inviterKingdom.capitalCity.hexTile;
		}
		this.path = path;
		this.daysBeforeMoving = this.path [0].movementDays;
		this.envoyAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/Envoy"), this.citizen.city.hexTile.transform) as GameObject;
		this.envoyAvatar.transform.localPosition = Vector3.zero;
		this.envoyAvatar.GetComponent<EnvoyAvatar>().Init(this);
	}
	internal override void Attack(){
		if(this.envoyAvatar != null){
			if(this.envoyAvatar.GetComponent<EnvoyAvatar> ().isDirectionUp){
				this.envoyAvatar.GetComponent<EnvoyAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.envoyAvatar.GetComponent<EnvoyAvatar> ().animator.Play ("Attack");
			}
		}
	}
	internal override void DestroyGO(){
		if(this.envoyAvatar != null){
			GameObject.Destroy (this.envoyAvatar);
		}
	}
}
