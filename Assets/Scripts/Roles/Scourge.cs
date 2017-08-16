using UnityEngine;
using System.Collections;

public class Scourge : Role {
	private ScourgeCity _scourgeCity;

	#region getters/setters
	public ScourgeCity scourgeCity {
		get { return this._scourgeCity; }
	}
	#endregion
	public Scourge(Citizen citizen): base(citizen){

	}

	internal override void Initialize(GameEvent gameEvent) {
		if (gameEvent is ScourgeCity) {
            base.Initialize(gameEvent);
			this._scourgeCity = (ScourgeCity)gameEvent;
			this._scourgeCity.scourge = this;
            CreateAvatarGO();
			this.avatar.GetComponent<ScourgeAvatar>().Init(this);
		}
	}

//	internal override void Attack() {
//		//		base.Attack ();
//		if (this.avatar != null) {
//			this.avatar.GetComponent<ScourgeAvatar>().HasAttacked();
//			if (this.avatar.GetComponent<ScourgeAvatar>().direction == DIRECTION.LEFT) {
//				this.avatar.GetComponent<ScourgeAvatar>().animator.Play("Attack_Left");
//			} else if (this.avatar.GetComponent<ScourgeAvatar>().direction == DIRECTION.RIGHT) {
//				this.avatar.GetComponent<ScourgeAvatar>().animator.Play("Attack_Right");
//			} else if (this.avatar.GetComponent<ScourgeAvatar>().direction == DIRECTION.UP) {
//				this.avatar.GetComponent<ScourgeAvatar>().animator.Play("Attack_Up");
//			} else {
//				this.avatar.GetComponent<ScourgeAvatar>().animator.Play("Attack_Down");
//			}
//		}
//	}
}
