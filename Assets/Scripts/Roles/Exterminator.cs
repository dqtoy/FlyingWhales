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
            this.avatar.GetComponent<ExterminatorAvatar>().Init(this);
        }
    }

    internal override void Attack() {
        //		base.Attack ();
        if (this.avatar != null) {
            this.avatar.GetComponent<ExterminatorAvatar>().HasAttacked();
            if (this.avatar.GetComponent<ExterminatorAvatar>().direction == DIRECTION.LEFT) {
                this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Left");
            } else if (this.avatar.GetComponent<ExterminatorAvatar>().direction == DIRECTION.RIGHT) {
                this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Right");
            } else if (this.avatar.GetComponent<ExterminatorAvatar>().direction == DIRECTION.UP) {
                this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Up");
            } else {
                this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Down");
            }
        }
    }
}
