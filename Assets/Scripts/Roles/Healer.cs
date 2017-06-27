using UnityEngine;
using System.Collections;

public class Healer : Role {
    private Plague _plague;

    #region getters/setters
    public Plague plague {
        get { return this._plague; }
    }
    #endregion

    public Healer(Citizen citizen): base(citizen){

    }

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Plague) {
            base.Initialize(gameEvent);
            this._plague = (Plague)gameEvent;
            this._plague.AddAgentToList(this.citizen);
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Healer"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<HealerAvatar>().Init(this);
        }
    }

    internal override void Attack() {
        //		base.Attack ();
        if (this.avatar != null) {
            this.avatar.GetComponent<HealerAvatar>().HasAttacked();
            if (this.avatar.GetComponent<HealerAvatar>().direction == DIRECTION.LEFT) {
                this.avatar.GetComponent<HealerAvatar>().animator.Play("Attack_Left");
            } else if (this.avatar.GetComponent<HealerAvatar>().direction == DIRECTION.RIGHT) {
                this.avatar.GetComponent<HealerAvatar>().animator.Play("Attack_Right");
            } else if (this.avatar.GetComponent<HealerAvatar>().direction == DIRECTION.UP) {
                this.avatar.GetComponent<HealerAvatar>().animator.Play("Attack_Up");
            } else {
                this.avatar.GetComponent<HealerAvatar>().animator.Play("Attack_Down");
            }
        }
    }
}
