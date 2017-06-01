using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Trader : Role {
    private Trade _tradeEvent;

    #region getters/setters
    public Trade tradeEvent {
        get { return this._tradeEvent;  }
    }
    #endregion

    public Trader(Citizen citizen): base(citizen){
		
	}

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Trade) {
            this._tradeEvent = (Trade)gameEvent;
            this.targetLocation = this._tradeEvent.targetKingdom.capitalCity.hexTile;
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Trader"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<TraderAvatar>().Init(this);
        }
    }

    /*
     * Called when citizen dies. See in Citizen.Death() Function.
     * */
    internal override void OnDeath() {
        if (this._tradeEvent != null) {
            if (this._tradeEvent.isActive) {
                this._tradeEvent.CancelEvent();
            }
        }
        this.DestroyGO();
    }
}
