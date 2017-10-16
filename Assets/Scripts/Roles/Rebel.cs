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
//		this.gameEvent = gameEvent;
//		if(gameEvent is Riot){
//            base.Initialize(gameEvent);
//			Riot riot = (Riot)this.gameEvent;
//			riot.rebel = this;
////			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Reinforcer"), this.citizen.city.hexTile.transform) as GameObject;
////			this.avatar.transform.localPosition = Vector3.zero;
////			this.avatar.GetComponent<ReinforcerAvatar>().Init(this);
//		}
//		else if(gameEvent is Rebellion){
//            base.Initialize(gameEvent);
//            Rebellion rebellion = (Rebellion)this.gameEvent;
//			rebellion.rebelLeader = this;
//		}
	}
		
}
