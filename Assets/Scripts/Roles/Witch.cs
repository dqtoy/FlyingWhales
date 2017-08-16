using UnityEngine;
using System.Collections;

public class Witch : Role {

    public Witch(Citizen citizen) : base(citizen) {

    }

    #region Overrides
    internal override void Initialize(GameEvent gameEvent) {
        if(gameEvent is Hypnotism) {
            base.Initialize(gameEvent);
            this.avatar.GetComponent<WitchAvatar>().Init(this);
        }
    }

//    internal override void Attack() {
//        //		base.Attack ();
//        if (this.avatar != null) {
//            this.avatar.GetComponent<WitchAvatar>().HasAttacked();
//            if (this.avatar.GetComponent<WitchAvatar>().direction == DIRECTION.LEFT) {
//                this.avatar.GetComponent<WitchAvatar>().animator.Play("Attack_Left");
//            } else if (this.avatar.GetComponent<WitchAvatar>().direction == DIRECTION.RIGHT) {
//                this.avatar.GetComponent<WitchAvatar>().animator.Play("Attack_Right");
//            } else if (this.avatar.GetComponent<WitchAvatar>().direction == DIRECTION.UP) {
//                this.avatar.GetComponent<WitchAvatar>().animator.Play("Attack_Up");
//            } else {
//                this.avatar.GetComponent<WitchAvatar>().animator.Play("Attack_Down");
//            }
//        }
//    }
    #endregion
}
