using UnityEngine;
using System.Collections;

public class Lycanthrope : Role {

    private Lycanthropy _lycanthropyEvent;

    #region getters/setters
    public Lycanthropy lycanthropyEvent {
        get { return this._lycanthropyEvent; }
    }
    #endregion
    public Lycanthrope(Citizen citizen): base(citizen){
        this.citizen.city.citizens.Remove(this.citizen);
    }

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Lycanthropy) {
            //base.Initialize(gameEvent);
            this._lycanthropyEvent = (Lycanthropy)gameEvent;
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Lycanthrope"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<LycanthropeAvatar>().Init(this);
        }
    }
    internal override void Attack() {
        //		base.Attack ();
        if (this.avatar != null) {
            this.avatar.GetComponent<LycanthropeAvatar>().HasAttacked();
            if (this.avatar.GetComponent<LycanthropeAvatar>().direction == DIRECTION.LEFT) {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Left");
            } else if (this.avatar.GetComponent<LycanthropeAvatar>().direction == DIRECTION.RIGHT) {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Right");
            } else if (this.avatar.GetComponent<LycanthropeAvatar>().direction == DIRECTION.UP) {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Up");
            } else {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Down");
            }
        }
    }
}
