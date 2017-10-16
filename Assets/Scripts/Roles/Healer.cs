using UnityEngine;
using System.Collections;

public class Healer : Role {
    private Plague _plagueEvent;

    #region getters/setters
    public Plague plagueEvent {
        get { return this._plagueEvent; }
    }
    #endregion

    public Healer(Citizen citizen): base(citizen){

    }

    internal override void Initialize(GameEvent gameEvent) {
        //if (gameEvent is Plague) {
        //    base.Initialize(gameEvent);
        //    this._plagueEvent = (Plague)gameEvent;
        //    this._plagueEvent.AddAgentToList(this.citizen);
        //    this.avatar.GetComponent<HealerAvatar>().Init(this);
        //}
    }
}
