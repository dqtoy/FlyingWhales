using UnityEngine;
using System.Collections;

public class Instigator : Role {
	public Instigator(Citizen citizen): base(citizen){
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize (gameEvent);
		this.avatar.GetComponent<InstigatorAvatar>().Init(this);
	}
}
