using UnityEngine;
using System.Collections;

public class Abductor : Role {
//	private SpouseAbduction _spouseAbduction;

	#region getters/setters
//	public SpouseAbduction spouseAbduction {
//		get { return this._spouseAbduction; }
//	}
	#endregion
	public Abductor(Citizen citizen): base(citizen){

	}

	internal override void Initialize(GameEvent gameEvent) {
//		if (gameEvent is SpouseAbduction) {
//            base.Initialize(gameEvent);
//			this._spouseAbduction = (SpouseAbduction)gameEvent;
//			this._spouseAbduction.abductor = this;
//			this.avatar.GetComponent<AbductorAvatar>().Init(this);
//		}
	}
		
}
