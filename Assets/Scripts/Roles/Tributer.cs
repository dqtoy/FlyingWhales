using UnityEngine;
using System.Collections;

public class Tributer : Role {
	public Tributer(Citizen citizen): base(citizen){
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize (gameEvent);
		this.avatar.GetComponent<TributerAvatar>().Init(this);
	}
}
