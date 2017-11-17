using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class General : Role {
	internal int soldiers;

    public General(Citizen citizen): base(citizen){
		
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize(gameEvent);
		this.avatar.GetComponent<GeneralAvatar>().Init(this);
	}

	internal void SetSoldiers(int amount){
		this.soldiers = amount;
		this.avatar.GetComponent<GeneralAvatar>().UpdateUI();
	}
	internal void AdjustSoldiers(int amount){
		this.soldiers += amount;
		if(this.soldiers < 0){
			this.soldiers = 0;
		}
		this.avatar.GetComponent<GeneralAvatar>().UpdateUI();
	}
}
