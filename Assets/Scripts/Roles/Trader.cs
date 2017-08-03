using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Trader : Role {

    public Trader(Citizen citizen): base(citizen){
		
	}

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Trade) {
            base.Initialize(gameEvent);
            this.targetLocation = ((Trade)this.gameEventInvolvedIn).targetCity.hexTile;
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Trader"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<TraderAvatar>().Init(this);
        }
    }

    /*
     * Called when citizen dies. See in Citizen.Death() Function.
     * */
    internal override void OnDeath() {
        if (((Trade)this.gameEventInvolvedIn) != null) {
            if (((Trade)this.gameEventInvolvedIn).isActive) {
                ((Trade)this.gameEventInvolvedIn).CancelEvent();
            }
        }
        base.OnDeath();
    }
    
}
