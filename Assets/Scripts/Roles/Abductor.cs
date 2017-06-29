using UnityEngine;
using System.Collections;

public class Abductor : Role {
	private SpouseAbduction _spouseAbduction;

	#region getters/setters
	public SpouseAbduction spouseAbduction {
		get { return this._spouseAbduction; }
	}
	#endregion
	public Abductor(Citizen citizen): base(citizen){

	}

	internal override void Initialize(GameEvent gameEvent) {
		if (gameEvent is SpouseAbduction) {
            base.Initialize(gameEvent);
			this._spouseAbduction = (SpouseAbduction)gameEvent;
			this._spouseAbduction.abductor = this;
			this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Abductor"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<AbductorAvatar>().Init(this);
		}
	}

	internal override void Attack() {
		//		base.Attack ();
		if (this.avatar != null) {
			this.avatar.GetComponent<AbductorAvatar>().HasAttacked();
			if (this.avatar.GetComponent<AbductorAvatar>().direction == DIRECTION.LEFT) {
				this.avatar.GetComponent<AbductorAvatar>().animator.Play("Attack_Left");
			} else if (this.avatar.GetComponent<AbductorAvatar>().direction == DIRECTION.RIGHT) {
				this.avatar.GetComponent<AbductorAvatar>().animator.Play("Attack_Right");
			} else if (this.avatar.GetComponent<AbductorAvatar>().direction == DIRECTION.UP) {
				this.avatar.GetComponent<AbductorAvatar>().animator.Play("Attack_Up");
			} else {
				this.avatar.GetComponent<AbductorAvatar>().animator.Play("Attack_Down");
			}
		}
	}
}
