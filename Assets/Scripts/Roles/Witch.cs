using UnityEngine;
using System.Collections;

public class Witch : Role {

    public Witch(Citizen citizen) : base(citizen) {

    }

    #region Overrides
    internal override void Initialize(GameEvent gameEvent) {
//        if(gameEvent is Hypnotism) {
//            base.Initialize(gameEvent);
//            this.avatar.GetComponent<WitchAvatar>().Init(this);
//        }
    }
    #endregion
}
