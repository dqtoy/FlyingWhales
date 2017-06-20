using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Trader : Role {
    private Trade _tradeEvent;
    private Plague _plague;

    #region getters/setters
    public Trade tradeEvent {
        get { return this._tradeEvent;  }
    }
    public Plague plague {
        get { return this._plague; }
    }
    #endregion

    public Trader(Citizen citizen): base(citizen){
		this._tradeEvent = null;
	}

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Trade) {
            this._tradeEvent = (Trade)gameEvent;
            this.targetLocation = this._tradeEvent.targetCity.hexTile;
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Trader"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<TraderAvatar>().Init(this);
            /*
            * Whenever a Trader agent comes out of a plagued city, 
            * there is a 5% chance for every plagued settlement that he carries the plague
            * */
            if(this.citizen.city.plague != null) {
                int numOfInfectedSettlements = this.citizen.city.plaguedSettlements.Count;
                int chanceToPlagueTrader = 5 * numOfInfectedSettlements;
                if (Random.Range(0, 100) < chanceToPlagueTrader) {
                    this.InfectWithPlague(this.citizen.city.plague);
                }
            }
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

    internal void InfectWithPlague(Plague plague) {
        this._plague = plague;
    }

    internal void DisinfectPlague() {
        this._plague = null;
    }
}
