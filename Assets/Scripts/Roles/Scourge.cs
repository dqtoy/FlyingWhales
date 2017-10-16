using UnityEngine;
using System.Collections;

public class Scourge : Role {
//	private ScourgeCity _scourgeCity;

	#region getters/setters
//	public ScourgeCity scourgeCity {
//		get { return this._scourgeCity; }
//	}
	#endregion
	public Scourge(Citizen citizen): base(citizen){

	}

	internal override void Initialize(GameEvent gameEvent) {
//		if (gameEvent is ScourgeCity) {
//            base.Initialize(gameEvent);
//			this._scourgeCity = (ScourgeCity)gameEvent;
//			this._scourgeCity.scourge = this;
//            CreateAvatarGO();
//			this.avatar.GetComponent<ScourgeAvatar>().Init(this);
//		}
	}
}
