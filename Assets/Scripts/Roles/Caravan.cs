using UnityEngine;
using System.Collections;

public class Caravan : Role {

	public Caravan(Citizen citizen): base(citizen){
	
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize(gameEvent);
		this.avatar.GetComponent<CaravanAvatar>().Init(this);
	}
}
