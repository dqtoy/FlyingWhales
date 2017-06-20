using UnityEngine;
using System.Collections;

public class Exterminator : Role {
    private Plague _plague;

    #region getters/setters
    public Plague plague {
        get { return this._plague; }
    }
    #endregion
    public Exterminator(Citizen citizen): base(citizen){
        
    }

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Plague) {
            this._plague = (Plague)gameEvent;
            this._plague.AddAgentToList(this.citizen);
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Exterminator"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            //this.avatar.GetComponent<RaiderAvatar>().Init(this);
        }
    }

    internal override void Attack() {
        //		base.Attack ();
        if (this.avatar != null) {
            this.avatar.GetComponent<RaiderAvatar>().HasAttacked();
            if (this.avatar.GetComponent<RaiderAvatar>().direction == DIRECTION.LEFT) {
                this.avatar.GetComponent<RaiderAvatar>().animator.Play("Attack_Left");
            } else if (this.avatar.GetComponent<RaiderAvatar>().direction == DIRECTION.RIGHT) {
                this.avatar.GetComponent<RaiderAvatar>().animator.Play("Attack_Right");
            } else if (this.avatar.GetComponent<RaiderAvatar>().direction == DIRECTION.UP) {
                this.avatar.GetComponent<RaiderAvatar>().animator.Play("Attack_Up");
            } else {
                this.avatar.GetComponent<RaiderAvatar>().animator.Play("Attack_Down");
            }
        }
    }
}
